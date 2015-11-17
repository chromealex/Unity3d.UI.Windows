using System;
using System.Net;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class RawDataDownloadClient : WebClient
    {
        public delegate void CompletionHandler(bool success, string reason = "");

        public CompletionHandler downloadCompletionHandler;


        public RawDataDownloadClient()
        {
        }

        public void DownloadFileAsync(string url, string filePath, CompletionHandler handler)
        {
            this.downloadCompletionHandler = handler;
            base.DownloadFileAsync(new Uri(url), filePath);
        }

        protected override void OnDownloadFileCompleted(System.ComponentModel.AsyncCompletedEventArgs e)
        {
            base.OnDownloadFileCompleted(e);
            downloadCompletionHandler(true, "Download complete");
        }
    }
}
