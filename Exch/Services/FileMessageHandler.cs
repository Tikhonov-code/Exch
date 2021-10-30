using Exch.Controllers;
using Exch.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Exch.Services
{
    public class FileMessageHandler : IMessageHandler
    {
        private readonly string _storageFolder;
        private const string _requestFileExtension = ".req";
        private const string _responseFileExtension = ".resp";
        private static int _messegaTimeLive = 0; // Response lifetime before deletion, min

        private static readonly object _storageFolderLock = new object();

        public FileMessageHandler(IConfiguration config, IHostingEnvironment env)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            // Initial settings ----------------------------------------------------------------------------------
            _storageFolder = config.GetValue("StorageFolder", Path.Combine(env.ContentRootPath, "StorageFolder"));
            _messegaTimeLive = config.GetValue<Int32>("MessageTimeLive");// in minute
            //----------------------------------------------------------------------------------------------------

            if (!Directory.Exists(_storageFolder))
            {
                throw new ApplicationException("error -- Storage folder setting is empty!");
            }

        }
        public string CreateRequest(HttpRequest request, InputMessage message)
        {
            string source = request.Method + request.Path + DateTime.Now.ToUniversalTime();
            string requestKey;
            using (MD5 md5Hash = MD5.Create())
            {
                requestKey = GetMd5Hash(md5Hash, source);
            }

            string requestFilePath = GetRequestFilePath(requestKey);
            lock (_storageFolderLock)
            {
                using (StreamWriter file = new StreamWriter(requestFilePath))
                {
                    // write request file
                    string msg = String.Format("\r\nChannel #{0}\r\n{1}", message.channel, message.text);
                    file.WriteAsync(request.Method + request.Path+msg);
                }
            }

            return requestKey;
        }

        public ResponseResult GetResponse(string requestKey)
        {
            ResponseResult result = new ResponseResult();
            result.answer = null;
            string requestFilePath = GetRequestFilePath(requestKey);
            string responseFilePath = GetResponseFilePath(requestKey);

            lock (_storageFolderLock)
            {
                if (!File.Exists(responseFilePath))
                {
                    result.status = StatusCodes.Status204NoContent;

                    return result;
                }

                using (StreamReader file = new StreamReader(responseFilePath))
                {
                    string statusCodeString = file.ReadLine();

                    if (!int.TryParse(statusCodeString, out int code))
                    {
                        result.status = StatusCodes.Status500InternalServerError;
                        return result;
                    }
                    result.status = code;
                    result.answer = file.ReadToEnd();
                }
            }

            // Clean Storage
            //CleanStorage(requestFilePath, responseFilePath);

            return result;
        }

        private static void CleanStorage(string requestFilePath, string responseFilePath)
        {
            //check if _messegaTimeLive is over
            FileInfo fi = new FileInfo(responseFilePath);
            int live = (DateTime.Now - fi.CreationTime).Minutes;

            if (live >= _messegaTimeLive)
            {
                lock (_storageFolderLock)
                {
                    try
                    {
                        File.Delete(requestFilePath);
                    }
                    catch (Exception)
                    {
                        // TODO: 
                    }
                    try
                    {
                        File.Delete(responseFilePath);
                    }
                    catch (Exception)
                    {
                        // TODO:
                    }
                }
            }
        }

        private string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }
        private string GetRequestFilePath(string requestKey) => Path.Combine(_storageFolder, requestKey + _requestFileExtension);
        private string GetResponseFilePath(string requestKey) => Path.Combine(_storageFolder, requestKey + _responseFileExtension);

    }
}
