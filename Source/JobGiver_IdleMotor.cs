using System;
using RimWorld;
using Verse.AI;
using Verse;


namespace motors
{
    public class JobGiver_IdleMotor : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = JobMaker.MakeJob(JobDefOf.Wait);
            job.expiryInterval = 100;
            return job;
        }

        private const int WaitTime = 100;
    }
}
