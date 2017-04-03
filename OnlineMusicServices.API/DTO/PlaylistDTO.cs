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
            DomainHosting = $"{uri.Scheme}://{uri.Authority}/api/resources/streaming/";
        }

        public IQueryable<PlaylistModel> GetPlaylistQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<Playlist, bool>> whereClause = null)
        {
            IQueryable<Playlist> query = db.Playlists;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            var playlistQuery = query.Select(pl => new PlaylistModel()
            {
                Id = pl.Id,
                Title = pl.Title,
                Description = pl.Description,
                CreatedDate = pl.CreatedDate,
                UserId = pl.UserId,
                Views = pl.Views,
                Photo = DomainHosting + pl.Photo,
                User = new UserModel { User = pl.User }
            });
            return playlistQuery;
        }
    }
}