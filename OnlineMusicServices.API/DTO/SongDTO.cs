using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class SongDTO
    {
        public string DomainHosting { get; set; }

        public SongDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.Authority}/api/resources/streaming/";
        }

        public IQueryable<SongModel> GetSongQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<Song, bool>> whereClause = null)
        {
            IQueryable<Song> query = db.Songs;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            var songQuery = query.Select(s => new SongModel
            {
                Title = s.Title,
                Artists = s.Artists.Select(a => new ArtistModel { Artist = a, Photo = DomainHosting + a.Photo, Followers = a.Users.Count, Genre = new GenreModel { Genre = a.Genre } }).ToList(),
                AuthorId = s.AuthorId,
                Author = new UserModel { User = s.User },
                GenreId = s.GenreId,
                Composer = s.Composer,
                Privacy = s.Privacy,
                UploadedDate = s.UploadedDate,
                Verified = s.Verified,
                Views = s.Views,
                Id = s.Id,
                ResourceId = s.ResourceId,
                Resource = DomainHosting + s.ResourceId,
                Genre = new GenreModel { Genre = s.Genre }
            });
            return songQuery;
        }

        public ICollection<SongModel> ConvertToSongModel(ICollection<Song> songs)
        {
            var listSongs = songs.Select(s => new SongModel
            {
                Title = s.Title,
                Artists = s.Artists.Select(a => new ArtistModel { Artist = a, Photo = DomainHosting + a.Photo, Followers = a.Users.Count, Genre = new GenreModel { Genre = a.Genre } }).ToList(),
                AuthorId = s.AuthorId,
                Author = new UserModel { User = s.User },
                GenreId = s.GenreId,
                Composer = s.Composer,
                Privacy = s.Privacy,
                UploadedDate = s.UploadedDate,
                Verified = s.Verified,
                Views = s.Views,
                Id = s.Id,
                ResourceId = s.ResourceId,
                Resource = DomainHosting + s.ResourceId,
                Genre = new GenreModel { Genre = s.Genre }
            }).ToList();
            return listSongs;
        }
        
    }
}