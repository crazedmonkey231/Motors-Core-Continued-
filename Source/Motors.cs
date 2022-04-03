using GiddyUpCore;

using HugsLib;
using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace motors
{
    public class motors : ModBase
    {
        public override string ModIdentifier => "com.crazedmonkey231.motors";

        public static bool GiddyUpMechanoidsLoaded = false;
        public static motors Instance { get; private set; }

        public static SettingHandle<DictAnimalRecordHandler> mechSelector;
        private static Color highlight1 = new Color(0.5f, 0, 0, 0.1f);
        internal static SettingHandle<float> bodySizeFilter;

        public motors()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();

            foreach (ThingDef td in getAllMotors())
            {
                if (td.HasModExtension<DrawingOffsetPatch>())
                {
                    td.GetModExtension<DrawingOffsetPatch>().Init();
                }
            }
            
        }


        private bool AssemblyExists(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith(assemblyName))
                    return true;
            }
            return false;
        }

        private static List<ThingDef> getAllMotors()
        {
            List<ThingDef> ar_mech = new List<ThingDef>();
            foreach (ThingDef t in from thing in DefDatabase<ThingDef>.AllDefs
                                   where thing.race != null && thing.race.IsMechanoid
                                   && thing.defName.ToLower().Contains("motor_")
                                         select thing)
            {
                ar_mech.Add(t);
            }
            return ar_mech;
        }


    }

}