using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("")]
    public class RootController : ApiController
    {
        public static string HostUrl { get; set; }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            if (HostUrl == null)
            {
                Uri uri = HttpContext.Current.Request.Url;
                HostUrl = $"{uri.Scheme}://{uri.DnsSafeHost}";
            }
            return Request.CreateResponse(HttpStatusCode.Forbidden);
        }
    }
}
