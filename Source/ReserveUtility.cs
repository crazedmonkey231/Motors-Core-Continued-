using Verse;
using Verse.AI;

namespace motors
{
    class ReserveUtility
    {
        public static bool ReserveEveryThingOfJob(Job targetJob, JobDriver jobDriver)
        {
            int targetACount = 1;
            int targetBCount = 1;
            bool result = true;

            if (!targetJob.targetQueueA.NullOrEmpty())
            {
                jobDriver.pawn.ReserveAsManyAsPossible(targetJob.targetQueueA, jobDriver.job, 1, -1, null);
            }
            if (!targetJob.targetQueueB.NullOrEmpty())
            {
                jobDriver.pawn.ReserveAsManyAsPossible(targetJob.targetQueueB, jobDriver.job, 1, -1, null);
            }

            if (jobDriver.job.targetA != null)
            {
                result &= jobDriver.pawn.Reserve(jobDriver.job.targetA, jobDriver.job, 1, -1, null);
            }
            if (targetJob.targetA != null)
            {
                result &= jobDriver.pawn.Reserve(targetJob.targetA, jobDriver.job, targetACount, targetACount > 1 ? 0 : -1, null);
            }
            if (targetJob.targetB != null)
            {
                result &= jobDriver.pawn.Reserve(targetJob.targetB, jobDriver.job, targetBCount, targetBCount > 1 ? 0 : -1, null);
            }
            if (targetJob.targetC != null)
            {
                result &= jobDriver.pawn.Reserve(targetJob.targetC, jobDriver.job, 1, -1, null);
            }
            return result;
        }
    }
}
