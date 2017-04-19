using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace OnlineMusicServices.API.Storage
{
    public class GoogleDriveServices
    {
        // Shouldn't delete these folders on drive
        // Folders in root folder
        public const string VIDEOS = "0Bw4-TAlJVHs0aHRmdlBsOGQzMFk";
        public const string IMAGES = "0Bw4-TAlJVHs0Y2VaN0FKVUc0WE0";
        public const string MUSICS = "0Bw4-TAlJVHs0cWVwbmVUZW9Sa0k";

        // Folders in Images folder
        public const string AVATARS = "0Bw4-TAlJVHs0eVJVemJYY2NySEk";
        public const string ARTISTS = "0Bw4-TAlJVHs0NnBzYXpTZ0xEZk0";
        public const string ALBUMS = "0Bw4-TAlJVHs0ZW5TT1p5VUtfckE";
        public const string PLAYLISTS = "0Bw4-TAlJVHs0RFlJZzRiZVBWdTA";

        // Default images in app
        public const string DEFAULT_ADMIN = "0Bw4-TAlJVHs0RU9DUmhwSmlnNGs";
        public const string DEFAULT_AVATAR = "0Bw4-TAlJVHs0MkNOSUxtX05uWUU";
        public const string DEFAULT_ALBUM = "0Bw4-TAlJVHs0MW1QM3l2MmdWckU";
        public const string DEFAULT_ARTIST = "0Bw4-TAlJVHs0aHBMODZvaElGMTQ";
        public const string DEFAULT_PLAYLIST = "0Bw4-TAlJVHs0LWIxUWlnRWVWYVE";

        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Musikai";
        static DriveService driveService;

        public GoogleDriveServices(string serverPath)
        {
            UserCredential credential;

            using (var stream = new FileStream(serverPath + "/Storage/client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(serverPath, ".credentials/drive-for-musikai.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }

        public string SearchFolder(string name, string rootFolder = "root", bool isFile = false)
        {
            string id = null;
            FilesResource.ListRequest listRequest = driveService.Files.List();
            listRequest.Q = $"'{rootFolder}' in parents";
            listRequest.Q += " and trashed=false";
            listRequest.Q += $" and name='{name}'";
            listRequest.Q += $" and mimeType {(isFile ? "!=" : "=")} 'application/vnd.google-apps.folder'";
            listRequest.Fields = "files(name, id)";

            try
            {
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
                if (files != null && files.Count > 0)
                {
                    id = files[0].Id;
                }
                else
                {
                    id = CreateFolder(name, rootFolder);
                    Console.WriteLine("Not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return id;
        }

        public string CreateFolder(string folderName, string parentId = "root")
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { parentId }
            };
            var request = driveService.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();
            Console.WriteLine("Created folder with ID: " + file.Id);
            return file.Id;
        }

        public string UploadFile(Stream fileStream, string fileName, string contentType, string folderId = "root")
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { folderId }
            };
            FilesResource.CreateMediaUpload request;
            request = driveService.Files.Create(fileMetadata, fileStream, contentType);
            request.Fields = "id, name";
            request.Upload();
            return request.ResponseBody?.Id;
        }

        public MemoryStream DownloadFile(string fileId)
        {
            var request = driveService.Files.Get(fileId);
            var stream = new MemoryStream();
            
            request.Download(stream);
            return stream;
        }

        public bool DeleteFile(string id)
        {
            try
            {
                driveService.Files.Delete(id).Execute();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async System.Threading.Tasks.Task<MemoryStream> Stream(string id)
        {
            var bytes = await driveService.HttpClient.GetByteArrayAsync($"https://www.googleapis.com/drive/v3/files/{id}?alt=media");
            var stream = new MemoryStream(bytes);
            return stream;
        }
    }
}