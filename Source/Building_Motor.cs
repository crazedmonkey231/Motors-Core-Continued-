using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWorld;

namespace motors
{
    public class Building_Motor : Building
    {

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            this.Destroy();

            PawnKindDef kind = PawnKindDef.Named($"motor_{this.def.defName}");
            Pawn pawn = PawnGenerator.GeneratePawn(kind, this.Faction);
            IntVec3 loc = this.Position;
            GenSpawn.Spawn(pawn, loc, map, Rot4.Random, WipeMode.Vanish, false);
        }
    }
}
