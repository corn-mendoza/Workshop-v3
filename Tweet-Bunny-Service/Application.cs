using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Pivotal.Helper;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TweetBunnyService
{
    public class Application
    {
        private IQueueClient queueClient;
        private string ServiceBusConnectionString = "{Service Bus connection string}";
        private string SqlConnectionString = "{SQL connection string}";
        private string StorageEndpoint = "{Blob connection string}";
        private string QueueName = "{Queue path/name}";
        private int MaxBlobLength = 1000000;
        private int FileUploadMs = 10000;
        private string UpdateUrl = "{Rest Update URL}";
        private StorageCredentials Credentials;
        ILogger<Application> _logger;
        IConfiguration Config { get; set; }

        public Application(ILogger<Application> logger, IConfiguration configApp)
        {
            Config = configApp;
            _logger = logger;
        }

        public async Task Run()
        {
            try
            {
                _logger?.LogInformation($"This is a console application for {Config.GetValue<string>("Name")}");

                // Use the CFHelper class to load the VCAP Services and Applications from Cloud Foundry
                CFEnvironmentVariables env = new CFEnvironmentVariables();
                var _azureConnect = env.getConnectionStringForMessagingService("azure-servicebus");

                if (!string.IsNullOrEmpty(_azureConnect))
                {
                    ServiceBusConnectionString = _azureConnect;
                }
                else
                {
                    ServiceBusConnectionString = Config.GetSection("ServiceBusConnectionString").Value;
                }

                _logger?.LogInformation($"Service Bus Connection String: {ServiceBusConnectionString}");

                //var _azureDbConnect = env.getConnectionStringForAzureDbService("azure-sqldb");

                //if (!string.IsNullOrEmpty(_azureDbConnect))
                //{
                //    SqlConnectionString = _azureDbConnect;
                //}
                //else
                //{
                //    SqlConnectionString = Config.GetSection("SqlConnectionString").Value;
                //}

                //Console.WriteLine(SqlConnectionString);

                //var _azureStorageConnect = env.getAzureStorageCredentials("azure-storage");

                //if (_azureStorageConnect != null)
                //{
                //    //Console.WriteLine($"Account Name: {_azureStorageConnect.AccountName } Token: {_azureStorageConnect.Token}");
                //    Credentials = new StorageCredentials(_azureStorageConnect.AccountName, _azureStorageConnect.Token);
                //    StorageEndpoint = _azureStorageConnect.Endpoint;
                //}
                //else
                //{
                //    throw new InvalidOperationException($"Storage Endpoint not configured");
                //}

                //QueueName = Config.GetSection("QueueName").Value;
                //UpdateUrl = Config.GetSection("RestUpdateUrl").Value;

                //var _val1 = Config.GetSection("MaxBlobLength");
                //if (_val1.Value != null)
                //{
                //    MaxBlobLength = int.Parse(_val1.Value);
                //}

                //var _val2 = Config.GetSection("FileUploadMs");
                //if (_val2.Value != null)
                //{
                //    FileUploadMs = int.Parse(_val2.Value);
                //}

                _logger?.LogInformation($"Setting queue name to {QueueName}");
                _logger?.LogInformation($"Max blob length set to {MaxBlobLength}");
                _logger?.LogInformation($"File upload interval set to {FileUploadMs}");

                queueClient = new QueueClient(ServiceBusConnectionString, QueueName, ReceiveMode.PeekLock);

                _logger?.LogInformation($"Processing started for {QueueName}");

                Console.WriteLine("Press ctrl-c to stop receiving messages.");

                ReceiveMessages();

                Console.Read();

                _logger?.LogInformation($"Stopping queue {QueueName}");

                // Close the client after the ReceiveMessages method has exited.
                await queueClient.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.ToString());
            }
        }

        // Receives messages from the queue in a loop
        private void ReceiveMessages()
        {
            try
            {
                // Register a OnMessage callback
                queueClient.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        // Process the message
                        _logger?.LogInformation($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                        await MessageReceived(message);

                        // Complete the message so that it is not received again.
                        // This can be done only if the queueClient is opened in ReceiveMode.PeekLock mode.
                        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                    },

                    new MessageHandlerOptions(async (message) =>
                    {
                        _logger?.LogCritical(message.Exception.Message);
                    })
                    { MaxConcurrentCalls = 1, AutoComplete = false });
            }
            catch (Exception exception)
            {
                _logger?.LogInformation($"{DateTime.Now} > Exception: {exception.Message}");
            }
        }


        private async Task MessageReceived(Message message)
        {
            RestClient _restClient = new RestClient();
            CloudBlobClient _blobClient = new CloudBlobClient(new Uri(StorageEndpoint), Credentials);

            try
            {
                var messageBody = Encoding.UTF8.GetString(message.Body);
                var actual = JsonConvert.DeserializeObject<RepositoryMessage>(messageBody) ?? throw new InvalidOperationException("could not deserialize message");

                Console.WriteLine($"Processing file {actual.RepositoryFile.FullName}");
                Console.WriteLine($"Target container is {actual.Container}");

                // Retrieve reference to a previously created container.
                CloudBlobContainer _origcontainer = _blobClient.GetContainerReference("filesystem");

                // Retrieve reference to a blob named "photo1.jpg".
                CloudBlockBlob blockBlob = _origcontainer.GetBlockBlobReference(actual.RepositoryFile.FullName);

                // Save blob contents to a file.
                using (var fileStream = System.IO.File.OpenWrite(actual.RepositoryFile.FullName))
                {
                    await blockBlob.DownloadToStreamAsync(fileStream);
                }

                var fileInfo = new FileInfo(actual.RepositoryFile.FullName);

                // validate file exists
                if (!fileInfo.Exists)
                {
                    throw new InvalidOperationException($"file does not exist {fileInfo.FullName}");
                }

                // validate file is < max blob length
                if (fileInfo.Length > MaxBlobLength)
                {
                    throw new InvalidOperationException($"file ({fileInfo.FullName}) length ({fileInfo.Length}) exceeds max ({MaxBlobLength})");
                }

                // create a new container; make sure it has a unique name
                CloudBlobContainer container = _blobClient.GetContainerReference(actual.Container);

                // upload the blob
                var file = actual.RepositoryFile;

                try
                {
                    using (var stream = System.IO.File.OpenRead(file.FullName))
                        //using (new Timer(async state => { file.BytesWritten = stream.Position; await _restClient.PutFileUpdateAsync(file, UpdateUrl); }, null, _fileUploadUpdateMs, _fileUploadUpdateMs))
                        try
                        {
                            var blob = container.GetBlockBlobReference(file.FullName);
                            await blob.UploadFromStreamAsync(stream);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        finally
                        {
                            file.BytesWritten = stream.Position;
                            //await _restClient.PutFileUpdateAsync(file, UpdateUrl);
                            Console.Write($"File Uploaded: {file.FullName}");
                        }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
