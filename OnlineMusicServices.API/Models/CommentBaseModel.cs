using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

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
        public UserInfoModel UserInfo { get; set; }

        [Required]
        [DataMember]
        public long DataId { get; set; }
        
    }
}