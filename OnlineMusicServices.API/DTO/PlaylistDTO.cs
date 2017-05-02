using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class PlaylistDTO
    {
        public string DomainHosting { get; set; } = String.Empty;

        public PlaylistDTO(Uri uri)
        {
            // Setting the url to get image
            DomainHosting = $"{uri.Scheme}://{uri.DnsSafeHost}/api/resources/streaming/";
        }

        public IQueryable<PlaylistModel> GetPlaylistQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<Playlist, bool>> whereClause = null)
        {
            IQueryable<Playlist> query = db.Playlists;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            var playlistQuery = query.Select(Converter).AsQueryable();
            return playlistQuery;
        }

        public PlaylistModel Converter(Playlist pl)
        {
            return new PlaylistModel()
            {
                Id = pl.Id,
                Title = pl.Title,
                Description = pl.Description,
                CreatedDate = pl.CreatedDate,
                UserId = pl.UserId,
                Views = (from v in pl.PlaylistViews select v.Views).FirstOrDefault(),
                Photo = pl.Photo,
                PhotoUrl = DomainHosting + pl.Photo,
                User = new UserModel { User = pl.User }
            };
        }
    }
}