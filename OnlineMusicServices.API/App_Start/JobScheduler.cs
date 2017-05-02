using System;
using System.IO;
using System.Web.Hosting;
using OnlineMusicServices.API.Scheduler;
using Quartz.Impl;
using Quartz;
using System.Net.Http;
using System.Web;
using OnlineMusicServices.API.Controllers;
using System.Threading.Tasks;

namespace OnlineMusicServices.API
{
    public class JobScheduler
    {
        public static void Start()
        {
            #region Quartz
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job1 = JobBuilder.Create<RankingOfWeek>().Build();
            ITrigger trigger1 = TriggerBuilder.Create()
                .WithIdentity("RankingOfWeek", "Ranking")
                // Run at [0 second] [0 minute] [0 hour] [any day] [every month] [on Monday] [every year]
                .WithCronSchedule("0 0 0 ? * MON *")
                .StartNow()
                .Build();
            scheduler.ScheduleJob(job1, trigger1);

            // Update ranking again
            IJobDetail job2 = JobBuilder.Create<RankingOfWeek>().Build();
            ITrigger trigger2 = TriggerBuilder.Create()
                .WithIdentity("RankingOfWeekAgain", "Ranking")
                // Run again at [0 second] [0 minute] [12 hour] [any day] [every month] [on Monday] [every year]
                .WithCronSchedule("0 0 12 ? * MON *")
                .StartNow()
                .Build();
            scheduler.ScheduleJob(job2, trigger2);

            IJobDetail job3 = JobBuilder.Create<Ping>().Build();
            ITrigger trigger3 = TriggerBuilder.Create()
                .WithIdentity("PingToHost", "Ping")
                // Ping to host every 5 minutes
                .WithCronSchedule("0 0/5 * ? * * *")
                .StartNow()
                .Build();
            scheduler.ScheduleJob(job3, trigger3);
            #endregion                        
        }
    }

    class Ping : IJob
    {

        public void Execute(IJobExecutionContext context)
        {
            PingToHost();
        }

        public void Log(string text)
        {
            string path = HostingEnvironment.MapPath("~/Scheduler/log_scheduler.txt");
            StreamWriter writer = new StreamWriter(path, true);
            try
            {
                writer.WriteLine("Ping failed!");
                writer.WriteLine(DateTime.Now.ToString() + "\t" + text);
                writer.WriteLine("---------------------------------------------");
            }
            catch
            {

            }
            finally
            {
                writer.Close();
            }
        }

        public async Task PingToHost()
        {
            if (RootController.HostUrl != null)
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        client.BaseAddress = new Uri(RootController.HostUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage res = await client.GetAsync("/");
                    }
                    catch(Exception e)
                    {
                        Log(e.Message);
                    }
                }
            }
        }
    }
}