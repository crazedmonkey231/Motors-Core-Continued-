
using Verse;
using RimWorld;


namespace motors
{
    public class others
    {
        public enum Tab
        {
            Pawns,
            Items
        }

        static public float GetMass(Thing thing, IgnorePawnsInventoryMode ignorePawnInventoryMass, bool ignoreSpawnedCorpseGearAndInventoryMass)
        {
            if (thing == null)
            {
                return 0f;
            }
            float num = thing.GetStatValue(StatDefOf.Mass, true);
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignorePawnInventoryMass))
                {
                    num -= MassUtility.InventoryMass(pawn);
                }
            }
            else if (ignoreSpawnedCorpseGearAndInventoryMass)
            {
                Corpse corpse = thing as Corpse;
                if (corpse != null && corpse.Spawned)
                {
                    num -= MassUtility.GearAndInventoryMass(corpse.InnerPawn);
                }
            }
            return num;
        }


    }

}