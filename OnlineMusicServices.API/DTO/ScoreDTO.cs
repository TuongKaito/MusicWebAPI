using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class ScoreDTO
    {
        public string DomainHosting { get; set; }
        private UserInfoDTO userInfoDto;

        public ScoreDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.Authority}/api/resources/streaming/";
            userInfoDto = new UserInfoDTO(uri);
        }

        public IQueryable<ScoreModel> GetScoreQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<Score, bool>> whereClause = null)
        {
            IQueryable<Score> query = db.Scores;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            var scoreQuery = query.Select(Converter).AsQueryable();
            return scoreQuery;
        }

        public ScoreModel Converter(Score s)
        {
            return new ScoreModel()
            {
                Id = s.Id,
                UserId = s.UserId,
                Score = s.Score1,
                UserInfo = userInfoDto.ConvertToUserInfoModel(s.User.UserInfoes).FirstOrDefault()
            };
        }

    }
}