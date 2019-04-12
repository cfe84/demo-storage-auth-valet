using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;

namespace demo_storage_auth_proxy
{
    public static class files
    {
        static bool IS_DEBUG = Environment.GetEnvironmentVariable("DEBUG") == "true";
        const int SAS_VALIDITY_DURATION_MINUTES = 15;
        private static bool AuthorizeRequest(HttpRequest req) {
            // This is the most basic authorization you can do ; just verifying the
            // user is authenticated.
            // In real life, you would probabily at least validate that the audience is
            // accepted, the token is up to date (although EasyAuth is doing all these
            // for you!), and user has roles that allows them to access the data.
            if (IS_DEBUG) {
                return true;
            } else {
                return req.Headers.ContainsKey("X-MS-CLIENT-PRINCIPAL");
            }
        }

        [FunctionName("files")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Blob("files/{Query.path}", FileAccess.Read)] CloudBlockBlob blob,
            ILogger log)
        {
            string path = req.Query["path"];

            if (!AuthorizeRequest(req)) {
                // This is useful only if you don't use EasyAuth. In this case,
                // you would redirect to a page that manages authentication for you, and
                // then redirect back here once auth worked.
                return new RedirectResult("/login&redirect_to=" + Uri.EscapeUriString(req.Path.ToUriComponent() + req.QueryString));
            }
           // var blob = new CloudBlockBlob(new Uri("dfs"));
            var sharedAccessSignature = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy {
                Permissions = SharedAccessBlobPermissions.Read, 
                // Always make it start earlier to account for some inconsistency between server clocks
                SharedAccessStartTime = DateTime.Now.Subtract(TimeSpan.FromMinutes(5)), 
                // If you end up having to download very large files, expiry time could be made a
                // function of file size.
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(SAS_VALIDITY_DURATION_MINUTES)
                });
            return new RedirectResult(blob.Uri.ToString() + sharedAccessSignature);
        }
    }
}
