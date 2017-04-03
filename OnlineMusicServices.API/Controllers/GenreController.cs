using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/genres")]
    public class GenreController : ApiController
    {
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllGenres()
        {
            using (var db = new OnlineMusicEntities())
            {
                var listGenres = (from genre in db.Genres
                                  select new GenreModel { Genre = genre }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listGenres);
            }
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllGenresWithCategory(string category)
        {
            using(var db = new OnlineMusicEntities())
            {
                var listGenres = (from genre in db.Genres
                                  orderby genre.Category descending
                                  where category == null || category == genre.Category
                                  select new GenreModel { Genre = genre }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listGenres);
            }
        }

        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetGenre([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var genre = (from g in db.Genres
                             where g.Id == id
                             select new GenreModel { Genre = g }).FirstOrDefault();
                if (genre == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy thể loại id=" + id);
                }
                return Request.CreateResponse(HttpStatusCode.OK, genre);
            }
        }
    }
}
