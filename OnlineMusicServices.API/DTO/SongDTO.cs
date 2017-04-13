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
        private ArtistDTO artistDto;
        public string DomainHosting { get; set; }

        public SongDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.Authority}/api/resources/streaming/";
            artistDto = new ArtistDTO(uri);
        }

        public IQueryable<SongModel> GetSongQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<Song, bool>> whereClause = null)
        {
            IQueryable<Song> query = db.Songs;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            var songQuery = query.Select(Converter).AsQueryable();
            return songQuery;
        }

        public ICollection<SongModel> ConvertToSongModel(ICollection<Song> songs)
        {
            var listSongs = songs.Select(Converter).ToList();
            return listSongs;
        }

        public SongModel Converter(Song s)
        {
            return new SongModel
            {
                Title = s.Title,
                Artists = artistDto.ConvertToArtistModel(s.Artists).ToList(),
                AuthorId = s.AuthorId,
                Author = new UserModel { User = s.User },
                GenreId = s.GenreId,
                Composer = s.Composer,
                Privacy = s.Privacy,
                UploadedDate = s.UploadedDate,
                Verified = s.Verified,
                Views = (from v in s.SongViews select v.Views).FirstOrDefault(),
                Id = s.Id,
                ResourceId = s.ResourceId,
                Resource = DomainHosting + s.ResourceId,
                Genre = new GenreModel { Genre = s.Genre }
            };
        }
        
    }
}