using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using OnlineMusicServices.API.Security;
using Owin;
using System;
using System.Web.Http;

[assembly: OwinStartup(typeof(OnlineMusicServices.API.Startup))]

namespace OnlineMusicServices.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            var myProvider = new CustomAuthorizationServerProvider();
            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                // For Dev environment only (on production should be AllowInsecureHttp = false)
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(30),
                Provider = myProvider
            };

            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            GlobalConfiguration.Configuration.Filters.Add(new CheckNullAttribute());
            GlobalConfiguration.Configuration.Filters.Add(new ValidateModelStateAttribute());
        }
    }
}