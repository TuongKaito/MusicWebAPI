using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class CommentDTO
    {
        private UserInfoDTO userInfoDto;
        public string DomainHosting { get; set; }

        public CommentDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.DnsSafeHost}/api/resources/streaming/";
            userInfoDto = new UserInfoDTO(uri);
        }

        public IQueryable<CommentAlbumModel> GetCommentQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<AlbumComment, bool>> awhereClause = null)
        {
            IQueryable<AlbumComment> query = db.AlbumComments;
            if (awhereClause != null)
            {
                query = query.Where(awhereClause);
            }

            var commentQuery = query.Select(Converter).AsQueryable();
            return commentQuery;
        }

        public IQueryable<CommentSongModel> GetCommentQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<SongComment, bool>> swhereClause = null)
        {
            IQueryable<SongComment> query = db.SongComments;
            if (swhereClause != null)
            {
                query = query.Where(swhereClause);
            }

            var commentQuery = query.Select(Converter).AsQueryable();
            return commentQuery;
        }

        public IQueryable<CommentPlaylistModel> GetCommentQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<PlaylistComment, bool>> pwhereClause = null)
        {
            IQueryable<PlaylistComment> query = db.PlaylistComments;
            if (pwhereClause != null)
            {
                query = query.Where(pwhereClause);
            }

            var commentQuery = query.Select(Converter).AsQueryable();
            return commentQuery;
        }

        public CommentAlbumModel Converter(AlbumComment c)
        {
            return new CommentAlbumModel()
            {
                Id = c.Id,
                Comment = c.Comment,
                UserId = c.UserId,
                Date = c.Date,
                DataId = c.AlbumId,
                UserInfo = userInfoDto.ConvertToUserInfoModel(c.User.UserInfoes).FirstOrDefault()
            };
        }

        public CommentSongModel Converter(SongComment c)
        {
            return new CommentSongModel()
            {
                Id = c.Id,
                Comment = c.Comment,
                UserId = c.UserId,
                Date = c.Date,
                DataId = c.SongId,
                UserInfo = userInfoDto.ConvertToUserInfoModel(c.User.UserInfoes).FirstOrDefault()
            };
        }

        public CommentPlaylistModel Converter(PlaylistComment c)
        {
            return new CommentPlaylistModel()
            {
                Id = c.Id,
                Comment = c.Comment,
                UserId = c.UserId,
                Date = c.Date,
                DataId = c.PlaylistId,
                UserInfo = userInfoDto.ConvertToUserInfoModel(c.User.UserInfoes).FirstOrDefault()
            };
        }
    }
}