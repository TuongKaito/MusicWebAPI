using OnlineMusicServices.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace OnlineMusicServices.API.Models
{
    [DataContract]
    public class UserModel : BaseModel<User>
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public int RoleId { get; set; }

        [DataMember]
        public bool Blocked { get; set; }

        public User User
        {
            set
            {
                Id = value.Id;
                CopyEntityData(value);
            }
        }

        public override void CopyEntityData(User user)
        {
            Username = user.Username;
            Email = user.Email;
            RoleId = user.RoleId;
            Blocked = user.Blocked;
        }

        public override void UpdateEntity(User user)
        {
            user.Username = Username;
            user.Email = Email;
            user.RoleId = RoleId;
            user.Blocked = Blocked;
        }
    }

    [DataContract]
    public class UserLoginModel : BaseModel<User>
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public int RoleId { get; set; } = (int) RoleManager.User;

        [DataMember]
        public bool Blocked { get; set; } = false;

        public override void CopyEntityData(User user)
        {
            Username = user.Username;
            Password = user.Password;
            Email = user.Email;
            RoleId = user.RoleId;
            Blocked = user.Blocked;
        }

        public override void UpdateEntity(User user)
        {
            user.Username = Username;
            user.Password = Password;
            user.Email = Email;
        }

    }

    [DataContract]
    public class PasswordModel
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string OldPassword { get; set; }

        [DataMember]
        public string NewPassword { get; set; }
    }
}