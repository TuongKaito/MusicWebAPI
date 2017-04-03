using OnlineMusicServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    [DataContract]
    public class RoleModel : BaseModel<Role>
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public Role Role
        {
            set
            {
                Id = value.RoleId;

            }
        }

        public override void CopyEntityData(Role role)
        {
            Name = role.RoleName;
        }

        public override void UpdateEntity(Role role)
        {
            role.RoleName = Name;
        }
    }

    public enum RoleManager
    {
        Admin = 1,
        User = 2,
        VIP = 3
    }
}