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
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/playlists")]
    public class PlaylistController : ApiController
    {
        GoogleDriveServices services;
        PlaylistDTO dto;
        SongDTO songDto;

        public PlaylistController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
            dto = new PlaylistDTO(HttpContext.Current.Request.Url);
            songDto = new SongDTO(HttpContext.Current.Request.Url);
        }

        #region Playlist Services
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllPlaylists()
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetPlaylistQuery(db);
                var listPlaylists = query.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listPlaylists);
            }
        }

        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetPlaylist([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetPlaylistQuery(db);
                var playlist = query.Where(a => a.Id == id).FirstOrDefault();
                if (playlist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy playlist id=" + id);
                }
                return Request.CreateResponse(HttpStatusCode.OK, playlist);
            }
        }

        [Route("pager")]
        [HttpGet]
        public HttpResponseMessage GetPagingPlaylists(int page = 1, int size = 200)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetPlaylistQuery(db, playlist => playlist.Songs.Count > 0);
                var listPlaylists = query.OrderBy(pl => pl.Title).Skip((page - 1) * size).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listPlaylists);
            }
        }

        [Route("latest")]
        [HttpGet]
        public HttpResponseMessage GetLatestPlaylists(int page = 1, int size = 200)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetPlaylistQuery(db, playlist => playlist.Songs.Count > 0);
                var listPlaylists = query.OrderByDescending(pl => pl.CreatedDate)
                                         .ThenByDescending(pl => pl.Id)
                                         .Skip((page - 1) * size).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listPlaylists);
            }
        }

        [Route("popular")]
        [HttpGet]
        public HttpResponseMessage GetPopularPlaylists(int page = 1, int size = 200)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetPlaylistQuery(db, playlist => playlist.Songs.Count > 0);
                var listPlaylists = query.OrderByDescending(pl => pl.Views).Skip((page - 1) * size).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listPlaylists);
            }
        }

        [Authorize(Roles = "User")]
        [Route("")]
        [HttpPost]
        public HttpResponseMessage CreatePlaylist([FromBody] PlaylistModel playlistModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetPlaylistQuery(db);
                var playlist = new Playlist();
                playlistModel.UpdateEntity(playlist);
                playlist.CreatedDate = DateTime.Now;
                playlist.Photo = GoogleDriveServices.DEFAULT_PLAYLIST;
                db.Playlists.Add(playlist);
                db.SaveChanges();
                playlistModel = query.Where(a => a.Id == playlist.Id).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.Created, playlistModel);
            }
        }

        /// <summary>
        /// Upload playlist photo to server
        /// </summary>
        /// <param name="id">Playlist id</param>
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
                var playlist = (from a in db.Playlists
                             where a.Id == id
                             select a).FirstOrDefault();
                if (playlist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy playlist id=" + id);
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
                            var fileName = playlist.Title + ext;
                            var folderId = services.SearchFolder(playlist.User.Username, GoogleDriveServices.PLAYLISTS) ??
                                services.CreateFolder(playlist.User.Username, GoogleDriveServices.PLAYLISTS);

                            // Photo will upload in Images/Playlists/{username}/{fileName}
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
                                playlist.Photo = resourceId;
                                db.SaveChanges();
                                transaction.Commit();
                                return Request.CreateResponse(HttpStatusCode.OK, String.Format("Upload hình ảnh cho playlist {0} thành công", playlist.Title));
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

        [Authorize(Roles = "User")]
        [Route("")]
        [HttpPut]
        public HttpResponseMessage UpdatePlaylist([FromBody] PlaylistModel playlistModel)
        {
            using (var db = new OnlineMusicEntities())
            {
                var query = dto.GetPlaylistQuery(db);
                var playlist = (from a in db.Playlists
                             where a.Id == playlistModel.Id
                             select a).FirstOrDefault();
                if (playlist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy playlist id=" + playlistModel.Id);
                }

                playlistModel.UpdateEntity(playlist);
                db.SaveChanges();
                playlistModel = query.Where(a => a.Id == playlist.Id).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, playlistModel);
            }
        }

        /// <summary>
        /// Increase view for a playlist by 1
        /// </summary>
        /// <param name="id">Id of playlist need to increase</param>
        /// <returns></returns>
        [Route("{id}/increaseView")]
        [HttpPut]
        public HttpResponseMessage IncreaseView([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var playlist = (from a in db.Playlists
                             where a.Id == id
                             select a).FirstOrDefault();
                if (playlist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy playlist id=" + id);
                }
                playlist.Views++;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}")]
        [HttpDelete]
        public HttpResponseMessage DeletePlaylist([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var playlist = (from a in db.Playlists
                             where a.Id == id
                             select a).FirstOrDefault();
                if (playlist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy playlist id=" + id);
                }
                db.Playlists.Remove(playlist);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Playlist Services

        #region Songs of Playlist

        [Route("{id}/songs")]
        [HttpGet]
        public HttpResponseMessage GetSongsOfPlaylist([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var playlist = (from a in db.Playlists
                             where a.Id == id
                             select a).FirstOrDefault();
                if (playlist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy playlist id=" + id);
                }
                var listSongs = songDto.ConvertToSongModel(playlist.Songs);
                return Request.CreateResponse(HttpStatusCode.OK, listSongs);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/songs")]
        [HttpPut]
        public HttpResponseMessage AddSongToPlaylist([FromUri] int id, [FromBody] ICollection<SongModel> listSongs)
        {
            using (var db = new OnlineMusicEntities())
            {
                var playlist = (from a in db.Playlists
                             where a.Id == id
                             select a).FirstOrDefault();
                if (playlist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy playlist id=" + id);
                }
                // Identity user upload song to playlist
                var identity = (ClaimsIdentity)User.Identity;
                if (identity.Name != playlist.UserId.ToString())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Token");
                }

                playlist.Songs.Clear();
                foreach (var song in listSongs)
                {
                    var sg = (from s in db.Songs
                              where s.Id == song.Id
                              select s).FirstOrDefault();
                    if (sg != null)
                        playlist.Songs.Add(sg);
                }
                // List songs add to playlist invalid
                if (playlist.Songs.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Danh sách bài hát không hợp lệ");
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/songs/clear")]
        [HttpPut]
        public HttpResponseMessage ClearPlaylist([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var playlist = (from a in db.Playlists
                             where a.Id == id
                             select a).FirstOrDefault();
                if (playlist == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy playlist id=" + id);
                }
                // Identity user upload song to playlist
                var identity = (ClaimsIdentity)User.Identity;
                if (identity.Name != playlist.UserId.ToString())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Token");
                }

                playlist.Songs.Clear();
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Songs of Playlist

        #region Comment of Playlist

        [Route("{id}/comments")]
        [HttpGet]
        public HttpResponseMessage GetCommentsOfPlaylist([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var listComments = (from c in db.PlaylistComments
                                    where c.PlaylistId == id
                                    select new CommentPlaylistModel() { PlaylistComment = c, User = new UserModel { User = c.User } }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listComments);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPost]
        public HttpResponseMessage AddCommentToPlaylist([FromUri] int id, [FromBody] CommentPlaylistModel commentModel)
        {
            if (commentModel.PlaylistId != id)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Dữ liệu không phù hợp");
            }
            using (var db = new OnlineMusicEntities())
            {
                PlaylistComment comment = new PlaylistComment();
                commentModel.UpdateEntity(comment);
                comment.Date = DateTime.Now;
                db.PlaylistComments.Add(comment);
                db.SaveChanges();
                commentModel = (from c in db.PlaylistComments
                                where c.Id == comment.Id
                                select new CommentPlaylistModel() { PlaylistComment = c, User = new UserModel { User = c.User } }).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{id}/comments")]
        [HttpPut]
        public HttpResponseMessage EditComment([FromUri] int id, [FromBody] CommentPlaylistModel commentModel)
        {
            if (commentModel.PlaylistId != id)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Dữ liệu không phù hợp");
            }
            using (var db = new OnlineMusicEntities())
            {
                PlaylistComment comment = (from c in db.PlaylistComments
                                        where c.Id == commentModel.Id
                                        select c).FirstOrDefault();
                if (comment == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Không tìm thấy comment id=" + commentModel.Id);
                }
                commentModel.UpdateEntity(comment);
                db.SaveChanges();
                commentModel = (from c in db.PlaylistComments
                                where c.Id == comment.Id
                                select new CommentPlaylistModel() { PlaylistComment = c, User = new UserModel { User = c.User } }).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, commentModel);
            }
        }

        [Authorize(Roles = "User")]
        [Route("{playlistId}/comments/{commentId}")]
        [HttpDelete]
        public HttpResponseMessage DeleteComment([FromUri] int playlistId, [FromUri] long commentId)
        {
            using (var db = new OnlineMusicEntities())
            {
                PlaylistComment comment = (from c in db.PlaylistComments
                                        where c.Id == commentId
                                        select c).FirstOrDefault();
                if (comment == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.OK, "Không tìm thấy comment id=" + commentId);
                }
                db.PlaylistComments.Remove(comment);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        #endregion Comment of Playlist
    }
}