using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    public class RankingAlbumModel : BaseRanking
    {
        [Required]
        [DataMember]
        public int AlbumId { get; set; }

        [DataMember]
        public AlbumModel Album { get; set; }
    }
}