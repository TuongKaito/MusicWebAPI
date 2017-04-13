using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class ArtistDTO
    {
        public string DomainHosting { get; set; }
        
        public ArtistDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.Authority}/api/resources/streaming/";
        }

        public IQueryable<ArtistModel> GetArtistQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<Artist, bool>> whereClause = null)
        {
            IQueryable<Artist> query = db.Artists;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            IQueryable<ArtistModel> queryArtists = query.Select(Converter).AsQueryable();
            return queryArtists;
        }

        public ICollection<ArtistModel> ConvertToArtistModel(ICollection<Artist> artists)
        {
            var artistList = artists.Select(Converter).ToList();
            return artistList;
        }

        public ArtistModel Converter(Artist a)
        {
            return new ArtistModel()
            {
                Id = a.Id,
                FullName = a.FullName,
                GenreId = a.GenreId,
                Genre = new GenreModel() { Genre = a.Genre },
                Gender = a.Gender.Value,
                DateOfBirth = a.DateOfBirth,
                City = a.City,
                Profile = a.Profile,
                Photo = DomainHosting + a.Photo,
                Verified = a.Verified,
                Followers = a.Users.Count
            };
        }
    }
}