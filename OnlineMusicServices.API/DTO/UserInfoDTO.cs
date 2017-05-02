using OnlineMusicServices.API.Models;
using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.DTO
{
    public class UserInfoDTO
    {
        public string DomainHosting { get; set; }

        public UserInfoDTO(Uri uri)
        {
            DomainHosting = $"{uri.Scheme}://{uri.DnsSafeHost}/api/resources/streaming/";
        }

        public IQueryable<UserInfoModel> GetUserInfoQuery(OnlineMusicEntities db, System.Linq.Expressions.Expression<Func<UserInfo, bool>> whereClause = null)
        {
            IQueryable<UserInfo> query = db.UserInfoes;
            if (whereClause != null)
            {
                query = query.Where(whereClause);
            }
            var songQuery = query.Select(Converter).AsQueryable();
            return songQuery;
        }

        public ICollection<UserInfoModel> ConvertToUserInfoModel(ICollection<UserInfo> info)
        {
            var listInfo = info.Select(Converter).ToList();
            return listInfo;
        }

        public UserInfoModel Converter(UserInfo i)
        {
            return new UserInfoModel
            {
                Id = i.Id,
                UserId = i.UserId,
                FullName = i.FullName,
                Gender = i.Gender,
                DateOfBirth = i.DateOfBirth,
                City = i.City,
                Followers = i.User.User1.Count,
                Avatar = i.Avatar,
                AvatarUrl = DomainHosting + i.Avatar
            };
        }
    }
}