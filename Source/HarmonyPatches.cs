using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
using RimWorld.Planet;
using System.Linq;
using UnityEngine;
using Verse.AI;
using System.Globalization;
using GiddyUpCore.Utilities;

namespace motors
{

	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{

		static HarmonyPatches()
		{

			Harmony harmony = new Harmony("com.crazedmonkey231.motors");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

		}

    }


    [HarmonyPatch(typeof(JobDriver_Mounted), "FinishAction")]
    class JobDriver_Mounted_FinishAction
    {
        static void Postfix(JobDriver_Mounted __instance)
        {
            ExtendedPawnData pawnData = GiddyUpCore.Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(__instance.pawn);

            if (!__instance.Rider.Drafted && __instance.pawn.Faction == Faction.OfPlayer)
            {
                if (pawnData.ownedBy != null && !__instance.interrupted && __instance.Rider.GetCaravan() == null)
                {
                    __instance.pawn.jobs.jobQueue.EnqueueFirst(new Job(RideOrWait_DefOf.WaitForRider, pawnData.ownedBy)
                    {
                        //expiryInterval = 1,
                        //checkOverrideOnExpire = true,
                        //followRadius = 8,
                        //locomotionUrgency = LocomotionUrgency.Walk

                        expiryInterval = 10000,
                        checkOverrideOnExpire = true,
                        followRadius = 8,
                        locomotionUrgency = LocomotionUrgency.Walk
                    }
                    ); //follow the rider for a while to give it an opportunity to take a ride back.  
                }
            }


            Log.Message("Creating job");
            if (__instance.pawn.def.defName.ToLower().Contains("motor_"))
            {
                __instance.pawn.jobs.jobQueue.EnqueueLast(new Job(RideOrWait_DefOf.RideToJob));
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "ChoicesAtFor")]
    static class FloatMenuMakerMap_ChoicesAtFor
    {
        static void Postfix(Vector3 clickPos, Pawn pawn, ref List<FloatMenuOption> __result)
        {
            __result.RemoveDuplicates<FloatMenuOption>();
        }
    }

