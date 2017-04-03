﻿using OnlineMusicServices.API.DTO;
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

        public AlbumController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
            dto = new AlbumDTO(HttpContext.Current.Request.Url);
            songDto = new SongDTO(HttpContext.Current.Request.Url);
        }

        #region Album Services
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllAlbums()
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db);
                var listAlbums = query.ToList();
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
        public HttpResponseMessage GetPagingAlbums(int page = 1, int size = 200)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db);
                var listAlbums = query.OrderBy(a => a.Title).Skip((page - 1) * size).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listAlbums);
            }
        }

        [Route("latest")]
        [HttpGet]
        public HttpResponseMessage GetLatestAlbums(int page = 1, int size = 200)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db);
                var listAlbums = query.OrderByDescending(a => a.ReleasedDate).Skip((page - 1) * size).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listAlbums);
            }
        }

        [Route("popular")]
        [HttpGet]
        public HttpResponseMessage GetPopularAlbums(int page = 1, int size = 200)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db);
                var listAlbums = query.OrderByDescending(a => a.Views).Skip((page - 1) * size).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listAlbums);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("")]
        [HttpPost]
        public HttpResponseMessage CreateAlbum([FromBody] AlbumModel albumModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetAlbumQuery(db);
                var album = new Album();
                albumModel.UpdateEntity(album);
                album.Photo = GoogleDriveServices.DEFAULT_ALBUM;
                db.Albums.Add(album);
                db.SaveChanges();
                albumModel = query.Where(a => a.Id == album.Id).FirstOrDefault();
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

                            // Photo will upload in Images/Albums/{artistFullName}/{fileName}
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
                var query = dto.GetAlbumQuery(db);
                var album = (from a in db.Albums
                             where a.Id == albumModel.Id
                             select a).FirstOrDefault();
                if (album == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy album id=" + albumModel.Id);
                }

                albumModel.UpdateEntity(album);
                db.SaveChanges();
                albumModel = query.Where(a => a.Id == album.Id).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, albumModel);
            }
        }

        /// <summary>
        /// Increase view for a album by 1
        /// </summary>
        /// <param name="id">Id of album need to increase</param>
        /// <returns></returns>
        [Route("{id}/increaseView")]
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
                album.Views++;
                db.SaveChanges();
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
                var listSongs = songDto.ConvertToSongModel(album.Songs);
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("{id}/songs")]
        [HttpPut]
        public HttpResponseMessage AddSongToAlbum([FromUri] int id, [FromBody] ICollection<SongModel> listSongs)
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
                var listComments = (from c in db.AlbumComments
                                    where c.AlbumId == id
                                    select new CommentAlbumModel() { AlbumComment = c, User = new UserModel { User = c.User } }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listComments);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPost]
        public HttpResponseMessage AddCommentToAlbum([FromUri] int id, [FromBody] CommentAlbumModel commentModel)
        {
            if (commentModel.AlbumId != id)
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
                commentModel = (from c in db.AlbumComments
                                where c.Id == comment.Id
                                select new CommentAlbumModel() { AlbumComment = c, User = new UserModel { User = c.User } }).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPut]
        public HttpResponseMessage EditComment([FromUri] int id, [FromBody] CommentAlbumModel commentModel)
        {
            if (commentModel.AlbumId != id)
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
                commentModel = (from c in db.AlbumComments
                                where c.Id == comment.Id
                                select new CommentAlbumModel() { AlbumComment = c, User = new UserModel { User = c.User } }).SingleOrDefault();
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