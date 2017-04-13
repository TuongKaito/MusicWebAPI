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
    public class BaseRanking : BaseModel<RankingSong>
    {
        [Required]
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public int Week { get; set; }
        
        [DataMember]
        public int Rank { get; set; }
        
        [DataMember]
        public int LastRank { get; set; }
        
        [DataMember]
        public DateTime StartDate { get; set; }
        
        [DataMember]
        public DateTime EndDate { get; set; } 

        public override void CopyEntityData(RankingSong rankingSong)
        {
            Week = rankingSong.Week;
            Rank = rankingSong.Rank;
            LastRank = rankingSong.LastRank;
            StartDate = rankingSong.StartDate;
            EndDate = rankingSong.EndDate;
        }

        public override void UpdateEntity(RankingSong rankingSong)
        {

        }
    }
}