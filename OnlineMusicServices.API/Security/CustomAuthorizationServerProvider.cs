using Microsoft.Owin.Security.OAuth;
using OnlineMusicServices.API.Models;
using OnlineMusicServices.API.Storage;
using OnlineMusicServices.API.Utility;
using OnlineMusicServices.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineMusicServices.API.Security
{
    public class CustomAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (var db = new OnlineMusicEntities())
            {
                var user = (from u in db.Users
                            where u.Username.ToLower() == context.UserName.ToLower()
                            select u).FirstOrDefault();

                MemoryCacher cache = new MemoryCacher();
                string cachePassword = string.Empty;
                if (user != null && cache.Get(user.Username) != null)
                    cachePassword = (string)cache.Get(user.Username);

                if (user != null && ( HashingPassword.ValidatePassword(context.Password, user.Password) || (!String.IsNullOrEmpty(cachePassword) && HashingPassword.ValidatePassword(context.Password, cachePassword)) ))
                {
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                    identity.AddClaim(new Claim(ClaimTypes.Name, user.Id.ToString()));
                    if (user.RoleId == (int)RoleManager.Administrator)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                    }
                    else if (user.RoleId == (int)RoleManager.Admin)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                    }
                    else if (user.RoleId == (int)RoleManager.VIP)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "VIP"));
                        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                    }
                    else if (user.RoleId == (int)RoleManager.User)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                    }
                    else
                    {
                        return;
                    }

                    context.Validated(identity);
                }
                else
                {
                    context.SetError("Invalid Grant", "Provided username and password is incorrect");
                    return;
                }
            }
        }
    }
}