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
    public class AlbumModel : BaseModel<Album>
    {
        [Required]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [Required]
        [DataMember]
        public int GenreId { get; set; }

        [DataMember]
        public GenreModel Genre { get; set; }

        [Required]
        [DataMember]
        public int ArtistId { get; set; }

        [DataMember]
        public ArtistModel Artist { get; set; }

        [Required]
        [DataMember]
        public DateTime ReleasedDate { get; set; }

        [DataMember]
        public long Views { get; set; }

        [DataMember]
        public string Photo { get; set; }

        [DataMember]
        public string PhotoUrl { get; set; }

        public Album Album
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void CopyEntityData(Album album)
        {
            Title = album.Title;
            Description = album.Description;
            GenreId = album.GenreId;
            ArtistId = album.ArtistId;
            ReleasedDate = album.ReleasedDate;
            Photo = album.Photo;
            Genre = new GenreModel { Genre = album.Genre };
            Artist = new ArtistModel { Artist = album.Artist };
        }

        public override void UpdateEntity(Album album)
        {
            album.Title = Title;
            album.Description = Description;
            album.GenreId = GenreId;
            album.ArtistId = ArtistId;
            album.ReleasedDate = ReleasedDate;
        }
    }
}