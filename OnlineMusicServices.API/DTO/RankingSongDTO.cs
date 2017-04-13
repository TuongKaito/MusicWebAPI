using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class RankingSongDTO
    {
        private SongDTO songDto;

        public RankingSongDTO(Uri uri)
        {
            songDto = new SongDTO(uri);
        }

        public IQueryable<RankingSongModel> GetRankingQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<RankingSong, bool>> whereClause = null)
        {
            IQueryable<RankingSong> query = db.RankingSongs;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            var rankingQuery = query.Select(Converter).AsQueryable();
            return rankingQuery;
        }

        public RankingSongModel Converter(RankingSong rs)
        {
            return new RankingSongModel()
            {
                Id = rs.Id,
                Week = rs.Week,
                Rank = rs.Rank,
                LastRank = rs.LastRank,
                StartDate = rs.StartDate,
                EndDate = rs.EndDate,
                SongId = rs.SongId,
                Song = songDto.Converter(rs.Song)
            };
        }
    }
}