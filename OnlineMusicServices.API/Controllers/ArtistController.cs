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
    [RoutePrefix("api/artists")]
    public class ArtistController : ApiController
    {
        GoogleDriveServices services;
        ArtistDTO dto;
        AlbumDTO albumDto;
        SongDTO songDto;

        public ArtistController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
            dto = new ArtistDTO(HttpContext.Current.Request.Url);
            albumDto = new AlbumDTO(HttpContext.Current.Request.Url);
            songDto = new SongDTO(HttpContext.Current.Request.Url);
        }

        /// <summary>
        /// Get list artists from database
        /// </summary>
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllArtists()
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetArtistQuery(db);
                IEnumerable<ArtistModel> listArtists;
                listArtists = query.OrderByDescending(a => a.Id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listArtists);
            }
        }

        /// <summary>
        /// Get list artists from database
        /// </summary>
        /// <param name="verified">Get list artists is verified. Null to get all list</param>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllArtists(bool verified)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetArtistQuery(db);
                IEnumerable<ArtistModel> listArtists;
                listArtists = query.Where(a => a.Verified == verified).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listArtists);
            }
        }

        /// <summary>
        /// Get list artists with paging option
        /// </summary>
        /// <param name="page">Number of page to get</param>
        /// <param name="size">Number of elements in a page</param>
        /// <returns></returns>
        [Route("pager")]
        [HttpGet]
        public HttpResponseMessage GetPagingArtists(int page = 1, int size = 0, bool verified = true)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetArtistQuery(db);
                List<ArtistModel> listArtists;
                if (size > 0)
                {
                    listArtists = query.Where(a => a.Verified == verified).OrderBy(a => a.FullName).Skip((page - 1) * size).Take(size).ToList();
                }
                else
                {
                    listArtists = query.Where(a => a.Verified == verified).OrderBy(a => a.FullName).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, listArtists);
            }
        }

        [Route("latest")]
        [HttpGet]
        public HttpResponseMessage GetLatestArtists(int page = 1, int size = 0)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetArtistQuery(db);
                List<ArtistModel> listArtists;
                if (size > 0)
                {
                    listArtists = query.Where(a => a.Verified == true).OrderByDescending(a => a.Id).Skip((page - 1) * size).Take(size).ToList();
                }
                else
                {
                    listArtists = query.Where(a => a.Verified == true).OrderByDescending(a => a.Id).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, listArtists);
            }
        }

        [Route("famous")]
        [HttpGet]
        public HttpResponseMessage GetFamousArtists(int page = 1, int size = 0)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetArtistQuery(db, (artist) => artist.Songs.Count > 0);
                List<ArtistModel> listArtists;
                if (size > 0)
                {
                    listArtists = query.Where(a => a.Verified == true).OrderByDescending(a => a.Followers).Skip((page - 1) * size).Take(size).ToList();
                }
                else
                {
                    listArtists = query.Where(a => a.Verified == true).OrderByDescending(a => a.Followers).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, listArtists);
            }
        }

        [Route("{id}/albums")]
        [HttpGet]
        public HttpResponseMessage GetAlbums([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetArtistQuery(db);
                var artist = query.Where(a => a.Id == id).FirstOrDefault();
                if (artist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy nghệ sĩ id=" + id);
                }

                var albumList = albumDto.GetAlbumQuery(db, album => album.ArtistId == id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, albumList);
            }
        }

        /// <summary>
        /// Get an artist specified id
        /// </summary>
        /// <param name="id">Id of artist</param>
        /// <returns></returns>
        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetArtist([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetArtistQuery(db);
                var artist = query.Where(a => a.Id == id).FirstOrDefault();
                if (artist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy nghệ sĩ id=" + id);
                }
                return Request.CreateResponse(HttpStatusCode.OK, artist);
            }
        }

        /// <summary>
        /// Upload artist photo to server
        /// </summary>
        /// <param name="id">Artist id</param>
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
                var artist = (from a in db.Artists
                              where a.Id == id
                              select a).FirstOrDefault();
                if (artist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy nghệ sĩ id=" + id);
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
                            var fileName = artist.FullName + ext;
                            var folderId = services.SearchFolder(artist.FullName, GoogleDriveServices.ARTISTS) ??
                                services.CreateFolder(artist.FullName, GoogleDriveServices.ARTISTS);

                            // Resize image before upload
                            var scaledImage = ImageFactory.Resize(file.InputStream);

                            // Photo will upload in Images/Artists/{artistFullName}/{fileName}
                            var resourceId = services.UploadFile(scaledImage, fileName, Media.GetMediaTypeFromExtension(ext), folderId);
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
                                artist.Photo = resourceId;
                                db.SaveChanges();
                                transaction.Commit();
                                return Request.CreateResponse(HttpStatusCode.OK, String.Format("Upload hình ảnh cho nghệ sĩ {0} thành công", artist.FullName));
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
        /// Create a new artist
        /// </summary>
        /// <param name="artistModel">Artist need to insert</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [Route("")]
        [HttpPost]
        public HttpResponseMessage CreateArtist([FromBody] ArtistModel artistModel)
        {
            try
            {
                using (var db = new OnlineMusicEntities())
                {
                    Artist artist = new Artist();
                    artistModel.UpdateEntity(artist);
                    artist.Photo = GoogleDriveServices.DEFAULT_ARTIST;
                    db.Artists.Add(artist);
                    db.SaveChanges();
                    db.Entry(artist).Reference(a => a.Genre).Load();
                    db.Entry(artist).Collection(a => a.Users).Load();

                    artistModel = dto.GetArtistQuery(db, a => a.Id == artist.Id).FirstOrDefault();
                    return Request.CreateResponse(HttpStatusCode.Created, artistModel);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Update artist infor
        /// </summary>
        /// <param name="artistModel">Artist data</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [Route("")]
        [HttpPut]
        public HttpResponseMessage UpdateArtist([FromBody] ArtistModel artistModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetArtistQuery(db);
                var artist = (from a in db.Artists
                              where a.Id == artistModel.Id
                              select a).FirstOrDefault();
                if (artist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy nghệ sĩ id=" + artistModel.Id);
                }

                artistModel.UpdateEntity(artist);
                db.SaveChanges();
                artistModel = query.Where(a => a.Id == artist.Id).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, artistModel);
            }
        }

        [Route("{id}/songs")]
        [HttpGet]
        public HttpResponseMessage GetSongsOfArtist([FromUri] int id, bool verified = true)
        {
            using (var db = new OnlineMusicEntities())
            {
                Artist artist = (from a in db.Artists
                                 where a.Id == id && a.Verified == verified
                                 select a).FirstOrDefault();
                if (artist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy nghệ sĩ id=" + id);
                }
                ICollection<SongModel> listSongs = songDto.ConvertToSongModel(artist.Songs.Where(s => s.Verified == true && s.Privacy == false && s.Official == true).ToList()).OrderByDescending(s => s.Views).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        [Route("{artistId}/followers")]
        [HttpGet]
        public HttpResponseMessage GetListFollowers([FromUri] int artistId)
        {
            using (var db = new OnlineMusicEntities())
            {
                var list = (from a in db.Artists
                            where a.Id == artistId
                            select a.Users).FirstOrDefault();
                if (list == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy nghệ sĩ id=" + artistId);
                }
                var listInfo = (from i in db.UserInfoes select i).ToList();
                var listFollowers = (from u in list
                                     join i in listInfo on u.Id equals i.UserId
                                     select new UserInfoModel { UserInfo = i, Followers = u.User1.Count, Avatar = dto.DomainHosting + i.Avatar }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listFollowers);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{artistId}/followers/{userId}")]
        [HttpPut]
        public HttpResponseMessage FollowArtist([FromUri] int artistId, [FromUri] int userId, bool unfollow = false)
        {
            using (var db = new OnlineMusicEntities())
            {
                var artist = (from a in db.Artists
                            where a.Id == artistId
                            select a).SingleOrDefault();
                if (artist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy nghệ sĩ id=" + userId);
                }
                var follower = (from u in db.Users
                                where u.Id == userId
                                select u).SingleOrDefault();
                if (follower == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy follower user id=" + userId);
                }
                if (unfollow)
                {
                    artist.Users.Remove(follower);
                }
                else
                {
                    artist.Users.Add(follower);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
