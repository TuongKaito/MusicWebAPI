using OnlineMusicServices.API.Models;
using OnlineMusicServices.API.Storage;
using OnlineMusicServices.API.Utility;
using OnlineMusicServices.Data;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web.Http;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/accounts")]
    public class AccountController : ApiController
    {
        #region Account Services

        [Authorize(Roles = "User")]
        [Route("verify")]
        [HttpPost]
        public HttpResponseMessage VerifyToken([FromBody] UserLoginModel userLogin)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users
                            where u.Username == userLogin.Username
                            select u).FirstOrDefault();
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy username=" + userLogin.Username);
                }
                var identity = (ClaimsIdentity)User.Identity;
                if (identity.Name != user.Id.ToString())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid token");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// Get accounts with uri api/accounts?page={pageNumber=1}&size={pageSize=10}
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllAccounts(int page = 1, int size = 200)
        {
            using (var db = new OnlineMusicEntities())
            {
                var listUsers = (from u in db.Users
                                 where u.RoleId != (int)RoleManager.Admin
                                 orderby u.Username
                                 select new UserModel { User = u }).Skip((page - 1) * size).Take(size).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, listUsers);
            }
        }

        /// <summary>
        /// Retrieve account by user id
        /// </summary>
        /// <param name="id">id of user</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetAccount([FromUri] int id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users
                            where u.Id == id
                            select new UserModel { User = u }).FirstOrDefault();

                if (user != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, user);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Tài khoản với id={id} không tồn tại");
                }
            }
        }

        [Route("register")]
        [HttpPost]
        public HttpResponseMessage Register(UserLoginModel user)
        {
            if (IsUsernameExisted(user.Username))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Tên đăng nhập đã tồn tại");
            }
            if (IsEmailExisted(user.Email))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Email đã được sử dụng");
            }

            using (var db = new OnlineMusicEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var newUser = new User();
                        user.Password = HashingPassword.HashPassword(user.Password);
                        user.UpdateEntity(newUser);
                        newUser.RoleId = (int)RoleManager.User;
                        newUser.Blocked = false;

                        db.Users.Add(newUser);
                        db.SaveChanges();

                        var userInfo = new UserInfo();
                        userInfo.UserId = newUser.Id;
                        if (newUser.RoleId == (int)RoleManager.Admin)
                        {
                            userInfo.Avatar = Storage.GoogleDriveServices.DEFAULT_ADMIN;
                        }
                        else
                        {
                            userInfo.Avatar = Storage.GoogleDriveServices.DEFAULT_AVATAR;
                        }

                        db.UserInfoes.Add(userInfo);
                        db.SaveChanges();

                        Notification notification = new Notification() {
                            Title = "Chào, " + newUser.Username,
                            Message = "Chào mừng bạn đến với ứng dụng nghe nhạc đỉnh cao Musikai\n mọi thắc mắc có thể liên hệ qua mail tuong.adm13@gmail.com",
                            UserId = newUser.Id,
                            IsMark = false,
                            CreatedAt = DateTime.Now,
                            Action = NotificationAction.REGISTER
                        };
                        db.Notifications.Add(notification);
                        db.SaveChanges();

                        transaction.Commit();

                        return Request.CreateResponse(HttpStatusCode.Created, new UserModel { User = newUser });
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// User login
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        public HttpResponseMessage Login([FromBody]UserLoginModel userLogin)
        {
            try
            {
                using(var db = new OnlineMusicEntities())
                {
                    bool success = false;
                    var user = (from u in db.Users
                                     where u.Username.ToLower() == userLogin.Username.ToLower()
                                     select u).FirstOrDefault();

                    if (user != null)
                    {
                        MemoryCacher cache = new MemoryCacher();
                        string cachePassword = cache.Get(user.Username) != null ? (string)cache.Get(user.Username) : String.Empty;
                        success = HashingPassword.ValidatePassword(userLogin.Password, user.Password);
                        if (!success)
                        {
                            success = !String.IsNullOrEmpty(cachePassword) && HashingPassword.ValidatePassword(userLogin.Password, cachePassword);
                            if (success)
                            {
                                Notification notification = new Notification()
                                {
                                    Title = "Đăng nhập với mật khẩu tạm thời",
                                    Message = "Bạn vừa đăng nhập bằng mật khẩu tạm thời của mình vào " + DateTime.Now.ToString() +
                                    "\nNếu đây không phải là bạn, khuyên cáo bạn nên đổi lại mật khẩu của mình",
                                    UserId = user.Id,
                                    IsMark = false,
                                    CreatedAt = DateTime.Now,
                                    Action = NotificationAction.LOGIN_TEMPORARILY
                                };
                                db.Notifications.Add(notification);
                                db.SaveChanges();
                            }
                        }
                    }

                    if (success)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new UserModel { User = user });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Change account password
        /// </summary>
        /// <param name="id">id of account need to change password</param>
        /// <param name="passwordModel">include oldPassword and newPassword to check valid oldPassword and change to newPassword</param>
        /// <returns></returns>
        [Authorize(Roles = "User")]
        [Route("{id}/changePassword")]
        [HttpPut]
        public HttpResponseMessage ChangePassword([FromUri] int id, PasswordModel passwordModel)
        {
            var identity = (ClaimsIdentity)User.Identity;
            if (identity.Name != id.ToString())
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Token");
            }
            using (var db = new OnlineMusicEntities())
            {
                try
                {
                    var user = (from u in db.Users
                                where u.Id == id
                                select u).FirstOrDefault();

                    if (user == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Tài khoản với id={id} không tồn tại");
                    }
                    else
                    {
                        MemoryCacher cache = new MemoryCacher();
                        string cachePassword = cache.Get(user.Username) != null ? (string)cache.Get(user.Username) : String.Empty;
                        bool isValid = HashingPassword.ValidatePassword(passwordModel.OldPassword, user.Password);
                        if (!isValid)
                        {
                            // Try check cache password
                            isValid = !String.IsNullOrEmpty(cachePassword) && HashingPassword.ValidatePassword(passwordModel.OldPassword, cachePassword);
                        }

                        if (!isValid)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Mật khẩu cũ không đúng");
                        }
                        else
                        {
                            user.Password = HashingPassword.HashPassword(passwordModel.NewPassword);
                            cache.Delete(user.Username);
                            db.SaveChanges();
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("")]
        [HttpPut]
        public HttpResponseMessage UpdateAccount([FromBody] UserModel userLogin)
        {
            try
            {
                using (var db = new OnlineMusicEntities())
                {
                    var user = (from u in db.Users
                                where u.Id == userLogin.Id
                                select u).FirstOrDefault();
                    if (user == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Tài khoản với id={userLogin.Id} không tồn tại");
                    }
                    else
                    {
                        user.Blocked = userLogin.Blocked;
                        user.RoleId = userLogin.RoleId;
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        [Route("recoveryPassword")]
        [HttpPut]
        public HttpResponseMessage RecoveryPassword([FromBody] UserModel user)
        {
            try
            {
                using (var db = new OnlineMusicEntities())
                {
                    var userData = (from u in db.Users
                                    where u.Username.ToLower() == user.Username.ToLower() && u.Email.ToLower() == user.Email.ToLower()
                                    select u).FirstOrDefault();
                    
                    if (userData == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Email sử dụng không trùng khớp với tài khoản");
                    }
                
                    MemoryCacher cache = new MemoryCacher();
                    if (cache.Get(userData.Username) == null)
                    {
                        // Recovery password for user
                        var rand = new Random();
                        byte[] randomBytes = Encoding.UTF8.GetBytes(rand.Next(100000, 999999).ToString());
                        string newPassword = Convert.ToBase64String(randomBytes);

                        string subject = "Recovery password in Musikai";
                        string htmlBody = String.Format(@"<html><body>
                            <h1>Hello, {0}</h1>
                            <p style=""font-size: 30px"">Your temporary password is <em>{1}</em></p>
                            <p style=""font-size: 27px"">The password is temporary and will expire within 3 days</p>
                            <p style=""font-size: 25px""><strong>We recommend you change your own password after you login</strong></p>
                                                    </body></html>", userData.Username, newPassword);
                        if (PostEmail.Send(userData.Email, subject, htmlBody))
                        {
                            newPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(newPassword));
                            string encryptedPassword = HashingPassword.HashPassword(newPassword);
                            cache.Add(userData.Username, encryptedPassword, DateTimeOffset.Now.AddDays(3));

                            Notification notification = new Notification()
                            {
                                Title = "Phục hồi mật khẩu",
                                Message = "Mật khẩu tạm thời của bạn đã được gửi tới email. Sau khi đăng nhập khuyên cáo bạn nên thay đổi mật khẩu của mình",
                                UserId = userData.Id,
                                IsMark = false,
                                CreatedAt = DateTime.Now,
                                Action = NotificationAction.RECOVERY_PASSWORD
                            };
                            db.Notifications.Add(notification);

                            db.SaveChanges();
                            return Request.CreateResponse(HttpStatusCode.OK, "Mật khẩu khôi phục đã được gửi tới email " + userData.Email);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.InternalServerError);
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "Mật khẩu phục hồi đã gửi tới email");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.StackTrace);
            }
        }
        
        #endregion

        private bool IsUsernameExisted(string username)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users
                            where u.Username.ToLower() == username.ToLower()
                            select u).FirstOrDefault();

                return user != null;
            }
        }

        private bool IsEmailExisted(string email)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users
                            where u.Email.ToLower() == email.ToLower()
                            select u).FirstOrDefault();

                return user != null;
            }
        }
    }
}
