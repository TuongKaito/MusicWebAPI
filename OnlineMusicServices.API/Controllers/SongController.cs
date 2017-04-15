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
        CommentDTO commentDto;
        RankingSongDTO rankingSongDto;

        public SongController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
            Uri uri = HttpContext.Current.Request.Url;
            dto = new SongDTO(uri);
            commentDto = new CommentDTO(uri);
            rankingSongDto = new RankingSongDTO(uri);
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
        public HttpResponseMessage GetAllSongsVerified(bool verified, bool privacy = false, string type = "audio")
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
                listSongs = query.Where(s => s.Verified == verified && s.Privacy == privacy && !String.IsNullOrEmpty(s.ResourceId)).ToList();
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
        public HttpResponseMessage GetPagingSongs(int page = 1, int size = 200, bool verified = true, bool privacy = false, string type = "audio")
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
                var listSongs = query.Where(s => !String.IsNullOrEmpty(s.ResourceId) && s.Verified == verified && s.Privacy == privacy)
                                     .OrderBy(s => s.Title)
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
                var listSongs = query.Where(s => !String.IsNullOrEmpty(s.ResourceId) && s.Verified == true && s.Privacy == false)
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
                var listSongs = query.Where(s => !String.IsNullOrEmpty(s.ResourceId) && s.Verified == true && s.Privacy == false)
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
                            string message;

                            if (resourceType == ResourceTypeManager.Video)
                            {
                                folderId = services.SearchFolder(song.User.Username, GoogleDriveServices.VIDEOS) ??
                                    services.CreateFolder(song.User.Username, GoogleDriveServices.VIDEOS);
                                message = $"{song.User.Username} vừa upload video mới";
                            }
                            else
                            {
                                folderId = services.SearchFolder(song.User.Username, GoogleDriveServices.MUSICS) ??
                                    services.CreateFolder(song.User.Username, GoogleDriveServices.MUSICS);
                                message = $"{song.User.Username} vừa upload bài hát mới";
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

                                try
                                {
                                    List<Notification> notifications = new List<Notification>();
                                    foreach (User user in song.User.User1)
                                    {
                                        Notification notification = new Notification()
                                        {
                                            Title = "Thông báo từ người dùng",
                                            Message = message,
                                            CreatedAt = DateTime.Now,
                                            UserId = user.Id,
                                            IsMark = false,
                                            Action = NotificationAction.UPLOAD
                                        };
                                        notifications.Add(notification);
                                    }
                                    if (notifications.Count > 0)
                                    {
                                        db.Notifications.AddRange(notifications);
                                        db.SaveChanges();
                                        transaction.Commit();
                                    }
                                }
                                catch
                                {

                                }

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
                var user = (from u in db.Users where u.Id == songModel.AuthorId select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + songModel.AuthorId);
                }
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
                        song.Privacy = song.Verified = song.Official = false;
                        db.Songs.Add(song);
                        db.SaveChanges();
                        transaction.Commit();
                        db.Entry(song).Reference(s => s.Genre).Load();
                        db.Entry(song).Reference(s => s.User).Load();
                        db.Entry(song).Collection(s => s.Artists).Load();

                        songModel = dto.GetSongQuery(db, s => s.Id == song.Id).FirstOrDefault();
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
                            art = new Artist() { FullName = artist.FullName, GenreId = artist.GenreId > 0 ? artist.GenreId : 1, Gender = 0,
                                DateOfBirth = null, Photo = GoogleDriveServices.DEFAULT_ARTIST, Verified = false };
                        }
                        song.Artists.Add(art);
                    }
                }
                songModel.UpdateEntity(song);
                db.SaveChanges();
                songModel = dto.GetSongQuery(db, s => s.Id == song.Id).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, songModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("verify")]
        [HttpPut]
        public HttpResponseMessage VerifySongs([FromBody] List<SongModel> list)
        {
            using (var db = new OnlineMusicEntities())
            {
                foreach(SongModel model in list)
                {
                    Song song = (from s in db.Songs where s.Id == model.Id select s).FirstOrDefault();
                    if (song != null && song.Verified != model.Verified)
                    {
                        song.Verified = model.Verified;
                        db.SaveChanges();

                        if (song.Verified)
                        {
                            try
                            {
                                // Push notification
                                string action = NotificationAction.VERIFIED_MEDIA + "_" + song.Id;
                                Notification notification = (from ntf in db.Notifications where ntf.UserId == song.AuthorId select ntf).FirstOrDefault();
                                if (notification == null)
                                {
                                    notification = new Notification()
                                    {
                                        Title = "Hệ thống đã kiểm duyệt",
                                        Message = song.Resource.Type == (int)ResourceTypeManager.Audio ? "Bài hát " : "Video " + song.Title +
                                        " của bạn đã được kiểm duyệt thành công",
                                        IsMark = false,
                                        UserId = song.AuthorId,
                                        CreatedAt = DateTime.Now,
                                        Action = action
                                    };
                                    db.Notifications.Add(notification);
                                    db.SaveChanges();
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Route("{id}/increase-view")]
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
                var views = (from v in db.SongViews where v.SongId == id select v).FirstOrDefault();
                // Create new if song hasn't view yet
                if (views == null)
                {
                    views = new SongView() { Ip = "", SongId = id, Timestamp = DateTime.Now, Views = 0 };
                    db.SongViews.Add(views);
                }
                else
                {
                    // Reset view every hour
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
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Song Services

        #region Ranking Song

        [Route("ranking")]
        [HttpGet]
        public HttpResponseMessage GetAllRankingSongs()
        {
            using (var db = new OnlineMusicEntities())
            {
                var rankingList = rankingSongDto.GetRankingQuery(db).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, rankingList);
            }
        }

        [Route("ranking/{year}/{week}")]
        [HttpGet]
        public HttpResponseMessage GetRankingSongs([FromUri] int year, [FromUri] int week)
        {
            using (var db = new OnlineMusicEntities())
            {
                var rankingList = rankingSongDto.GetRankingQuery(db, r => r.StartDate.Year == year && r.Week == week).ToList();
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
                var rankingList = rankingSongDto.GetRankingQuery(db, r => r.StartDate.Date.Equals(rankingDate.Date)).ToList();
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
                    db.UpdateSongRanking(updateDate);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                catch (Exception e)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.InnerException.Message);
                }
            }
        }

        #endregion Ranking Song

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
                    
                    return Request.CreateResponse(HttpStatusCode.Created);
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
                
                return Request.CreateResponse(HttpStatusCode.OK);
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
        public HttpResponseMessage GetCommentsOfSong([FromUri] long id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var listComments = commentDto.GetCommentQuery(db, (SongComment c) => c.SongId == id)
                    .OrderByDescending(c => c.Date).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listComments);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPost]
        public HttpResponseMessage AddCommentToSong([FromUri] long id, [FromBody] CommentSongModel commentModel)
        {
            if (commentModel.DataId != id)
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

                comment.User = (from u in db.Users where u.Id == comment.UserId select u).FirstOrDefault();
                commentModel = commentDto.GetCommentQuery(db, swhereClause: null).Where(c => c.Id == comment.Id).FirstOrDefault();
                try
                {
                    Song song = (from s in db.Songs where s.Id == id select s).FirstOrDefault();
                    if (song != null && song.AuthorId != comment.UserId)
                    {
                        string action = NotificationAction.COMMENT_AUDIO + "_" + song.Id;
                        Notification notification = (from ntf in db.Notifications where ntf.UserId == song.AuthorId && ntf.Action == action select ntf).FirstOrDefault();
                        if (notification == null)
                        {
                            notification = new Notification()
                            {
                                Title = "Hệ thống",
                                IsMark = false,
                                Action = action,
                                UserId = song.AuthorId
                            };
                            db.Notifications.Add(notification);
                        }
                        UserInfoModel info = commentModel.UserInfo;
                        string actor = info != null && !String.IsNullOrEmpty(info.FullName) ? info.FullName : comment.User?.Username;
                        long commentCount = song.SongComments.Select(c => c.UserId).Distinct().Count();
                        if (commentCount > 1)
                            actor += " và " + (commentCount - 1) + " người khác";
                        notification.Message = $"{actor} đã comment vào" +
                            (song.Resource.Type == (int)ResourceTypeManager.Audio ? " bài hát " : " video ") + song.Title + " của bạn";
                        notification.CreatedAt = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                catch
                {
                }

                return Request.CreateResponse(HttpStatusCode.Created, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPut]
        public HttpResponseMessage EditComment([FromUri] long id, [FromBody] CommentSongModel commentModel)
        {
            if (commentModel.DataId != id)
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
                commentModel = commentDto.GetCommentQuery(db, (SongComment c) => c.Id == comment.Id).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{songId}/comments/{commentId}")]
        [HttpDelete]
        public HttpResponseMessage DeleteComment([FromUri] long songId, [FromUri] long commentId)
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
