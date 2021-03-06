﻿using OnlineMusicServices.Data;
using OnlineMusicServices.API.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System;
using System.Web;
using System.IO;
using OnlineMusicServices.API.Storage;
using OnlineMusicServices.API.Utility;
using System.Security.Claims;
using OnlineMusicServices.API.DTO;
using System.Collections.Generic;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        GoogleDriveServices services;
        UserInfoDTO dto;
        PlaylistDTO playlistDto;
        SongDTO songDto;

        public UsersController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
            Uri uri = HttpContext.Current.Request.Url;
            dto = new UserInfoDTO(uri);
            playlistDto = new PlaylistDTO(uri);
            songDto = new SongDTO(uri);
        }

        [Route("famous")]
        [HttpGet]
        public HttpResponseMessage GetUserInfo(int page = 1, int size = 20)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                return Request.CreateResponse(HttpStatusCode.OK, dto.GetUserInfoQuery(db)
                                                                  .OrderByDescending(info => info.Followers)
                                                                  .Skip((page - 1) * size)
                                                                  .Take(size)
                                                                  .ToList());
            }
        }

        /// <summary>
        /// Get information of a user
        /// </summary>
        /// <param name="id">Loged in account id</param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetUserInfo([FromUri] int id)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                return Request.CreateResponse(HttpStatusCode.OK, dto.GetUserInfoQuery(db, info => info.UserId == id).FirstOrDefault());
            }
        }

        /// <summary>
        /// Update information of a user
        /// </summary>
        /// <param name="userInfo">Info of user</param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("")]
        [HttpPut]
        public HttpResponseMessage UpdateUserInfo([FromBody] UserInfoModel userInfo)
        {
            var identity = (ClaimsIdentity)User.Identity;
            if (identity.Name != userInfo.UserId.ToString())
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Token");
            }
            try
            {
                using (OnlineMusicEntities db = new OnlineMusicEntities())
                {
                    var user = (from u in db.UserInfoes
                                where u.Id == userInfo.Id
                                select u).FirstOrDefault();
                    if (user == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Thông tin người dùng này không tồn tại");
                    }

                    userInfo.UpdateEntity(user);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, userInfo);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Upload avatar of user to server
        /// </summary>
        /// <param name="id">User info id</param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
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
                var user = (from u in db.UserInfoes
                            where u.Id == id
                            select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + id);
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
                            string ext = Path.GetExtension(file.FileName).ToLower();
                            string fileName = user.FullName + ext;
                            string folderId = services.SearchFolder(user.User.Username, GoogleDriveServices.AVATARS) ??
                                services.CreateFolder(user.User.Username, GoogleDriveServices.AVATARS);

                            Stream scaledImage = ImageFactory.Resize(file.InputStream);

                            // Photo will upload in Images/Avatars/{username}/{fileName}
                            string resourceId = services.UploadFile(scaledImage, fileName, Media.GetMediaTypeFromExtension(ext), folderId);
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
                                user.Avatar = resourceId;
                                db.SaveChanges();
                                transaction.Commit();
                                return Request.CreateResponse(HttpStatusCode.OK, String.Format("Upload avatar cho user {0} thành công", user.User.Username));
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

        [Route("{id}/playlists")]
        [HttpGet]
        public HttpResponseMessage GetPlaylists([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = playlistDto.GetPlaylistQuery(db, playlist => playlist.UserId == id);
                var list = (from pl in query select pl).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/songs")]
        [HttpGet]
        public HttpResponseMessage GetMediaUploaded([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users where u.Id == id select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + id);
                }
                var query = songDto.GetSongQuery(db, song => song.AuthorId == id);
                var list = query.OrderByDescending(song => song.UploadedDate)
                    .ThenBy(song => song.Verified)
                    .ThenBy(song => song.Privacy).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{userId}/songs/privacy")]
        [HttpPut]
        public HttpResponseMessage SetPrivacySongs([FromUri] int userId, [FromBody] List<SongModel> list)
        {
            using (var db = new OnlineMusicEntities())
            {
                foreach(SongModel model in list)
                {
                    var song = (from s in db.Songs where s.Id == model.Id select s).FirstOrDefault();
                    if (song != null)
                    {
                        song.Privacy = model.Privacy;
                    }
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/notifications")]
        [HttpGet]
        public HttpResponseMessage GetNotifications([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var list = (from ntf in db.Notifications
                            where ntf.UserId == id
                            orderby ntf.CreatedAt descending
                            select new NotificationModel() { Notification = ntf }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/notifications/marks")]
        [HttpPut]
        public HttpResponseMessage MarkNotifications([FromUri] int id, [FromBody] List<NotificationModel> list)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users where u.Id == id select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + id);
                }
                foreach(var model in list)
                {
                    var notify = (from ntf in user.Notifications
                                  where ntf.Id == model.Id
                                  select ntf).FirstOrDefault();
                    if (notify != null)
                    {
                        notify.IsMark = true;
                    }
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/notifications/{notificationId}/marks")]
        [HttpPut]
        public HttpResponseMessage MarkNotification([FromUri] int id, [FromUri] long notificationId)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users where u.Id == id select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + id);
                }
                var notify = (from ntf in user.Notifications
                                where ntf.Id == notificationId
                                select ntf).FirstOrDefault();
                if (notify != null)
                {
                    notify.IsMark = true;
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/notifications")]
        [HttpDelete]
        public HttpResponseMessage DeleteNotifications([FromUri] int id, [FromBody] List<NotificationModel> list)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users where u.Id == id select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + id);
                }
                List<long> deletingId = (from model in list select model.Id).ToList();
                var listNofity = (from ntf in user.Notifications
                                where deletingId.Contains(ntf.Id)
                                select ntf).ToList();
                if (listNofity != null && listNofity.Count > 0)
                {
                    db.Notifications.RemoveRange(listNofity);
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/notifications/{notificationId}")]
        [HttpDelete]
        public HttpResponseMessage DeleteNotification([FromUri] int id, [FromUri] long notificationId)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users where u.Id == id select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + id);
                }
                var notify = (from ntf in user.Notifications
                              where ntf.Id == notificationId
                              select ntf).FirstOrDefault();
                if (notify != null)
                {
                    db.Notifications.Remove(notify);
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/followers")]
        [HttpGet]
        public HttpResponseMessage GetListFollowers([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var list = (from u in db.Users
                            where u.Id == id
                            select u.User1).FirstOrDefault();
                if (list == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + id);
                }
                var listUsers = (from l in list
                                 join info in db.UserInfoes on l.Id equals info.UserId
                                 select info).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, dto.ConvertToUserInfoModel(listUsers));
            }
        }

        [Authorize(Roles = "User")]
        [Route("{userId}/followers/{followerId}")]
        [HttpPut]
        public HttpResponseMessage FollowerUser([FromUri] int userId, [FromUri] int followerId, bool unfollow = false)
        {
            var identity = (ClaimsIdentity)User.Identity;
            if (identity.Name != followerId.ToString())
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Token");
            }
            if (userId == followerId)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Không thể follow bản thân");
            }
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users
                            where u.Id == userId
                            select u).SingleOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy user id=" + userId);
                }
                var follower = (from u in db.Users
                                where u.Id == followerId
                                select u).SingleOrDefault();
                if (follower == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy follower user id=" + followerId);
                }
                if (unfollow)
                {
                    user.User1.Remove(follower);
                }
                else
                {
                    user.User1.Add(follower);

                    Notification notification = (from ntf in db.Notifications where ntf.UserId == user.Id && ntf.Action == NotificationAction.USER_FOLLOW select ntf).FirstOrDefault();
                    if (notification == null)
                    {
                        notification = new Notification()
                        {
                            Title = "Hệ thống",
                            IsMark = false,
                            UserId = user.Id,
                            Action = NotificationAction.USER_FOLLOW
                        };
                        db.Notifications.Add(notification);
                    }
                    UserInfo info = (from i in db.UserInfoes where i.UserId == follower.Id select i).FirstOrDefault();
                    string actor = info != null && !String.IsNullOrEmpty(info.FullName) ? info.FullName : follower.Username;
                    int followerCount = user.User1.Count;
                    if (followerCount > 1)
                        actor += " và " + (followerCount - 1) + " người dùng khác"; 
                    notification.Message = $"{actor} đang theo dõi bạn";
                    notification.CreatedAt = DateTime.Now;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

    }
}
