/// <summary>
/// Gets raw events from the Unity Analytics server.
/// </summary>
/// This is a port of get_raw_events.py, because not everyone likes to use
/// python.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityAnalyticsHeatmap.MiniJSON;

#if UNITY_EDITOR_WIN
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
#endif

namespace UnityAnalyticsHeatmap
{
    public class RawEventClient
    {

        public delegate void CompletionHandler(List<string> fileList);

        CompletionHandler m_CompletionHandler;

        List<string> m_UrlsToFetch;
        List<string> m_FilesToFetch;
        int m_DownloadedCount;

        public void Fetch(string path, bool localOnly, UnityAnalyticsEventType[] events, DateTime startDate, DateTime endDate, CompletionHandler handler)
        {
            m_UrlsToFetch = new List<string>();
            m_FilesToFetch = new List<string>();
            m_DownloadedCount = 0;

            if (startDate > endDate)
            {
                throw new Exception("End date must be before start date.");
            }

            m_CompletionHandler = handler;
            List<object> data = null;
            if (!localOnly)
            {
                // Load the Data Export Manifest
                object manifest = FetchData(path);
                data = manifest as List<object>;
            }
            if (data != null)
            {

                // We have the manifest, look for batches within the time frame
                int foundItems = 0;

                foreach (Dictionary<string, object> manifestItem in data)
                {
                    if (manifestItem.ContainsKey("generated_at") && manifestItem.ContainsKey("url"))
                    {
                        DateTime generatedAt = DateTime.Parse(manifestItem["generated_at"] as string);

                        // Ignore if outside date range
                        if (generatedAt >= startDate && generatedAt <= endDate)
                        {
                            foundItems++;
                            Dictionary<string, object> batch = FetchData(manifestItem["url"] as string) as Dictionary<string, object>;
                            List<object> batchData = batch["data"] as List<object>;
                            string batchID = batch["batchid"] as string;

                            if (batchData != null)
                            {
                                foreach (Dictionary<string, object> batchItem in batchData)
                                {
                                    string bUrl = batchItem["url"] as string;

                                    // Trim so we d/l only requested event types
                                    foreach (UnityAnalyticsEventType evt in events)
                                    {
                                        if (bUrl.IndexOf(evt.ToString()) > -1)
                                        {
                                            m_UrlsToFetch.Add(bUrl);
                                            m_FilesToFetch.Add(ConstructFileName(batchID, evt.ToString()));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (foundItems == 0)
                {
                    Debug.LogWarning("No data found within specified dates.");
                }
                else
                {
                    for (var a = 0; a < m_FilesToFetch.Count; a++)
                    {
                        string url = m_UrlsToFetch[a];
                        string filePath = m_FilesToFetch[a];
                        DownloadFile(url, filePath);
                    }
                }
            }
            else
            {
                // No internet connection, or user has chosen local mode. Return local files only.
                string savePath = GetSavePath();
                if (System.IO.Directory.Exists(savePath))
                {
                    var possibleFilesToFetch = new List<string>(Directory.GetFiles(savePath, "*.txt"));

                    m_FilesToFetch = new List<string>();

                    // Trim the local file list heuristically (i.e., make a generouse guess as to which ones are even worth looking at)
                    TimeSpan oneDay = new TimeSpan(24, 0, 0);
                    DateTime heuristicStart = startDate.Subtract(oneDay);
                    DateTime heuristicEnd = endDate.Add(oneDay);
                    for (int a = 0; a < possibleFilesToFetch.Count; a++)
                    {
                        string fileName = Path.GetFileName(possibleFilesToFetch[a]);
                        Double fileDateString = Convert.ToDouble(fileName.Substring(0, fileName.IndexOf("_")));
                        // Create a date at the epoch, then add to it
                        DateTime fileDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                        fileDate = fileDate.AddSeconds(fileDateString);
                        if (fileDate >= heuristicStart && fileDate <= heuristicEnd)
                        {
                            m_FilesToFetch.Add(possibleFilesToFetch[a]);
                        }
                    }

                }
                m_CompletionHandler(m_FilesToFetch);
            }
        }

        public void PurgeData()
        {
            string savePath = GetSavePath();

            if (System.IO.Directory.Exists(savePath))
            {
                System.IO.Directory.Delete(savePath, true);
            }
        }

        protected object FetchData(string path)
        {
            #if UNITY_EDITOR_WIN
            // Bypassing SSL security in Windows to work around a CURL bug.
            // This is insecure and should be fixed when the Engine supports SSL.
            ServicePointManager.ServerCertificateValidationCallback = delegate(System.Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) {
                return true;
            };
            #endif

            WebRequest www = WebRequest.Create(path);
            try
            {
                Stream stream = www.GetResponse().GetResponseStream();
                var reader = new StreamReader(stream);
                using (reader)
                {
                    string text = reader.ReadToEnd();
                    return Json.Deserialize(text);
                }
            }
            catch (WebException ex)
            {
                Debug.LogWarning("No web connection. Will proceed with local data if possible.\n" + ex.ToString());
                return null;
            }
        }

        protected string ConstructFileName(string batchID, string eventType)
        {
            string savePath = GetSavePath();
            return savePath + Path.DirectorySeparatorChar + batchID + "_" + eventType + ".txt";

        }

        protected void DownloadFile(string path, string filePath)
        {
            string savePath = GetSavePath();

            // Create the save path if necessary
            if (!System.IO.Directory.Exists(savePath))
            {
                System.IO.Directory.CreateDirectory(savePath);
            }

            if (File.Exists(filePath))
            {
                OnDownload(true, "Already downloaded");
            }
            else
            {
                var client = new RawDataDownloadClient();
                client.DownloadFileAsync(path, filePath, OnDownload);
            }
        }

        void OnDownload(bool success, string reason = "")
        {
            m_DownloadedCount++;

            string report = "Downloaded " + m_DownloadedCount + "/" + m_FilesToFetch.Count + ". Note: " + reason;
            Debug.Log(report);

            if (m_DownloadedCount == m_FilesToFetch.Count)
            {
                m_CompletionHandler(m_FilesToFetch);
            }
        }

        string GetSavePath()
        {
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "HeatmapData");
            return savePath;
        }
    }
}
