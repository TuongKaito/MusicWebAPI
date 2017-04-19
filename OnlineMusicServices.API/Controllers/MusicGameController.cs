using OnlineMusicServices.API.DTO;
using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
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

        [Route("scores/users/{userId}")]
        [HttpGet]
        public HttpResponseMessage GetScore([FromUri] int userId)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                User user = (from u in db.Users where u.Id == userId select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + userId);
                }
                Score score = (from s in db.Scores where s.UserId == userId select s).FirstOrDefault();
                if (score == null)
                {
                    score = new Score();
                    score.Score1 = 0;
                    score.UserId = userId;
                    db.Scores.Add(score);
                    db.SaveChanges();
                }
                db.Entry(score).Reference(s => s.User).Load();
                ScoreModel model = dto.GetScoreQuery(db, s => s.UserId == userId).FirstOrDefault(); 
                
                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
        }

        [Route("highscore")]
        [HttpGet]
        public HttpResponseMessage GetHighScore(int size = 10)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                var list = dto.GetScoreQuery(db)
                    .OrderByDescending(s => s.Score).ThenBy(s => s.Id).Take(size).ToList();
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

        [Route("resources")]
        [HttpGet]
        public HttpResponseMessage GetResourcesByGenre(int genre = 1)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                var list = songDto.GetSongQuery(db, s => s.Privacy == false && s.Verified == true && s.Official == true && s.GenreId == genre).ToList();
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

                var identity = (ClaimsIdentity)User.Identity;
                if (identity.Name != scoreModel.UserId.ToString())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Token");
                }

                Score score = (from s in db.Scores where s.UserId == user.Id select s).FirstOrDefault();
                if (score == null)
                {
                    score = new Score();
                    db.Scores.Add(score);
                }
                // Update new score if new score greater than old one
                if (scoreModel.Score > score.Score1)
                {
                    scoreModel.UpdateEntity(score);
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
