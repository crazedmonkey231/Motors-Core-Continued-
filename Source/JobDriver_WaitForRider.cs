﻿using GiddyUpCore;
using GiddyUpCore.Jobs;
using GiddyUpCore.Storage;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace motors
{
    class JobDriver_WaitForRider : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private Pawn Followee
        {
            get
            {
                return (Pawn)TargetA;
            }
        }
        int moveInterval = Rand.Range(300, 1200);
        private JobDef initialJob;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            initialJob = Followee.CurJobDef;
            Log.Message("checking pawn is null : " + pawn.Map.ToString()); ;
            this.FailOn(() => pawn.Map == null);
            Toil firstToil = new Toil
            {
                initAction = delegate
                {
                    WalkRandomNearby();
                }
            };
            firstToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return firstToil;
            Toil toil = new Toil
            {
                tickAction = delegate
                {
                    if (this.Followee.Map == null ||
                       this.Followee.Dead ||
                       this.Followee.Downed ||
                       this.Followee.InMentalState ||
                       this.Followee.jobs.curJob.def == JobDefOf.LayDown ||
                       this.Followee.jobs.curJob.def == JobDefOf.Research)
                    {
                        this.EndJobWith(JobCondition.Incompletable);
                    }

                    if (pawn.IsHashIntervalTick(moveInterval) && !this.pawn.pather.Moving)
                    {
                        WalkRandomNearby();
                        moveInterval = Rand.Range(300, 1200);
                    }
                    if (TimeUntilExpire(pawn.CurJob) < 10 && Followee.CurJobDef == initialJob)
                    {
                        pawn.CurJob.expiryInterval += 1000;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
            toil.AddFinishAction(() =>
            {
                if (Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store)
                {
                    ExtendedPawnData animalData = store.GetExtendedDataFor(pawn);
                    ExtendedPawnData pawnData = store.GetExtendedDataFor(Followee);
                    pawnData.owning = null;
                    animalData.ownedBy = null;
                }
            });
            yield return toil;
        }

        private void WalkRandomNearby()
        {
            
            IntVec3 target = RCellFinder.RandomWanderDestFor(Followee, this.Followee.Position, 8, ((Pawn p, IntVec3 loc, IntVec3 root) => true), Danger.Some);
            this.pawn.pather.StartPath(target, PathEndMode.Touch);
        }

        private int TimeUntilExpire(Job job)
        {
            return job.expiryInterval - (Find.TickManager.TicksGame - job.startTick);
        }
    }
}
