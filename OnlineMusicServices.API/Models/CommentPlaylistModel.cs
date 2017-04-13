using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    public class CommentPlaylistModel : CommentBaseModel<PlaylistComment>
    {

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
            DataId = playlistComment.PlaylistId;
        }

        public override void UpdateEntity(PlaylistComment playlistComment)
        {
            playlistComment.Comment = Comment;
            playlistComment.UserId = UserId;
            playlistComment.PlaylistId = (int)DataId;
        }
    }
}