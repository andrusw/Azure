//Nuget packages: 
//Microsoft.Azure.DataLake.Store
//Microsoft.Rest.ClientRuntime.Azure.Authentication

using Microsoft.Azure.DataLake.Store;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using System;
using System.Text;
using System.Threading;

namespace UploadToDataLake
{
    class Program
    {
        private static System.Uri ADL_TOKEN_AUDIENCE = new System.Uri(@"https://datalake.azure.net/");

        //Found in data lake > Data Explorer > select the root > Folder properties > path
        //"<DATA-LAKE-STORAGE-GEN1-NAME>.azuredatalakestore.net";
        private static string _adlsg1AccountName = "datalere.azuredatalakestore.net";

        // Service principal / appplication authentication with client secret / key
        // Use the client ID of an existing AAD "Web App" application.

        //Go to Active Directory > App registrations > Endpoints (loacated as a button at top) 
        //Copy out from one of the textboxes the guid
        //" < AAD-directory-domain>";
        private static string TENANT = "21c44f3b-c9a9-4937-8b56-c047c890a093"; 

        //Go to Active Directory > App registrations > select the service app using > Application ID
        //" < AAD_WEB_APP_CLIENT_ID>";
        private static string CLIENTID = "96077a48-f567-4c83-9ca8-21f3d107fcb5";

        //One time visible when app service was created and should be saved somewhere. Like lastpass
        //" < AAD_WEB_APP_SECRET_KEY>";
        private static string secret_key = "bh316soEd0kTgyrA0h6zI+FlKsFmmZ8IMsG+z2TaGR8="; 


        public static void Main()
        {
            //Get Cred          
            var adlCreds = GetCreds_SPI_SecretKey(TENANT, ADL_TOKEN_AUDIENCE, CLIENTID, secret_key);

            // Create client objects
            AdlsClient client = AdlsClient.CreateClient(_adlsg1AccountName, adlCreds);

            string fileName = "/Upload/test"+ DateTime.Now.ToString("yyyyMMddHHmmss")+".txt";

            //Create a file and directory
            using (var stream = client.CreateFile(fileName, IfExists.Overwrite))
            {
                byte[] textByteArray = Encoding.UTF8.GetBytes("This is test data to write.\r\n");
                stream.Write(textByteArray, 0, textByteArray.Length);

                textByteArray = Encoding.UTF8.GetBytes("This is the second line.\r\n");
                stream.Write(textByteArray, 0, textByteArray.Length);
            }
        }

        /// <summary>
        /// Helper function
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="tokenAudience"></param>
        /// <param name="clientId"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        private static ServiceClientCredentials GetCreds_SPI_SecretKey(string tenant, Uri tokenAudience, string clientId, string secretKey)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
            serviceSettings.TokenAudience = tokenAudience;

            var creds = ApplicationTokenProvider.LoginSilentAsync(
             tenant,
             clientId,
             secretKey,
             serviceSettings).GetAwaiter().GetResult();
            return creds;
        }
    }
}
