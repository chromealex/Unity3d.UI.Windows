/// <summary>
/// Inspector for the Aggregation portion of the Heatmapper.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class AggregationInspector
    {
        const string k_UrlKey = "UnityAnalyticsHeatmapDataExportUrlKey";

        const string k_SpaceKey = "UnityAnalyticsHeatmapAggregationSpace";
        const string k_KeyToTime = "UnityAnalyticsHeatmapAggregationTime";
        const string k_AngleKey = "UnityAnalyticsHeatmapAggregationAngle";
        const string k_AggregateTimeKey = "UnityAnalyticsHeatmapAggregationAggregateTime";
        const string k_AggregateAngleKey = "UnityAnalyticsHeatmapAggregationAggregateAngle";
        const string k_EventsKey = "UnityAnalyticsHeatmapAggregationEvents";

        const float k_DefaultSpace = 10f;
        const float k_DefaultTime = 10f;
        const float k_DefaultAngle = 15f;


        string m_RawDataPath = "";


        Dictionary<string, HeatPoint[]> m_HeatData;

        public delegate void AggregationHandler(string jsonPath);

        AggregationHandler m_AggregationHandler;

        RawEventClient m_RawEventClient;
        HeatmapAggregator m_Aggregator;

        string m_StartDate = "";
        string m_EndDate = "";
        float m_Space = k_DefaultSpace;
        float m_Time = k_DefaultTime;
        float m_Angle = k_DefaultAngle;
        bool m_AggregateTime = true;
        bool m_AggregateAngle = true;

        List<string> m_Events = new List<string>{ };

        public AggregationInspector(RawEventClient client, HeatmapAggregator aggregator)
        {
            m_Aggregator = aggregator;
            m_RawEventClient = client;

            // Restore cached paths
            m_RawDataPath = EditorPrefs.GetString(k_UrlKey);

            // Set dates based on today (should this be cached?)
            m_EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            m_StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)));

            // Restore other options
            m_Space = EditorPrefs.GetFloat(k_SpaceKey) == 0 ? k_DefaultSpace : EditorPrefs.GetFloat(k_SpaceKey);
            m_Time = EditorPrefs.GetFloat(k_KeyToTime) == 0 ? k_DefaultTime : EditorPrefs.GetFloat(k_KeyToTime);
            m_Angle = EditorPrefs.GetFloat(k_AngleKey) == 0 ? k_DefaultAngle : EditorPrefs.GetFloat(k_AngleKey);
            m_AggregateTime = EditorPrefs.GetBool(k_AggregateTimeKey);
            m_AggregateAngle = EditorPrefs.GetBool(k_AggregateAngleKey);

            // Restore list of events
            string loadedEvents = EditorPrefs.GetString(k_EventsKey);
            string[] eventsList;
            if (string.IsNullOrEmpty(loadedEvents))
            {
                eventsList = new string[]{ };
            }
            else
            {
                eventsList = loadedEvents.Split('|');
            }
            m_Events = new List<string>(eventsList);
        }

        public static AggregationInspector Init(RawEventClient client, HeatmapAggregator aggregator)
        {
            return new AggregationInspector(client, aggregator);
        }

        public void PurgeData()
        {
            m_RawEventClient.PurgeData();
        }

        public void Fetch(AggregationHandler handler, bool localOnly)
        {
            m_AggregationHandler = handler;
            if (!string.IsNullOrEmpty(m_RawDataPath))
            {
                EditorPrefs.SetString(k_UrlKey, m_RawDataPath);
                DateTime start, end;
                try
                {
                    start = DateTime.Parse(m_StartDate);
                }
                catch
                {
                    throw new Exception("The start date is not properly formatted. Correct format is YYYY-MM-DD.");
                }
                try
                {
                    end = DateTime.Parse(m_EndDate);
                }
                catch
                {
                    throw new Exception("The end date is not properly formatted. Correct format is YYYY-MM-DD.");
                }

                m_RawEventClient.Fetch(m_RawDataPath, localOnly, new UnityAnalyticsEventType[]{ UnityAnalyticsEventType.custom }, start, end, rawFetchHandler);
            }
        }

        public void OnGUI()
        {
            string oldPath = m_RawDataPath;
            m_RawDataPath = EditorGUILayout.TextField(new GUIContent("Data Export URL", "Copy the URL from the 'Editing Project' page of your project dashboard"), m_RawDataPath);
            if (oldPath != m_RawDataPath && !string.IsNullOrEmpty(m_RawDataPath))
            {
                EditorPrefs.SetString(k_UrlKey, m_RawDataPath);
            }

            m_StartDate = EditorGUILayout.TextField(new GUIContent("Start Date (YYYY-MM-DD)", "Start date as ISO-8601 datetime"), m_StartDate);
            m_EndDate = EditorGUILayout.TextField(new GUIContent("End Date (YYYY-MM-DD)", "End date as ISO-8601 datetime"), m_EndDate);

            float oldSpace = m_Space;
            m_Space = EditorGUILayout.FloatField(new GUIContent("Space Smooth", "Divider to smooth out x/y/z data"), m_Space);
            if (oldSpace != m_Space)
            {
                EditorPrefs.SetFloat(k_SpaceKey, m_Space);
            }

            GUILayout.BeginHorizontal();
            bool oldAggregateTime = m_AggregateTime;
            m_AggregateTime = EditorGUILayout.Toggle(new GUIContent("Aggregate Time", "Units of space will aggregate, but units of time won't"), m_AggregateTime);
            if (oldAggregateTime != m_AggregateTime)
            {
                EditorPrefs.SetBool(k_AggregateTimeKey, m_AggregateTime);
            }
            if (!m_AggregateTime)
            {
                float oldTime = m_Time;
                m_Time = EditorGUILayout.FloatField(new GUIContent("Smooth", "Divider to smooth out time data"), m_Time);
                if (oldTime != m_Time)
                {
                    EditorPrefs.SetFloat(k_KeyToTime, m_Time);
                }
            }
            else
            {
                m_Time = 1f;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            bool oldAggregateAngle = m_AggregateAngle;
            m_AggregateAngle = EditorGUILayout.Toggle(new GUIContent("Aggregate Direction", "Units of space will aggregate, but different angles won't"), m_AggregateAngle);
            if (oldAggregateAngle != m_AggregateAngle)
            {
                EditorPrefs.SetBool(k_AggregateAngleKey, m_AggregateAngle);
            }
            if (!m_AggregateAngle)
            {
                float oldAngle = m_Angle;
                m_Angle = EditorGUILayout.FloatField(new GUIContent("Smooth", "Divider to smooth out angle data"), m_Angle);
                if (oldAngle != m_Angle)
                {
                    EditorPrefs.SetFloat(k_AngleKey, m_Angle);
                }
            }
            else
            {
                m_Angle = 1f;
            }
            GUILayout.EndHorizontal();

            string oldEventsString = string.Join("|", m_Events.ToArray());
            if (GUILayout.Button(new GUIContent("Limit To Events", "Specify events to include in the aggregation. If specified, all other events will be excluded.")))
            {
                m_Events.Add("Event name");
            }
            for (var a = 0; a < m_Events.Count; a++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                {
                    m_Events.RemoveAt(a);
                    break;
                }
                m_Events[a] = EditorGUILayout.TextField(m_Events[a]);
                GUILayout.EndHorizontal();
            }
            string currentEventsString = string.Join("|", m_Events.ToArray());

            if (oldEventsString != currentEventsString)
            {
                EditorPrefs.SetString(k_EventsKey, currentEventsString);
            }
        }

        void rawFetchHandler(List<string> fileList)
        {
            if (fileList.Count == 0)
            {
                Debug.LogWarning("No matching data found.");
            }
            else
            {
                DateTime start, end;
                try
                {
                    start = DateTime.Parse(m_StartDate);
                }
                catch
                {
                    start = DateTime.Parse("2000-01-01");
                }
                try
                {
                    end = DateTime.Parse(m_EndDate);
                }
                catch
                {
                    end = DateTime.UtcNow;
                }

                m_Aggregator.Process(aggregationHandler, fileList, start, end, m_Space, m_Time, m_Angle, !m_AggregateTime, !m_AggregateAngle, m_Events);
            }
        }

        void aggregationHandler(string jsonPath)
        {
            m_AggregationHandler(jsonPath);
        }
    }
}
