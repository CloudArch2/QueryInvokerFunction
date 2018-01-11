using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using FidoFetch.Models;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.Azure.ServiceBus;

namespace FidoFetch
{
    public static class RegisterQuery
    {
        [FunctionName(name: "InvokeQuery")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"Webhook was triggered!");

            try
            {

                // Deserialize the request
                var request = new Request();
                var ms = await req.Content.ReadAsStreamAsync();
                RegisterQuery.FromJSON<Request>(ms, out request);
                log.Info("Request Deserialized");


                //create queryevent
                var qe = new QueryEvent(request);
                log.Info("QueryEvent Created");

                //create blob
                log.Info($"Creating Blob contrainer");
                await RegisterQuery.CreateBlobContainer(qe.QueryID);
                log.Info("Blob " + qe.QueryID + " Created");

                log.Info("Publishing topic");
                // publish event
                RegisterQuery.PublishTopic(qe);
                log.Info("Topic Published");

                return req.CreateResponse(HttpStatusCode.OK, new
                {
                    body = string.Format("{{\"QueryID\" : \"{0}\"}}", qe.QueryID)
                });
            }
            catch (Exception e)
            {
                log.Info("Error Occured");
                log.Info(e.InnerException.ToString());
                return req.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    body = string.Format("{{\"Error\" : \"{0}\"}}", e.ToString())
                });
            }
        }

        /// <summary>
        /// Basic operations to work with block blobs
        /// </summary>
        /// <returns>A Task object.</returns>
        private static async Task CreateBlobContainer(string containerName)
        {
 
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=cloud2fidooutputstorage;AccountKey=7yLa9oTFHJtHDYa0VyzStRaxAw5pWIYgkttroCdg1cwV1EhGe+IMzpkZF0ELAEaov4U/1bSK4W4gEAf2Z/lLUQ==;EndpointSuffix=core.windows.net";

    

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("BlobConnectionString");
            }

            // Retrieve storage account information from connection string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a blob client for interacting with the blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Create a container for organizing blobs within the storage account.
            Console.WriteLine("1. Creating Container");
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            try
            {
                // The call below will fail if the sample is configured to use the storage emulator in the connection string, but 
                // the emulator is not running.
                // Change the retry policy for this call so that if it fails, it fails quickly.
                BlobRequestOptions requestOptions = new BlobRequestOptions() { RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.NoRetry() };
                await container.CreateIfNotExistsAsync(requestOptions, null);

                var permissions = new BlobContainerPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                await container.SetPermissionsAsync(permissions);
            }
            catch (StorageException)
            {
                Console.WriteLine("If you are running with the default connection string, please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }

        }

        public static async void PublishTopic(QueryEvent qe)
        {
            const string ServiceBusConnectionString = "Endpoint=sb://cloud2fidosb.servicebus.windows.net/;SharedAccessKeyName=TheWriter;SharedAccessKey=0c3IeFz15pod35soz15yzBVtyj/Pyxx856Q3ty82ufA=";
            const string TopicName = "cloud2fidotopic";

            ITopicClient topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

            var message = new Message(Encoding.UTF8.GetBytes(RegisterQuery.ToJSON<QueryEvent>(qe)));
            await topicClient.SendAsync(message);

            await topicClient.CloseAsync();
        }
       
        public static string ToJSON<T>(T input)
        { 
            //Create a stream to serialize the object to.  
            MemoryStream ms = new MemoryStream();

            // Serializer the User object to the stream.  
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, input);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }

        public static bool FromJSON<T> (Stream s, out T output) where T : class
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            output = ser.ReadObject(s) as T;
            s.Close();

            return true;
        }

    }
}
