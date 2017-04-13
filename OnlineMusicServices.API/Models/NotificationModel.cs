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
    public class NotificationModel : BaseModel<Notification>
    {
        [Required]
        [DataMember]
        public long Id { get; set; }
        
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Message { get; set; }

        [Required]
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public bool IsMark { get; set; }

        [DataMember]
        public string Action { get; set; }

        public Notification Notification
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void UpdateEntity(Notification notification)
        {
            notification.Title = Title;
            notification.Message = Message;
            notification.UserId = UserId;
            notification.IsMark = false;
            notification.CreatedAt = DateTime.Now;
            notification.Action = Action;
        }

        public override void CopyEntityData(Notification notification)
        {
            Title = notification.Title;
            Message = notification.Message;
            UserId = notification.UserId;
            CreatedAt = notification.CreatedAt;
            IsMark = notification.IsMark;
            Action = notification.Action;
        }
    }
}