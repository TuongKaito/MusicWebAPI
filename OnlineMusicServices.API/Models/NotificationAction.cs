using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.Models
{
    public static class NotificationAction
    {
        public const string REGISTER = "register_success";
        public const string LOGIN_TEMPORARILY = "login_temporarily";
        public const string RECOVERY_PASSWORD = "recovery_password";
        public const string USER_FOLLOW = "user_follow";
        public const string UPLOAD = "upload";
        public const string ALBUM_RELEASED = "album_released";
        public const string COMMENT_AUDIO = "comment_audio";
        public const string COMMENT_ALBUM = "comment_album";
        public const string COMMENT_PLAYLIST = "comment_playlist";
        public const string VERIFIED_MEDIA = "verified_media";
    }
}