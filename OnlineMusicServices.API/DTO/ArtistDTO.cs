﻿using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class ArtistDTO
    {
        public string DomainHosting { get; set; }
        
        public ArtistDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.DnsSafeHost}/api/resources/streaming/";
        }

        public IQueryable<ArtistModel> GetArtistQuery(OnlineMusicEntities db, Expression<Func<Artist, bool>> whereClause = null)
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
                Gender = a.Gender.GetValueOrDefault(),
                DateOfBirth = a.DateOfBirth,
                City = a.City,
                Profile = a.Profile,
                Photo = a.Photo,
                PhotoUrl = DomainHosting + a.Photo,
                Verified = a.Verified,
                Followers = a.Users.Count
            };
        }
    }
}