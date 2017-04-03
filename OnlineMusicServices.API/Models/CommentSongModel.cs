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
    public class CommentSongModel : CommentBaseModel<SongComment>
    {
        [Required]
        [DataMember]
        public long SongId { get; set; }

        public SongComment SongComment
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void UpdateEntity(SongComment songComment)
        {
            songComment.Comment = Comment;
            songComment.UserId = UserId;
            songComment.SongId = SongId;
        }

        public override void CopyEntityData(SongComment songComment)
        {
            Comment = songComment.Comment;
            Date = songComment.Date;
            UserId = songComment.UserId;
            SongId = songComment.SongId;
        }
    }
}