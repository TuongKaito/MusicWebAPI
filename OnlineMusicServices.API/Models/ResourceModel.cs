using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    public class ResourceModel : BaseModel<Resource>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }

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
            Type = resource.Type;
        }

        public override void UpdateEntity(Resource resource)
        {
            resource.Name = Name;
            resource.Type = Type;
        }
    }
}