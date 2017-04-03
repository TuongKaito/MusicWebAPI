using ImageResizer;
using System.IO;

namespace OnlineMusicServices.API.Utility
{
    public class ImageFactory
    {
        private const int IMAGE_WIDTH = 500;
        private const int IMAGE_HEIGHT = 500;

        public static Stream Resize(Stream image)
        {
            var result = new MemoryStream();
            result.Seek(0, SeekOrigin.Begin);
            ImageJob job = new ImageJob(image, result, new Instructions()
            {
                Width = IMAGE_WIDTH,
                Height = IMAGE_HEIGHT,
                Mode = FitMode.Stretch
            });
            job.Build();
            return result;
        }
    }
}