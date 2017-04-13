using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    public class CommentAlbumModel : CommentBaseModel<AlbumComment>
    {
        public AlbumComment AlbumComment
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void CopyEntityData(AlbumComment albumComment)
        {
            Comment = albumComment.Comment;
            Date = albumComment.Date;
            UserId = albumComment.UserId;
            DataId = albumComment.AlbumId;
        }

        public override void UpdateEntity(AlbumComment albumComment)
        {
            albumComment.Comment = Comment;
            albumComment.UserId = UserId;
            albumComment.AlbumId = (int)DataId;
        }
    }
}