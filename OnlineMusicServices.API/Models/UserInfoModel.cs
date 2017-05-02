using OnlineMusicServices.Data;
using System;

namespace OnlineMusicServices.API.Models
{
    public class UserInfoModel : BaseModel<UserInfo>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int? Gender { get; set; } = 0;
        public DateTime? DateOfBirth { get; set; }
        public string City { get; set; }
        public string Avatar { get; set; }
        public string AvatarUrl { get; set; }
        public long Followers { get; set; }
        
        public UserInfo UserInfo
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void CopyEntityData(UserInfo userInfo)
        {
            UserId = userInfo.UserId;
            FullName = userInfo.FullName;
            Gender = userInfo.Gender;
            DateOfBirth = userInfo.DateOfBirth;
            City = userInfo.City;
            Avatar = userInfo.Avatar;
        }

        public override void UpdateEntity(UserInfo userInfo)
        {
            userInfo.FullName = FullName;
            userInfo.Gender = Gender;
            userInfo.DateOfBirth = DateOfBirth;
            userInfo.City = City;
        }
    }
}