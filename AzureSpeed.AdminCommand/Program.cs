﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AzureSpeed.WebUI.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using NLog;

namespace AzureSpeed.AdminCommand
{

    // This tool is for azurespeed admin operations. It setups everythings needed for a storage to run speed test.
    // 1. Enable CORS
    // 2. Create a container azurespeed
    // 2. Create a callback.js which is used by latency test
    // 3. Upload a 100 mb dummy file for download speed test.

    class Program
    {
        static void Main()
        {
            Worker worker = new Worker();
            worker.InitStorage();
            Console.ReadLine();
        }
    }

    class Worker
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public void InitStorage()
        {
            foreach (var account in AzureSpeedData.Accounts)
            {
                CloudStorageAccount storageAccount;
                if (!TryParse(account, out storageAccount))
                {
                    _logger.Error("Account invalid");
                    continue;
                }

                var blobClient = storageAccount.CreateCloudBlobClient();

                _logger.Info("Enabling CORS for account {0}", account.Name);
                EnableCors(blobClient);
                _logger.Info("Enabling CORS for account {0} successfully", account.Name);

                var container = blobClient.GetContainerReference("azurespeed");
                if (container != null && !container.Exists())
                {
                    _logger.Info("Creating azurespeed container");
                    CreateAndSetupContainer(container);
                    _logger.Info("Creating azurespeed container completes");
                }

                var blob = container.GetBlockBlobReference("callback.js");
                if (blob != null && !blob.Exists())
                {
                    _logger.Info("Uploading callback.js blob");
                    CreateCallbackJSBlob(account, blob);
                    _logger.Info("Uploading callback.js blob completes");
                }

                var blob1 = container.GetBlockBlobReference("100MB.bin");
                if (blob1 != null && !blob1.Exists())
                {
                    _logger.Info("Uploading 100MB.bin blob");
                    var fullFilePath = @"C:\DelMe\100MB.bin";
                    using (var fileStream = File.OpenRead(fullFilePath))
                    {
                        blob1.UploadFromStream(fileStream);
                    }
                    _logger.Info("Uploading 100MB.bin blob completes");
                }

                if (!blob.Uri.ToString().Contains(account.Region.Replace(" ", "").ToLower()))
                {
                    _logger.Error("Storage account {0} has wrong region {1}", account.Name, account.Region);
                }

                _logger.Info("Storage account {0} successfully initilized completes", account.Name);
            }
        }

        private void EnableCors(CloudBlobClient blobClient)
        {
            try
            {
                CorsHttpMethods allowedMethods = CorsHttpMethods.None;
                allowedMethods = allowedMethods | CorsHttpMethods.Get;
                allowedMethods = allowedMethods | CorsHttpMethods.Put;
                allowedMethods = allowedMethods | CorsHttpMethods.Post;
                allowedMethods = allowedMethods | CorsHttpMethods.Delete;
                allowedMethods = allowedMethods | CorsHttpMethods.Options;

                var delimiter = new[] { "," };
                CorsRule corsRule = new CorsRule();
                const string allowedOrigins = "*";
                const string allowedHeaders = "*";
                const string exposedHeaders = "";

                string[] allAllowedOrigin = allowedOrigins.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                string[] allExpHeaders = exposedHeaders.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                string[] allAllowHeaders = allowedHeaders.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                List<string> corsAllowedOrigin = new List<string>();
                foreach (var item in allAllowedOrigin)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        corsAllowedOrigin.Add(item.Trim());
                    }
                }
                List<string> corsExposedHeaders = new List<string>();
                foreach (var item in allExpHeaders)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        corsExposedHeaders.Add(item.Trim());
                    }
                }
                List<string> corsAllowHeaders = new List<string>();
                foreach (var item in allAllowHeaders)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        corsAllowHeaders.Add(item.Trim());
                    }
                }
                corsRule.MaxAgeInSeconds = 200;
                corsRule.AllowedMethods = allowedMethods;
                corsRule.AllowedHeaders = corsAllowHeaders;
                corsRule.AllowedOrigins = corsAllowedOrigin;
                corsRule.ExposedHeaders = corsExposedHeaders;
                ServiceProperties properties = blobClient.GetServiceProperties();
                properties.Cors.CorsRules.Clear();
                properties.Cors.CorsRules.Add(corsRule);
                blobClient.SetServiceProperties(properties);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error enable CORS", ex);
            }
        }

        private void CreateCallbackJSBlob(Account account, CloudBlockBlob blob)
        {
            string callback = string.Format(@"latency._pingCallback('{0}');", account.Name);
            Stream stream = GenerateStreamFromString(callback);
            blob.UploadFromStream(stream);
            blob.Properties.ContentType = "application/javascript";
            blob.SetProperties();
        }

        private void CreateAndSetupContainer(CloudBlobContainer container)
        {
            container.Create();
            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            container.SetPermissions(permissions);
        }

        private MemoryStream GenerateStreamFromString(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s ?? ""));
        }

        private bool TryParse(Account account, out CloudStorageAccount storageAccount)
        {
            string endpoint = string.IsNullOrEmpty(account.EndpointSuffix)
                    ? ""
                    : string.Format("BlobEndpoint=https://{0}.blob.{1}/;", account.Name, account.EndpointSuffix);
            string connectionString = string.Format(
                "{0}DefaultEndpointsProtocol=https;AccountName={1};AccountKey={2}",
                endpoint, account.Name, account.Key);
            // todo: TryParse does not validate if account is valid or not.
            return CloudStorageAccount.TryParse(connectionString, out storageAccount);
        }
    }
}
