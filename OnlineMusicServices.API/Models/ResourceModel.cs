using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    [DataContract]
    public class ResourceModel : BaseModel<Resource>
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int TypeId { get; set; }
        
        [DataMember]
        public string Type { get; set; }

        public Resource Resource
        {
            set
            {
                if (value != null)
                {
                    Id = value.Id;
                    CopyEntityData(value);
                }
            }
        }

        public override void CopyEntityData(Resource resource)
        {
            Name = resource.Name;
            TypeId = resource.Type;
        }

        public override void UpdateEntity(Resource resource)
        {
            resource.Name = Name;
            resource.Type = TypeId;
        }
    }
}