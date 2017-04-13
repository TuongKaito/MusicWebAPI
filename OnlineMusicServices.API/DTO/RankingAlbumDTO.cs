using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class RankingAlbumDTO
    {
        private AlbumDTO albumDto;

        public RankingAlbumDTO(Uri uri)
        {
            albumDto = new AlbumDTO(uri);
        }

        public IQueryable<RankingAlbumModel> GetQueryRanking(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<RankingAlbum, bool>> whereClause = null)
        {
            IQueryable<RankingAlbum> query = db.RankingAlbums;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            var rankingQuery = query.Select(Converter).AsQueryable();
            return rankingQuery;
        }

        public RankingAlbumModel Converter(RankingAlbum ra)
        {
            return new RankingAlbumModel()
            {
                Id = ra.Id,
                Week = ra.Week,
                Rank = ra.Rank,
                LastRank = ra.LastRank,
                StartDate = ra.StartDate,
                EndDate = ra.EndDate,
                AlbumId = ra.AlbumId,
                Album = albumDto.Converter(ra.Album)
            };
        }
    }
}