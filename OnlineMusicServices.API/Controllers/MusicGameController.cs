using OnlineMusicServices.API.DTO;
using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/games/music-quiz")]
    public class MusicGameController : ApiController
    {
        private ScoreDTO dto;
        private SongDTO songDto;

        public MusicGameController()
        {
            Uri uri = HttpContext.Current.Request.Url;
            dto = new ScoreDTO(uri);
            songDto = new SongDTO(uri);
        }

        [Route("scores")]
        [HttpGet]
        public HttpResponseMessage GetAllScore()
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                var list = dto.GetScoreQuery(db).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [Route("highscores")]
        [HttpGet]
        public HttpResponseMessage GetHighScore(int size = 10)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                var list = dto.GetScoreQuery(db)
                    .OrderByDescending(s => s.Score).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [Route("resources")]
        [HttpGet]
        public HttpResponseMessage GetResources()
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                var list = songDto.GetSongQuery(db, s => s.Privacy == false && s.Verified == true && s.Official == true).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [Authorize(Roles = "User")]
        [Route("scores")]
        [HttpPost]
        public HttpResponseMessage AddScore([FromBody] ScoreModel scoreModel)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users where u.Id == scoreModel.UserId select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + scoreModel.UserId);
                }
                Score score = (from s in db.Scores where s.UserId == user.Id select s).FirstOrDefault();
                if (score == null)
                {
                    score = new Score();
                    db.Scores.Add(score);
                }
                scoreModel.UpdateEntity(score);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
