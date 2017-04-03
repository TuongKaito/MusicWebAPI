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
    public class CommentAlbumModel : CommentBaseModel<AlbumComment>
    {
        [Required]
        [DataMember]
        public int AlbumId { get; set; }

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
            AlbumId = albumComment.AlbumId;
        }

        public override void UpdateEntity(AlbumComment albumComment)
        {
            albumComment.Comment = Comment;
            albumComment.UserId = UserId;
            albumComment.AlbumId = AlbumId;
        }
    }
}