    [HarmonyPatch(typeof(GUC_FloatMenuUtility), "AddMountingOptions")]
    class GUC_FloatMenuUtility_patched
    {
        static void Postfix(Pawn target, Pawn pawn, List<FloatMenuOption> opts)
        {

            var pawnData = GiddyUpCore.Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            if (pawnData.mount == null)
            {
                Action action = delegate
                {
                    if (target.Faction == Faction.OfPlayer)
                    {
                        if (!pawn.Drafted)
                        {
                            pawn.drafter.Drafted = true;
                        }
                        if (target.drafter != null && target.Drafted)
                        {
                            target.drafter.Drafted = false;
                        }
                    }
                    Job jobRider = new Job(GUC_JobDefOf.Mount, target);
                    jobRider.count = 1;
                    pawn.jobs.TryTakeOrderedJob(jobRider);
                };
                opts.Add(new FloatMenuOption("GUC_Mount".Translate() + " " + target.Name, action, MenuOptionPriority.Low));
                //opts.Replace(opts.Find(e => e.Label.Contains("GUC_Mount")), new FloatMenuOption("GUC_Mount".Translate() + " " + target.Name, action, MenuOptionPriority.Low));
            }
            else if (target == pawnData.mount)
            {
                Action action = delegate
                {
                    pawnData.reset();
                };
                opts.Add(new FloatMenuOption("GUC_Dismount".Translate(), action, MenuOptionPriority.High));
                //opts.Replace(opts.Find(e => e.Label.Contains("GUC_Dismount")), new FloatMenuOption("GUC_Dismount".Translate(), action, MenuOptionPriority.High));
            }
        }
    }

   
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddDraftedOrders")]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>), typeof(bool) })]
    static class FloatMenuMakerMap_AddDraftedOrders
    {
        static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts, bool suppressAutoTakeableGoto)
        {

            foreach (LocalTargetInfo current in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackAny(), true))
            {

                if ((current.Thing is Pawn target) && target.def.defName.ToLower().Contains("motor_") && target.GetComp<CompMotor>() != null)
                {
                    if (target.GetComp<CompMotor>() != null && target.GetComp<CompRefuelable>() == null)
                    {
                        GUC_FloatMenuUtility.AddMountingOptions(target, pawn, opts);

                    }else if (target.GetComp<CompRefuelable>() != null && target.GetComp<CompRefuelable>().Fuel > 0)
                    {
                        GUC_FloatMenuUtility.AddMountingOptions(target, pawn, opts);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddUndraftedOrders")]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Pawn), typeof(List<FloatMenuOption>) })]
    static class FloatMenuMakerMap_AddUndraftedOrders
    {
        static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {

            foreach (LocalTargetInfo current in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackAny(), true))
            {
                if ((current.Thing is Pawn target) && target.def.defName.ToLower().Contains("motor_"))
                {
                    if (target.GetComp<CompMotor>() != null && target.GetComp<CompRefuelable>() == null)
                    {
                        GiddyUpCore.Utilities.GUC_FloatMenuUtility.AddMountingOptions(target, pawn, opts);

                    }else if (target.GetComp<CompRefuelable>() != null && target.GetComp<CompRefuelable>().Fuel > 0)
                    {
                        GiddyUpCore.Utilities.GUC_FloatMenuUtility.AddMountingOptions(target, pawn, opts);
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Pawn_DrawTracker), "get_DrawPos")]
    static class Pawn_DrawTracker_get_DrawPos
    {

        static void Postfix(Pawn_DrawTracker __instance, ref Vector3 __result, ref Pawn ___pawn)
        {
            if (__instance == null || __result == null || ___pawn == null) return;

            ExtendedDataStorage store = GiddyUpCore.Base.Instance.GetExtendedDataStorage();
            ExtendedPawnData pawnData = store.GetExtendedDataFor(___pawn);

            if (pawnData != null && pawnData.mount != null && pawnData.mount.GetComp<CompMotor>() != null && pawnData.mount.def.HasModExtension<DrawingOffsetPatch>())
            {

                __result += AddCustomOffsets(___pawn, pawnData);

            }
        }

        private static Vector3 AddCustomOffsets(Pawn __instance, ExtendedPawnData pawnData)
        {
            DrawingOffsetPatch customOffsets = pawnData.mount.def.GetModExtension<DrawingOffsetPatch>();
            if (__instance.Rotation == Rot4.North)
            {
                return customOffsets.northOffset;
            }
            if (__instance.Rotation == Rot4.South)
            {
                return customOffsets.southOffset;
            }
            if (__instance.Rotation == Rot4.East)
            {
                return customOffsets.eastOffset;
            }
            return customOffsets.westOffset;
        }
    }

    [HarmonyPatch(typeof(CaravanUIUtility), "AddPawnsSections")]
    internal static class CaravanUIUtility_AddPawnsSections_patch
    {
        [HarmonyPrefix]
        private static bool Prefix(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
        {

            IEnumerable<TransferableOneWay> source = from x in transferables
                                                     where x.ThingDef.category == ThingCategory.Pawn
                                                     select x;

            widget.AddSection("ColonistsSection".Translate(), from x in source
                                                              where ((Pawn)x.AnyThing).IsFreeColonist
                                                              select x);
            widget.AddSection("PrisonersSection".Translate(), from x in source
                                                              where ((Pawn)x.AnyThing).IsPrisoner
                                                              select x);
            widget.AddSection("CaptureSection".Translate(), from x in source
                                                            where ((Pawn)x.AnyThing).Downed && CaravanUtility.ShouldAutoCapture((Pawn)x.AnyThing, Faction.OfPlayer)
                                                            select x);
            
            widget.AddSection("VehicleSection".Translate(), from x in source
                                                            where ((Pawn)x.AnyThing).RaceProps.IsMechanoid
                                                            select x);
            widget.AddSection("AnimalsSection".Translate(), from x in source
                                                            where ((Pawn)x.AnyThing).RaceProps.Animal
                                                            select x);



            return false;
        }


    }
}
