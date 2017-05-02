using OnlineMusicServices.API.DTO;
using OnlineMusicServices.API.Models;
using OnlineMusicServices.API.Storage;
using OnlineMusicServices.API.Utility;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/albums")]
    public class AlbumController : ApiController
    {
        GoogleDriveServices services;
        AlbumDTO dto;
        SongDTO songDto;
        CommentDTO commentDto;
        RankingAlbumDTO rankingAlbumDto;

        public AlbumController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
            Uri uri = HttpContext.Current.Request.Url;
            dto = new AlbumDTO(uri);
            songDto = new SongDTO(uri);
            commentDto = new CommentDTO(uri);
            rankingAlbumDto = new RankingAlbumDTO(uri);
        }

        #region Album Services
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllAlbums()
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db, null, true);
                var listAlbums = query.OrderByDescending(a => a.Id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listAlbums);
            }
        }
        
        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetAlbum([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db);
                var album = query.Where(a => a.Id == id).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + id);
                }
                return Request.CreateResponse(HttpStatusCode.OK, album);
            }
        }

        [Route("pager")]
        [HttpGet]
        public HttpResponseMessage GetPagingAlbums(int page = 1, int size = 0)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db, album => album.Songs.Count > 0);
                List<AlbumModel> listAlbums;
                if (size > 0)
                {
                    listAlbums = query.OrderBy(a => a.Title).Skip((page - 1) * size).Take(size).ToList();
                }
                else
                {
                    listAlbums = query.OrderBy(a => a.Title).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, listAlbums);
            }
        }

        [Route("latest")]
        [HttpGet]
        public HttpResponseMessage GetLatestAlbums(int page = 1, int size = 0)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db);
                List<AlbumModel> listAlbums;
                if (size > 0)
                {
                    listAlbums = query.OrderByDescending(a => a.ReleasedDate).Skip((page - 1) * size).Take(size).ToList();
                }
                else
                {
                    listAlbums = query.OrderByDescending(a => a.ReleasedDate).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, listAlbums);
            }
        }

        [Route("popular")]
        [HttpGet]
        public HttpResponseMessage GetPopularAlbums(int page = 1, int size = 0)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db);
                List<AlbumModel> listAlbums;
                if (size > 0)
                {
                    listAlbums = query.OrderByDescending(a => a.Views).Skip((page - 1) * size).Take(size).ToList();
                }
                else
                {
                    listAlbums = query.OrderByDescending(a => a.Views).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, listAlbums);
            }
        }

        #region Ranking Album

        [Route("ranking")]
        [HttpGet]
        public HttpResponseMessage GetAllRankingSongs()
        {
            using (var db = new OnlineMusicEntities())
            {
                var rankingList = rankingAlbumDto.GetQueryRanking(db).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, rankingList);
            }
        }

        [Route("ranking/{year}/{week}")]
        [HttpGet]
        public HttpResponseMessage GetRankingSongs([FromUri] int year, [FromUri] int week)
        {
            using (var db = new OnlineMusicEntities())
            {
                var rankingList = rankingAlbumDto.GetQueryRanking(db, r => r.StartDate.Year == year && r.Week == week).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, rankingList);
            }
        }

        [Route("ranking/{year}/{month}/{day}")]
        [HttpGet]
        public HttpResponseMessage GetRankingSongs([FromUri] int year, [FromUri] int month, [FromUri] int day)
        {
            using (var db = new OnlineMusicEntities())
            {
                DateTime rankingDate = new DateTime(year, month, day);
                var rankingList = rankingAlbumDto.GetQueryRanking(db , r => r.StartDate.Equals(rankingDate)).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, rankingList);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("ranking/{year}/{month}/{day}")]
        [HttpPut]
        public HttpResponseMessage UpdateRanking([FromUri] int year, [FromUri] int month, [FromUri] int day)
        {
            using (var db = new OnlineMusicEntities())
            {
                try
                {
                    DateTime updateDate = new DateTime(year, month, day);
                    db.UpdateAlbumRanking(updateDate);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                catch (Exception e)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
                }
            }
        }

        #endregion Ranking Album

        [Authorize(Roles = "Admin")]
        [Route("")]
        [HttpPost]
        public HttpResponseMessage CreateAlbum([FromBody] AlbumModel albumModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var album = new Album();
                albumModel.UpdateEntity(album);
                album.Photo = GoogleDriveServices.DEFAULT_ALBUM;
                db.Albums.Add(album);
                db.SaveChanges();
                db.Entry(album).Reference(a => a.Genre).Load();
                db.Entry(album).Reference(a => a.Artist).Load();

                albumModel = dto.Converter(db.Albums.Where(a => a.Id == album.Id).FirstOrDefault());
                return Request.CreateResponse(HttpStatusCode.Created, albumModel);
            }
        }

        /// <summary>
        /// Upload album photo to server
        /// </summary>
        /// <param name="id">Album id</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [Route("{id}/upload")]
        [HttpPost]
        public HttpResponseMessage UploadPhoto([FromUri] int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType));
            }
            using (var db = new OnlineMusicEntities())
            {
                // Query artist in database and check artist is existed
                var album = (from a in db.Albums
                              where a.Id == id
                              select a).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + id);
                }

                var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
                if (file != null && file.ContentLength > 0)
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            #region Upload file to drive
                            // Setup photo uploaded path
                            var ext = Path.GetExtension(file.FileName).ToLower();
                            var fileName = album.Title + ext;
                            var folderId = services.SearchFolder(album.Artist.FullName, GoogleDriveServices.ALBUMS) ??
                                services.CreateFolder(album.Artist.FullName, GoogleDriveServices.ALBUMS);

                            Stream scaledImge = ImageFactory.Resize(file.InputStream);

                            // Photo will upload in Images/Albums/{artistFullName}/{fileName}
                            var resourceId = services.UploadFile(scaledImge, fileName, Media.GetMediaTypeFromExtension(ext), folderId);
                            if (resourceId == null)
                            {
                                transaction.Rollback();
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Upload thất bại");
                            }
                            else
                            {
                                // Add new resource
                                Resource resource = new Resource()
                                {
                                    Id = resourceId,
                                    Name = fileName,
                                    Type = (int)ResourceTypeManager.Image
                                };
                                db.Resources.Add(resource);
                                db.SaveChanges();

                                // Update artist photo resource
                                album.Photo = resourceId;
                                db.SaveChanges();
                                transaction.Commit();
                                return Request.CreateResponse(HttpStatusCode.OK, String.Format("Upload hình ảnh cho album {0} thành công", album.Title));
                            }
                            #endregion Upload file to drive
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                        }
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Dữ liệu upload không hợp lệ");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("")]
        [HttpPut]
        public HttpResponseMessage UpdateAlbum([FromBody] AlbumModel albumModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var album = (from a in db.Albums
                             where a.Id == albumModel.Id
                             select a).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + albumModel.Id);
                }

                albumModel.UpdateEntity(album);
                db.SaveChanges();
                albumModel = dto.Converter(album);
                return Request.CreateResponse(HttpStatusCode.OK, albumModel);
            }
        }

        /// <summary>
        /// Increase view for a album by 1
        /// </summary>
        /// <param name="id">Id of album need to increase</param>
        /// <returns></returns>
        [Route("{id}/increase-view")]
        [HttpPut]
        public HttpResponseMessage IncreaseView([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var album = (from a in db.Albums
                             where a.Id == id
                             select a).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + id);
                }
                else if (album.Songs.Count > 0)
                {
                    var views = (from v in db.AlbumViews where v.AlbumId == id select v).FirstOrDefault();
                    // Create new if song hasn't view yet
                    if (views == null)
                    {
                        views = new AlbumView() { Ip = "", AlbumId = id, Timestamp = DateTime.Now, Views = 0 };
                        db.AlbumViews.Add(views);
                    }
                    else
                    {
                        // Reset view hour
                        if (views.Timestamp.Date.CompareTo(DateTime.Now.Date) != 0 || views.Timestamp.Hour != DateTime.Now.Hour)
                        {
                            views.Ip = "";
                            views.Timestamp = DateTime.Now;
                        }
                    }

                    string ip = HttpContext.Current.Request.UserHostAddress.Trim();
                    // If IP hasn't view yet then increase view of song
                    if (!views.Ip.Contains(ip))
                    {
                        views.Ip += " " + ip;
                        views.Views++;
                    }

                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteAlbum([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var album = (from a in db.Albums
                             where a.Id == id
                             select a).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + id);
                }
                db.Albums.Remove(album);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Album Services

        #region Songs of Album

        [Route("{id}/songs")]
        [HttpGet]
        public HttpResponseMessage GetSongsOfAlbum([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var album = (from a in db.Albums
                                 where a.Id == id
                                 select a).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + id);
                }
                var listSongs = songDto.ConvertToSongModel(album.Songs.Where(s => s.Verified == true && s.Privacy == false && s.Official == true).ToList());
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("{id}/songs")]
        [HttpPut]
        public HttpResponseMessage AddSongToAlbum([FromUri] int id, [FromBody] ICollection<SongModel> listSongs, bool notify = false)
        {
            using (var db = new OnlineMusicEntities())
            {
                var album = (from a in db.Albums
                             where a.Id == id
                             select a).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + id);
                }
                album.Songs.Clear();
                foreach(var song in listSongs)
                {
                    var sg = (from s in db.Songs
                              where s.Id == song.Id
                              select s).FirstOrDefault();
                    if (sg != null)
                        album.Songs.Add(sg);
                }
                // List songs add to album invalid
                if (album.Songs.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Danh sách bài hát không hợp lệ");
                }
                db.SaveChanges();

                //Push notification
                if (notify)
                {
                    try
                    {
                        List<Notification> notifications = new List<Notification>();
                        foreach(User user in album.Artist.Users)
                        {
                            Notification notification = new Notification()
                            {
                                Title = "Hệ thống",
                                Message = $"{album.Artist.FullName} ra mắt album {album.Title}",
                                CreatedAt = DateTime.Now,
                                IsMark = false,
                                UserId = user.Id,
                                Action = NotificationAction.ALBUM_RELEASED
                            };
                            notifications.Add(notification);
                        }
                        if (notifications.Count > 0)
                        {
                            db.Notifications.AddRange(notifications);
                            db.SaveChanges();
                        }
                    }
                    catch
                    {

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("{id}/songs/clear")]
        [HttpPut]
        public HttpResponseMessage ClearAlbum([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var album = (from a in db.Albums
                             where a.Id == id
                             select a).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + id);
                }

                album.Songs.Clear();
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Songs of Album

        #region Comment of Album

        [Route("{id}/comments")]
        [HttpGet]
        public HttpResponseMessage GetCommentsOfAlbum([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var listComments = commentDto.GetCommentQuery(db, (AlbumComment c) => c.AlbumId == id)
                    .OrderByDescending(c => c.Date).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listComments);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPost]
        public HttpResponseMessage AddCommentToAlbum([FromUri] int id, [FromBody] CommentAlbumModel commentModel)
        {
            if (commentModel.DataId != id)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Dữ liệu không phù hợp");
            }
            using (var db = new OnlineMusicEntities())
            {
                AlbumComment comment = new AlbumComment();
                commentModel.UpdateEntity(comment);
                comment.Date = DateTime.Now;
                db.AlbumComments.Add(comment);
                db.SaveChanges();

                comment.User = (from u in db.Users where u.Id == comment.UserId select u).FirstOrDefault();
                commentModel = commentDto.GetCommentQuery(db, awhereClause: null).Where(c => c.Id == comment.Id).FirstOrDefault();
                
                return Request.CreateResponse(HttpStatusCode.Created, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPut]
        public HttpResponseMessage EditComment([FromUri] int id, [FromBody] CommentAlbumModel commentModel)
        {
            if (commentModel.DataId != id)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Dữ liệu không phù hợp");
            }
            using (var db = new OnlineMusicEntities())
            {
                AlbumComment comment = (from c in db.AlbumComments
                                        where c.Id == commentModel.Id
                                        select c).FirstOrDefault();
                if (comment == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Không tìm thấy comment id=" + commentModel.Id);
                }
                commentModel.UpdateEntity(comment);
                db.SaveChanges();
                commentModel = commentDto.GetCommentQuery(db, awhereClause: null).Where(c => c.Id == comment.Id).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{albumId}/comments/{commentId}")]
        [HttpDelete]
        public HttpResponseMessage DeleteComment([FromUri] int albumId, [FromUri] long commentId)
        {
            using (var db = new OnlineMusicEntities())
            {
                AlbumComment comment = (from c in db.AlbumComments
                                        where c.Id == commentId
                                        select c).FirstOrDefault();
                if (comment == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Không tìm thấy comment id=" + commentId);
                }
                db.AlbumComments.Remove(comment);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Comment of Album

    }
}
