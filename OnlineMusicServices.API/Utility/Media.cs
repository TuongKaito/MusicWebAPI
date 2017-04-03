using OnlineMusicServices.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.Utility
{
    public class Media
    {
        public static readonly string[] AudioExtension = new String[] { "mp3", "wav" };
        public static readonly string[] VideoExtension = new String[] { "mp4", "avi", "flv" };
        public static readonly string[] ImageExtension = new String[] { "jpg", "png", "gif", "bmp" };

        public static string GetMediaTypeFromExtension(string ext)
        {
            // Remove . character of ext
            ext = ext.Remove(0, 1);
            if (AudioExtension.Any(s => s == ext))
            {
                return "audio/" + ext;
            }
            else if (VideoExtension.Any(s => s == ext))
            {
                return "video/" + ext;
            }
            else if (ImageExtension.Any(s => s == ext))
            {
                return "image/" + ext;
            }
            else
            {
                throw new InvalidDataException(String.Format("Phần mở rộng {0} không được hỗ trợ", ext));
            }
        }

        public static ResourceTypeManager GetResourceType(string ext)
        {
            // Remove . character of ext
            ext = ext.Remove(0, 1);
            if (AudioExtension.Any(s => s == ext))
            {
                return ResourceTypeManager.Audio;
            }
            else if (VideoExtension.Any(s => s == ext))
            {
                return ResourceTypeManager.Video;
            }
            else if (ImageExtension.Any(s => s == ext))
            {
                return ResourceTypeManager.Image;
            }
            else
            {
                throw new InvalidDataException(String.Format("Phần mở rộng {0} không được hỗ trợ", ext));
            }
        }
    }
}