using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineMusicServices.API.DTO
{
    public class AlbumDTO
    {
        private ArtistDTO artistDto;
        public string DomainHosting { get; set; }

        public AlbumDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.DnsSafeHost}/api/resources/streaming/";
            artistDto = new ArtistDTO(uri);
        }

        public IQueryable<AlbumModel> GetAlbumQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<Album, bool>> whereClause = null)
        {
            IQueryable<Album> query = db.Albums.Where(album => album.Songs.Count > 0);
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }

            var albumQuery = query.Select(Converter).AsQueryable();
            return albumQuery;
        }

        public ICollection<AlbumModel> ConvertToAlbumModel(ICollection<Album> albumList)
        {
            var list = albumList.Select(Converter).ToList();
            return list;
        }

        public AlbumModel Converter(Album a)
        {
            return new AlbumModel()
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                GenreId = a.GenreId,
                Genre = new GenreModel { Genre = a.Genre },
                ArtistId = a.ArtistId,
                Artist = artistDto.Converter(a.Artist),
                ReleasedDate = a.ReleasedDate,
                Views = (from v in a.AlbumViews select v.Views).FirstOrDefault(),
                Photo = a.Photo,
                PhotoUrl = DomainHosting + a.Photo
            };
        }
    }
}