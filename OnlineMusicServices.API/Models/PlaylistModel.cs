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
    public class PlaylistModel : BaseModel<Playlist>
    {
        [Required]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [Required]
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public UserModel User { get; set; }

        [DataMember]
        public long Views { get; set; } = 0;

        [DataMember]
        public string Photo { get; set; }

        public override void UpdateEntity(Playlist playlist)
        {
            playlist.Title = Title;
            playlist.Description = Description;
            playlist.UserId = UserId;
        }

        public override void CopyEntityData(Playlist playlist)
        {
            Title = playlist.Title;
            Description = playlist.Description;
            CreatedDate = playlist.CreatedDate;
            UserId = playlist.UserId;
            Views = playlist.Views;
            Photo = playlist.Photo;
        }
    }
}