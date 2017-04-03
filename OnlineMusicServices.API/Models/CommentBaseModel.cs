using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    [DataContract]
    public abstract class CommentBaseModel<T> : BaseModel<T>
    {
        [Required]
        [DataMember]
        public long Id { get; set; }

        [Required]
        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public UserModel User { get; set; }
    }
}