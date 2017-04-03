using OnlineMusicServices.API.Models;
using OnlineMusicServices.API.Storage;
using OnlineMusicServices.Data;
using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;

namespace OnlineMusicServices.API.Controllers
{
    [RoutePrefix("api/resources")]
    public class ResourceController : ApiController
    {
        GoogleDriveServices services;
        MemoryStream stream;

        public ResourceController()
        {
            services = new GoogleDriveServices(HttpContext.Current.Server.MapPath("~/"));
        }

        [Route("streaming/{id}")]
        [HttpGet]
        public async Task<HttpResponseMessage> StreamingResource([FromUri] string id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var resource = (from r in db.Resources
                                where r.Id == id
                                select r).FirstOrDefault();
                if (resource == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy resource");
                }

                stream = await services.Stream(id);

                var mediaType = "image/jpg";
                if (resource.Type == (int)ResourceTypeManager.Audio)
                {
                    mediaType = "audio/mpeg";
                }
                else if (resource.Type == (int)ResourceTypeManager.Video)
                {
                    mediaType = "video/mp4";
                }

                if (Request.Headers.Range != null)
                {
                    try
                    {
                        HttpResponseMessage partialResponse = Request.CreateResponse(HttpStatusCode.PartialContent);
                        partialResponse.Content = new ByteRangeStreamContent(stream, Request.Headers.Range, mediaType);
                        return partialResponse;
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
                else
                {
                    HttpContent content;
                    if (resource.Type == (int)ResourceTypeManager.Video)
                    {
                        content = new PushStreamContent((Func<Stream, HttpContent, TransportContext, Task>)WriteToStream, new MediaTypeHeaderValue(mediaType));
                    }
                    else
                    {
                        content = new StreamContent(stream);
                    }
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = content;
                    response.Content.Headers.ContentLength = stream.Length;
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                    return response;
                }
            }
        }

        [Route("download/{id}")]
        [HttpGet]
        public async Task<HttpResponseMessage> DownloadResource([FromUri] string id)
        {
            using (var db = new OnlineMusicEntities())
            {
                var resource = (from r in db.Resources
                                where r.Id == id
                                select r).FirstOrDefault();
                if (resource == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Không tìm thấy resource");
                }

                // If resource exists
                var stream = await services.Stream(id);

                HttpResponseMessage response = Request.CreateResponse();
                response.Headers.AcceptRanges.Add("bytes");
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StreamContent(stream);
                response.Content.Headers.Add("x-filename", resource.Name);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = resource.Name;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentLength = stream.Length;
                return response;
            }
        }

        private async Task WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            try
            {
                var buffer = new byte[65536];
                var length = (int)stream.Length;
                var bytesRead = 8;

                while (length > 0 && bytesRead > 0)
                {
                    bytesRead = stream.Read(buffer, 0, Math.Min(length, buffer.Length));
                    await outputStream.WriteAsync(buffer, 0, bytesRead);
                    length -= bytesRead;
                }
            }
            catch
            {
                return;
            }
            finally
            {
                outputStream.Close();
            }
        }
        
    }
    
}
