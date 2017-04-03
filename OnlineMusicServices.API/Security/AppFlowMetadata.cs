using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineMusicServices.API.Security
{
    public class AppFlowMetadata : FlowMetadata
    {
        private static readonly IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
            {
                ClientId = "82177246904-4motca7ev3jh0a66uij2p5e6ijl47ie0.apps.googleusercontent.com",
                ClientSecret = "TClo-O_HpkIR29ahJhCIU5qE"
            },
            Scopes = new[] { DriveService.Scope.Drive },
            DataStore = new FileDataStore("Drive.API.Auth.Store")
        });

        public override IAuthorizationCodeFlow Flow
        {
            get
            {
                return flow;
            }
        }

        public override string GetUserId(System.Web.Mvc.Controller controller)
        {
            return Guid.NewGuid().ToString();
        }
    }
}