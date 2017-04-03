using OnlineMusicServices.API.DTO;
using OnlineMusicServices.API.Models;
using OnlineMusicServices.API.Storage;
using OnlineMusicServices.API.Utility;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/songs")]
    public class SongController : ApiController
    {
        GoogleDriveServices services;
        SongDTO dto;

        public SongController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
            dto = new SongDTO(HttpContext.Current.Request.Url);
        }

        #region Song Services
        /// <summary>
        /// Get list songs from database
        /// </summary>
        /// <param name="verified">Get list songs is verified. Null to get all list</param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllSongs(string type = "audio")
        {
            using (var db = new OnlineMusicEntities())
            {
                IQueryable<SongModel> query = null;
                if (String.Compare(type, "audio", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Audio);
                }
                else if (String.Compare(type, "video", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Video);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Resource type not supported");
                }
                ICollection<SongModel> listSongs;
                listSongs = query.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        /// <summary>
        /// Get list songs from database
        /// </summary>
        /// <param name="verified">Get list songs is verified. Null to get all list</param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllSongsVerified(bool verified, string type = "audio")
        {
            using (var db = new OnlineMusicEntities())
            {
                IQueryable<SongModel> query = null;
                if (String.Compare(type, "audio", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Audio);
                }
                else if (String.Compare(type, "video", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Video);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Resource type not supported");
                }
                ICollection<SongModel> listSongs;
                listSongs = query.Where(s => s.Verified == verified && !String.IsNullOrEmpty(s.ResourceId)).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        /// <summary>
        /// Get list songs with paging option
        /// </summary>
        /// <param name="page">Number of page to get</param>
        /// <param name="size">Number of elements in a page</param>
        /// <returns></returns>
        [Route("pager")]
        [HttpGet]
        public HttpResponseMessage GetPagingSongs(int page = 1, int size = 200, bool verified = true, string type = "audio")
        {
            using (var db = new OnlineMusicEntities())
            {
                IQueryable<SongModel> query = null;
                if (String.Compare(type, "audio", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Audio);
                }
                else if (String.Compare(type, "video", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Video);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Resource type not supported");
                }
                var listSongs = query.Where(s => !String.IsNullOrEmpty(s.ResourceId) && s.Verified == verified)
                                     .OrderByDescending(s => s.Title)
                                     .Skip((page - 1) * size).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        [Route("latest")]
        [HttpGet]
        public HttpResponseMessage GetLatestSongs(int page = 1, int size = 200, string type = "audio")
        {
            using (var db = new OnlineMusicEntities())
            {
                IQueryable<SongModel> query = null;
                if (String.Compare(type, "audio", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Audio);
                }
                else if (String.Compare(type, "video", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Video);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Resource type not supported");
                }
                var listSongs = query.Where(s => !String.IsNullOrEmpty(s.ResourceId) && s.Verified == true)
                                     .OrderByDescending(s => s.UploadedDate)
                                     .Skip((page - 1) * size)
                                     .Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        [Route("popular")]
        [HttpGet]
        public HttpResponseMessage GetPopularSongs(int page = 1, int size = 200, string type = "audio")
        {
            using (var db = new OnlineMusicEntities())
            {
                IQueryable<SongModel> query = null;
                if (String.Compare(type, "audio", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Audio);
                }
                else if (String.Compare(type, "video", true) == 0)
                {
                    query = dto.GetSongQuery(db, (song) => song.Resource.Type == (int)ResourceTypeManager.Video);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Resource type not supported");
                }
                var listSongs = query.Where(s => !String.IsNullOrEmpty(s.ResourceId) && s.Verified == true)
                                     .OrderByDescending(s => s.Views)
                                     .Skip((page - 1) * size)
                                     .Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        /// <summary>
        /// Get an song specified id
        /// </summary>
        /// <param name="id">Id of song</param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetSong([FromUri] long id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetSongQuery(db);
                var song = query.Where(s => !String.IsNullOrEmpty(s.ResourceId)).Where(s => s.Id == id).FirstOrDefault();
                if (song == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy media id=" + id);
                }
                return Request.CreateResponse(HttpStatusCode.OK, song);
            }
        }

        /// <summary>
        /// Upload song photo to server
        /// </summary>
        /// <param name="id">Song id</param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("{id}/upload")]
        [HttpPost]
        public HttpResponseMessage UploadSong([FromUri] long id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType));
            }
            using (var db = new OnlineMusicEntities())
            {
                // Check if song existed in database
                var song = (from s in db.Songs
                            join u in db.Users on s.AuthorId equals u.Id
                            where s.Id == id
                            select s).FirstOrDefault();
                if (song == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy media id=" + id);
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
                            // Path.GetExtension return .{ext}
                            var ext = Path.GetExtension(file.FileName).ToLower();
                            var fileName = song.Title + ext;
                            ResourceTypeManager resourceType = Media.GetResourceType(ext);
                            string folderId;
                            if (resourceType == ResourceTypeManager.Video)
                            {
                                folderId = services.SearchFolder(song.User.Username, GoogleDriveServices.VIDEOS) ??
                                    services.CreateFolder(song.User.Username, GoogleDriveServices.VIDEOS);
                            }
                            else
                            {
                                folderId = services.SearchFolder(song.User.Username, GoogleDriveServices.MUSICS) ??
                                    services.CreateFolder(song.User.Username, GoogleDriveServices.MUSICS);
                            }

                            // Photo will upload in Musics/{username}/{fileName}
                            var resourceId = services.UploadFile(file.InputStream, fileName, Media.GetMediaTypeFromExtension(ext), folderId);
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
                                    Type = (int)resourceType
                                };
                                db.Resources.Add(resource);
                                db.SaveChanges();

                                // Update song resource
                                song.ResourceId = resourceId;
                                db.SaveChanges();
                                transaction.Commit();
                                return Request.CreateResponse(HttpStatusCode.OK, String.Format("Upload media {0} thành công", song.Title));
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

        /// <summary>
        /// Create a new song
        /// </summary>
        /// <param name="songModel">Song need to insert</param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("")]
        [HttpPost]
        public HttpResponseMessage CreateSong([FromBody] SongModel songModel)
        {
            if (songModel.Artists.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Phải có ít nhất 1 nghệ sĩ");
            }
            using (var db = new OnlineMusicEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var song = new Song();
                        var query = dto.GetSongQuery(db);

                        // Update artists of song
                        foreach (var artist in songModel.Artists)
                        {
                            var art = (from a in db.Artists
                                       where a.Id == artist.Id
                                       select a).FirstOrDefault();
                            if (art == null)
                            {
                                art = new Artist() { FullName = artist.FullName, GenreId = artist.GenreId > 0 ? artist.GenreId : 1,
                                    Gender = 0, DateOfBirth = null, Photo = GoogleDriveServices.DEFAULT_ARTIST, Verified = false };
                                db.Artists.Add(art);
                                db.SaveChanges();
                            }
                            song.Artists.Add(art);
                        }
                        
                        songModel.UpdateEntity(song);
                        song.UploadedDate = DateTime.Now;
                        song.Verified = false;
                        song.Privacy = true;
                        db.Songs.Add(song);
                        db.SaveChanges();
                        transaction.Commit();
                        songModel = query.Where(s => s.Id == song.Id).FirstOrDefault();
                        return Request.CreateResponse(HttpStatusCode.Created, songModel);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Update song infor
        /// </summary>
        /// <param name="songModel">Song data</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [Route("")]
        [HttpPut]
        public HttpResponseMessage UpdateSong([FromBody] SongModel songModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetSongQuery(db);
                var song = (from a in db.Songs
                              where a.Id == songModel.Id
                              select a).FirstOrDefault();
                if (song == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy nghệ sĩ id=" + songModel.Id);
                }

                // Update junction data
                if (songModel.Artists.Count > 0)
                {
                    song.Artists.Clear();
                    foreach (var artist in songModel.Artists)
                    {
                        var art = (from a in db.Artists
                                   where a.Id == artist.Id
                                   select a).FirstOrDefault();
                        if (art == null)
                        {
                            art = new Artist() { FullName = artist.FullName };
                        }
                        song.Artists.Add(art);
                    }
                }
                songModel.UpdateEntity(song);
                db.SaveChanges();
                songModel = query.FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, songModel);
            }
        }

        [Route("{id}/increaseView")]
        [HttpPut]
        public HttpResponseMessage IncreaseView([FromUri] long id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var song = (from s in db.Songs
                            where s.Id == id
                            select s).FirstOrDefault();
                if (song == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy media id=" + id);
                }
                song.Views++;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Song Services

        #region Lyrics of Song
        [Route("{id}/lyrics")]
        [HttpGet]
        public HttpResponseMessage GetLyrics([FromUri] long id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var song = (from s in db.Songs
                            where s.Id == id
                            select s).FirstOrDefault();
                if (song == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy bài hát id=" + id);
                }
                var listLyrics = (from l in db.Lyrics
                                  where l.SongId == id && l.Verified == true
                                  select new LyricModel() { LyricEntity = l }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listLyrics);
            }
        }

        /// <summary>
        /// Add lyric for song
        /// </summary>
        /// <param name="id">Id of song need to add</param>
        /// <param name="lyricModel">Lyric for song</param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("{id}/lyrics")]
        [HttpPost]
        public HttpResponseMessage AddLyric([FromUri] long id, [FromBody] LyricModel lyricModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetSongQuery(db);
                var song = (from s in db.Songs
                            where s.Id == id
                            select s).FirstOrDefault();
                if (song == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy bài hát id=" + id);
                }
                if (song.Resource.Type == (int)ResourceTypeManager.Audio)
                {
                    Lyric lyric = new Lyric();
                    lyricModel.UpdateEntity(lyric);
                    db.Lyrics.Add(lyric);
                    db.SaveChanges();

                    var songModel = query.Where(s => s.Id == song.Id).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.Created, songModel);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Chỉ có thể thêm lời bài hát cho audio");
                }
            }
        }

        /// <summary>
        /// Update lyric of a song
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lyricModel"></param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("{id}/lyrics")]
        [HttpPut]
        public HttpResponseMessage UpdateLyric([FromUri] long id, [FromBody] LyricModel lyricModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetSongQuery(db);
                var lyric = (from l in db.Lyrics
                             where l.Id == lyricModel.Id
                             select l).FirstOrDefault();
                if (lyric == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy lời bài hát id=" + lyricModel.Id);
                }

                lyric.Lyric1 = lyricModel.Lyric;
                lyric.Verified = lyricModel.Verified;
                db.SaveChanges();
                
                // Update lyric model to response
                lyricModel = new LyricModel() { LyricEntity = lyric };
                return Request.CreateResponse(HttpStatusCode.OK, lyricModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("{songId}/lyrics/{lyricId}")]
        [HttpDelete]
        public HttpResponseMessage DeleteLyric([FromUri] long songId, [FromUri] long lyricId)
        {
            using (var db = new OnlineMusicEntities())
            {
                var song = (from s in db.Songs
                            where s.Id == songId
                            select s).FirstOrDefault();
                if (song == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy bài hát id=" + songId);
                }
                var lyric = (from l in db.Lyrics
                             where l.Id == lyricId
                             select l).FirstOrDefault();
                if (lyric == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy lời bài hát id=" + lyricId);
                }
                db.Lyrics.Remove(lyric);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
        #endregion Lyrics of Song

        #region Comment of Song

        [Route("{id}/comments")]
        [HttpGet]
        public HttpResponseMessage GetCommentsOfSong([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var listComments = (from c in db.SongComments
                                    where c.SongId == id
                                    select new CommentSongModel() { SongComment = c, User = new UserModel { User = c.User } }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listComments);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPost]
        public HttpResponseMessage AddCommentToSong([FromUri] int id, [FromBody] CommentSongModel commentModel)
        {
            if (commentModel.SongId != id)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Dữ liệu không phù hợp");
            }
            using (var db = new OnlineMusicEntities())
            {
                SongComment comment = new SongComment();
                commentModel.UpdateEntity(comment);
                comment.Date = DateTime.Now;
                db.SongComments.Add(comment);
                db.SaveChanges();
                commentModel = (from c in db.SongComments
                                where c.Id == comment.Id
                                select new CommentSongModel() { SongComment = c, User = new UserModel { User = c.User } }).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPut]
        public HttpResponseMessage EditComment([FromUri] int id, [FromBody] CommentSongModel commentModel)
        {
            if (commentModel.SongId != id)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Dữ liệu không phù hợp");
            }
            using (var db = new OnlineMusicEntities())
            {
                SongComment comment = (from c in db.SongComments
                                        where c.Id == commentModel.Id
                                        select c).FirstOrDefault();
                if (comment == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Không tìm thấy comment id=" + commentModel.Id);
                }
                commentModel.UpdateEntity(comment);
                db.SaveChanges();
                commentModel = (from c in db.SongComments
                                where c.Id == comment.Id
                                select new CommentSongModel() { SongComment = c, User = new UserModel { User = c.User } }).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{songId}/comments/{commentId}")]
        [HttpDelete]
        public HttpResponseMessage DeleteComment([FromUri] int songId, [FromUri] long commentId)
        {
            using (var db = new OnlineMusicEntities())
            {
                SongComment comment = (from c in db.SongComments
                                        where c.Id == commentId
                                        select c).FirstOrDefault();
                if (comment == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Không tìm thấy comment id=" + commentId);
                }
                db.SongComments.Remove(comment);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Comment of Song
    }
}
