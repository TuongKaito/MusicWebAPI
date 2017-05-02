using OnlineMusicServices.Data;
using Quartz;
using System;
using System.IO;
using System.Web.Hosting;

namespace OnlineMusicServices.API.Scheduler
{
    public class RankingOfWeek : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                try
                {
                    db.UpdateSongRanking(DateTime.Now.Date);
                    Log("Updated song ranking");
                }
                catch (Exception e)
                {
                    Log("Failed to update song ranking: " + e.InnerException?.Message);
                }

                try
                {
                    db.UpdateAlbumRanking(DateTime.Now.Date);
                    Log("Updated album ranking");
                }
                catch (Exception e)
                {
                    Log("Failed to update album ranking: " + e.InnerException?.Message);
                }
            }
        }

        public void Log(string text)
        {
            string path = HostingEnvironment.MapPath("~/Scheduler/log_scheduler.txt");
            StreamWriter writer = new StreamWriter(path, true);
            try
            {
                writer.WriteLine(DateTime.Now.ToString() + "\t" + text);
            }
            catch
            {

            }
            finally
            {
                writer.Close();
            }
        }
    }
}