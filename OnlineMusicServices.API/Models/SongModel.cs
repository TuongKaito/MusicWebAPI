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
    public class SongModel : BaseModel<Song>
    {
        #region SongModel Properties
        [Required]
        [DataMember]
        public long Id { get; set; }

        [Required]
        [DataMember]
        public string Title { get; set; }

        [Required]
        [DataMember]
        public int GenreId { get; set; }

        [Required]
        [DataMember]
        public int AuthorId { get; set; }

        [DataMember]
        public string Composer { get; set; }

        [DataMember]
        public DateTime UploadedDate { get; set; } = DateTime.Now;

        [DataMember]
        public long Views { get; set; }

        [DataMember]
        public bool Verified { get; set; } = false;

        [DataMember]
        public string ResourceId { get; set; }

        [DataMember]
        public bool Privacy { get; set; } = true;

        [DataMember]
        public bool Official { get; set; }

        [DataMember]
        public GenreModel Genre { get; set; }

        [DataMember]
        public UserModel Author { get; set; }

        [DataMember]
        public string Resource { get; set; }

        [DataMember]
        public ICollection<ArtistModel> Artists { get; set; } = new List<ArtistModel>();

        #endregion

        public override void UpdateEntity(Song song)
        {
            song.Title = Title;
            song.GenreId = GenreId;
            song.AuthorId = AuthorId;
            song.Composer = Composer;
            song.Verified = Verified;
            song.Privacy = Privacy;
            song.Official = Official;
        }

        public override void CopyEntityData(Song song)
        {
            Title = song.Title;
            GenreId = song.GenreId;
            AuthorId = song.AuthorId;
            Composer = song.Composer;
            UploadedDate = song.UploadedDate;
            Verified = song.Verified;
            ResourceId = song.ResourceId;
            Privacy = song.Privacy;
            Official = song.Official;
        }
    }
}