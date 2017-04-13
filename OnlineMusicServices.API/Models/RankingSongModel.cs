using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace OnlineMusicServices.API.Models
{
    public class RankingSongModel : BaseRanking
    {
        [DataMember]
        [Required]
        public long SongId { get; set; }

        [DataMember]
        public SongModel Song { get; set; }
        
    }
}