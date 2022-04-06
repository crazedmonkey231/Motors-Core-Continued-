using GiddyUpCore;
using GiddyUpCore.Storage;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace motors
{
    class JobDriver_RideToJob : JobDriver
    {
        IntVec3 dest;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //For automatic mounting, reserve the mount aswell as targets of the job the pawn is riding to (target B and possibly C). 

            bool result = true;
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store == null)
            {
                return true;
            }
            ExtendedPawnData pawnData = store.GetExtendedDataFor(this.pawn);
            if (pawnData.targetJob == null)
            {
                return true;
            }
            Log.Message("pawn data : " + pawnData);
            result = ReserveUtility.ReserveEveryThingOfJob(pawnData.targetJob, this);
            pawnData.targetJob = null;
            return result;
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil goToToil = goToToil = Toils_Goto.GotoCell(TargetB.Cell, PathEndMode.Touch);

            this.AddFinishAction(delegate
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(this.pawn);
                pawnData.wasRidingToJob = true;
            });
            yield return goToToil;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntVec3>(ref dest, "dest", TargetB.Cell);
        }
    }
}
