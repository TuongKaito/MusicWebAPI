using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    [DataContract]
    public class GenreModel : BaseModel<Genre>
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Category { get; set; }

        public Genre Genre
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void UpdateEntity(Genre genre)
        {
            genre.Name = Name;
            genre.Category = Category;
        }

        public override void CopyEntityData(Genre genre)
        {
            Name = genre.Name;
            Category = genre.Category;
        }
    }
}