using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class AlbumDTO
    {
        public string DomainHosting { get; set; }

        public AlbumDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.Authority}/api/resources/streaming/";
        }

        public IQueryable<AlbumModel> GetAlbumQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<Album, bool>> whereClause = null)
        {
            IQueryable<Album> query = db.Albums.Where(album => album.Songs.Count > 0);
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }

            var albumQuery = query.Select(a => new AlbumModel()
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                GenreId = a.GenreId,
                Genre = new GenreModel { Genre = a.Genre },
                ArtistId = a.ArtistId,
                Artist = new ArtistModel { Artist = a.Artist, Photo = DomainHosting + a.Artist.Photo, Followers = a.Artist.Users.Count, Genre = new GenreModel { Genre = a.Artist.Genre } },
                ReleasedDate = a.ReleasedDate,
                Views = a.Views,
                Photo = DomainHosting + a.Photo
            });
            return albumQuery;
        }
    }
}