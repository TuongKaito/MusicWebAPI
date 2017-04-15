using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    [DataContract]
    public class ScoreModel : BaseModel<Score>
    {
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public long Score { get; set; }

        [DataMember]
        public UserInfoModel UserInfo { get; set; }

        public override void UpdateEntity(Score score)
        {
            score.UserId = UserId;
            score.Score1 = Score;
        }

        public override void CopyEntityData(Score score)
        {
            Id = score.Id;
            UserId = score.UserId;
            Score = score.Score1;
        }
    }
}