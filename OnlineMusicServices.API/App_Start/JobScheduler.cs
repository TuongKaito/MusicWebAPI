using OnlineMusicServices.API.Scheduler;
using Quartz;
using Quartz.Impl;

namespace OnlineMusicServices.API
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job1 = JobBuilder.Create<RankingOfWeek>().Build();
            ITrigger trigger1 = TriggerBuilder.Create()
                .WithIdentity("RankingOfWeek", "Ranking")
                // Run at [0 second] [0 minute] [0 hour] [any day] [every month] [on Monday] [in 2017-2020]
                .WithCronSchedule("0 0 0 ? * MON 2016-2020")
                .StartNow()
                .Build();
            scheduler.ScheduleJob(job1, trigger1);

            // Update ranking again
            IJobDetail job2 = JobBuilder.Create<RankingOfWeek>().Build();
            ITrigger trigger2 = TriggerBuilder.Create()
                .WithIdentity("RankingOfWeekAgain", "Ranking")
                // Run again at [0 second] [0 minute] [12 hour] [any day] [every month] [on Monday] [in 2017-2020]
                .WithCronSchedule("0 0 12 ? * MON 2016-2020")
                .StartNow()
                .Build();
            scheduler.ScheduleJob(job2, trigger2);
        }
    }
}