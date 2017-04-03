using OnlineMusicServices.Data;
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

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        GoogleDriveServices services;
        PlaylistDTO playlistDto;
        string domainHosting;

        public UsersController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
            Uri uri = HttpContext.Current.Request.Url;
            playlistDto = new PlaylistDTO(uri);
            domainHosting = $"{uri.Scheme}://{uri.Authority}/api/resources/streaming/";
        }

        [Route("famous")]
        [HttpGet]
        public HttpResponseMessage GetUserInfo(int page = 1, int size = 20)
        {
            using (OnlineMusicEntities db = new OnlineMusicEntities())
            {
                return Request.CreateResponse(HttpStatusCode.OK, (from info in db.UserInfoes
                                                                  join user in db.Users on info.UserId equals user.Id
                                                                  orderby user.User1.Count descending, user.Users.Count descending
                                                                  where user.RoleId != (int)RoleManager.Admin
                                                                  select new UserInfoModel { UserInfo = info, Followers = user.User1.Count, Avatar = domainHosting + info.Avatar })
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
                return Request.CreateResponse(HttpStatusCode.OK, (from info in db.UserInfoes
                                                                  join user in db.Users on info.UserId equals user.Id
                                                                  where info.UserId == id
                                                                  select new UserInfoModel { UserInfo = info, Followers = user.User1.Count, Avatar = domainHosting + info.Avatar }).FirstOrDefault());
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
            catch(Exception ex)
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
                            var ext = Path.GetExtension(file.FileName).ToLower();
                            var fileName = user.FullName + ext;
                            var folderId = services.SearchFolder(user.User.Username, GoogleDriveServices.AVATARS) ??
                                services.CreateFolder(user.User.Username, GoogleDriveServices.AVATARS);

                            // Photo will upload in Images/Avatars/{username}/{fileName}
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
                                 select new UserInfoModel() { UserInfo = info, Followers = l.User1.Count, Avatar = domainHosting + info.Avatar }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listUsers);
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
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

    }
}
