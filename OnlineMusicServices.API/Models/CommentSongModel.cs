using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    public class CommentSongModel : CommentBaseModel<SongComment>
    {

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
            songComment.SongId = DataId;
        }

        public override void CopyEntityData(SongComment songComment)
        {
            Comment = songComment.Comment;
            Date = songComment.Date;
            UserId = songComment.UserId;
            DataId = songComment.SongId;
        }
    }
}