﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Downloader
{
    public class Request
    {
        public Request(string address, RequestConfiguration config = null)
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri) == false)
                uri = new Uri(new Uri("http://localhost"), address);

            Address = uri;
            Configuration = config ?? new RequestConfiguration();
            ResponseHeaders = new Dictionary<string, string>();
        }


        protected const string GetRequestMethod = "GET";
        protected const string HeaderContentLengthKey = "Content-Length";
        protected const string HeaderContentDispositionKey = "Content-Disposition";
        protected Dictionary<string, string> ResponseHeaders { get; set; }
        public Uri Address { get; }
        public RequestConfiguration Configuration { get; }
        protected HttpWebRequest GetRequest(string method)
        {
            var request = (HttpWebRequest)WebRequest.CreateDefault(Address);
            request.Timeout = -1;
            request.Method = method;
            request.Accept = Configuration.Accept;
            request.KeepAlive = Configuration.KeepAlive;
            request.AllowAutoRedirect = Configuration.AllowAutoRedirect;
            request.AutomaticDecompression = Configuration.AutomaticDecompression;
            request.UserAgent = Configuration.UserAgent;
            request.ProtocolVersion = Configuration.ProtocolVersion;
            request.UseDefaultCredentials = Configuration.UseDefaultCredentials;
            request.SendChunked = Configuration.SendChunked;
            request.TransferEncoding = Configuration.TransferEncoding;
            request.Expect = Configuration.Expect;
            request.MaximumAutomaticRedirections = Configuration.MaximumAutomaticRedirections;
            request.MediaType = Configuration.MediaType;
            request.PreAuthenticate = Configuration.PreAuthenticate;
            request.Credentials = Configuration.Credentials;
            request.ClientCertificates = Configuration.ClientCertificates;
            request.Referer = Configuration.Referer;
            request.Pipelined = Configuration.Pipelined;
            request.Proxy = Configuration.Proxy;

            if (Configuration.IfModifiedSince.HasValue)
                request.IfModifiedSince = Configuration.IfModifiedSince.Value;

            return request;
        }
        public HttpWebRequest GetRequest()
        {
            return GetRequest(GetRequestMethod);
        }

        protected async Task FetchResponseHeaders()
        {
            if (ResponseHeaders.Any())
                return;
         
            var response = await GetRequest().GetResponseAsync();
            if (response?.SupportsHeaders == true)
            {
                foreach (var headerKey in response.Headers.AllKeys)
                {
                    var headerValue = response.Headers[headerKey];
                    ResponseHeaders.Add(headerKey, headerValue);
                }
            }
        }
        public async Task<long> GetFileSize()
        {
            await FetchResponseHeaders();
            if (ResponseHeaders.TryGetValue(HeaderContentLengthKey, out var contentLengthText))
            {
                if (long.TryParse(contentLengthText, out var contentLength))
                {
                    return contentLength;
                }
            }

            return -1L;
        }
        public string GetFileName()
        {
            // await FetchResponseHeaders();
            // var contentLengthKey = "Content-Length";
            // if (ResponseHeaders.TryGetValue(contentLengthKey, out var contentLengthText))
            // {
            //     if (long.TryParse(contentLengthText, out var contentLength))
            //     {
            //         return contentLength;
            //     }
            // }
            var filename = Path.GetFileName(Address.LocalPath);
            var queryIndex = filename.IndexOf("?", StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                filename = filename.Substring(0, queryIndex);
            }
            return filename;
        }
    }
}
