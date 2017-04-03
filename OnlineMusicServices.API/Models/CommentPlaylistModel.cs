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
    public class CommentPlaylistModel : CommentBaseModel<PlaylistComment>
    {
        [Required]
        [DataMember]
        public int PlaylistId { get; set; }

        public PlaylistComment PlaylistComment
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void CopyEntityData(PlaylistComment playlistComment)
        {
            Comment = playlistComment.Comment;
            Date = playlistComment.Date;
            UserId = playlistComment.UserId;
            PlaylistId = playlistComment.PlaylistId;
        }

        public override void UpdateEntity(PlaylistComment playlistComment)
        {
            playlistComment.Comment = Comment;
            playlistComment.UserId = UserId;
            playlistComment.PlaylistId = PlaylistId;
        }
    }
}