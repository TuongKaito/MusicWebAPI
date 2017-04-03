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
    public class ArtistModel : BaseModel<Artist>
    {
        #region ArtistModel Properties
        [Required]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public string FullName { get; set; }

        [Required]
        [DataMember]
        public int GenreId { get; set; }

        [DataMember]
        public GenreModel Genre { get; set; }

        [DataMember]
        public int Gender { get; set; } = 1;

        [DataMember]
        public DateTime? DateOfBirth { get; set; }

        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string Profile { get; set; }

        [DataMember]
        public long Followers { get; set; }

        [DataMember]
        public bool Verified { get; set; }

        [DataMember]
        public string Photo { get; set; }

        #endregion

        public Artist Artist
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void UpdateEntity(Artist artist)
        {
            artist.FullName = FullName;
            artist.GenreId = GenreId > 0 ? GenreId : 1;
            artist.DateOfBirth = DateOfBirth.GetValueOrDefault();
            artist.City = City;
            artist.Profile = Profile;
            artist.Verified = Verified;
        }

        public override void CopyEntityData(Artist artist)
        {
            FullName = artist.FullName;
            GenreId = artist.GenreId;
            Gender = artist.Gender.HasValue ? artist.Gender.Value : 0;
            DateOfBirth = artist.DateOfBirth;
            City = artist.City;
            Profile = artist.Profile;
            Verified = artist.Verified;
            Photo = artist.Photo;
        }
    }
}