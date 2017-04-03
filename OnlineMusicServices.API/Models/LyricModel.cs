using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    [DataContract]
    public class LyricModel : BaseModel<Lyric>
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string Lyric { get; set; }

        [DataMember]
        public long SongId { get; set; }

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public UserModel User { get; set; }

        [DataMember]
        public bool Verified { get; set; } = false;

        public Lyric LyricEntity
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void CopyEntityData(Lyric lyric)
        {
            Lyric = lyric.Lyric1;
            SongId = lyric.SongId;
            UserId = lyric.UserId;
            Verified = lyric.Verified;
        }

        public override void UpdateEntity(Lyric lyric)
        {
            lyric.Lyric1 = Lyric;
            lyric.SongId = SongId;
            lyric.UserId = UserId;
            lyric.Verified = Verified;
        }
    }
}