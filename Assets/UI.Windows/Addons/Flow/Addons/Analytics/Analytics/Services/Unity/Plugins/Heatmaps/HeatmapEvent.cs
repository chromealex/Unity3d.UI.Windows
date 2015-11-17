/// <summary>
/// Adapter API for sending Heatmap analytics events
/// </summary>
/// This is <i>simply</i> an adapter. As such, you could choose not to
/// use it at all, but by passing your events through this API you gain type
/// safety and ensure that you're conforming to the data that the aggregator
/// and Heatmapper expect to receive.
/// 
/// The script is designed to work in Unity 4.6 > 5.x

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 ||  UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
using analyticsResultNamespace = UnityEngine.Cloud.Analytics;
using analyticsEventNamespace = UnityEngine.Cloud.Analytics.UnityAnalytics;


#else
using analyticsResultNamespace = UnityEngine.Analytics;
using analyticsEventNamespace = UnityEngine.Analytics.Analytics;
#endif


namespace UnityAnalyticsHeatmap
{
    public class HeatmapEvent
    {
        private static Dictionary<string, object> s_Dictionary = new Dictionary<string, object>();

        /// <summary>
        /// Send the event with position and an optional dictionary.
        /// </summary>
        /// Note that Vector2 will implicitly convert to Vector3
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 v, Dictionary<string, object> options = null)
        {
            AddXY(v.x, v.y);
            AddZ(v.z);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, time and an optional dictionary.
        /// </summary>
        /// Note that Vector2 will implicitly convert to Vector3
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 v, float time, Dictionary<string, object> options = null)
        {
            AddXY(v.x, v.y);
            AddZ(v.z);
            AddTime(time);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, time and an optional dictionary.
        /// </summary>
        /// Note that Vector2 will implicitly convert to Vector3
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Vector3 v, float time, float rotation, Dictionary<string, object> options = null)
        {
            AddXY(v.x, v.y);
            AddZ(v.z);
            s_Dictionary["rx"] = rotation;
            AddTime(time);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, rotation and an optional dictionary.
        /// </summary>
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Transform trans, Dictionary<string, object> options = null)
        {
            AddXY(trans.position.x, trans.position.y);
            AddZ(trans.position.z);
            AddRotation(trans.rotation.eulerAngles);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Send the event with position, rotation, time and an optional dictionary.
        /// </summary>
        public static analyticsResultNamespace.AnalyticsResult Send(string eventName, Transform trans, float time, Dictionary<string, object> options = null)
        {
            AddXY(trans.position.x, trans.position.y);
            AddZ(trans.position.z);
            AddRotation(trans.rotation.eulerAngles);
            AddTime(time);
            AddOptions(options);
            return Commit(eventName);
        }

        /// <summary>
        /// Transmit the event
        /// </summary>
        protected static analyticsResultNamespace.AnalyticsResult Commit(string eventName)
        {
            return analyticsEventNamespace.CustomEvent("Heatmap." + eventName, s_Dictionary);
        }

        /// <summary>
        /// Convenience method for adding X/Y to dict
        /// </summary>
        protected static void AddXY(float x, float y)
        {
            s_Dictionary["x"] = x;
            s_Dictionary["y"] = y;
        }

        /// <summary>
        /// Convenience method for adding Z to dict
        /// </summary>
        protected static void AddZ(float z)
        {
            s_Dictionary["z"] = z;
        }

        /// <summary>
        /// Convenience method for adding time to dict
        /// </summary>
        protected static void AddTime(float time)
        {
            s_Dictionary["t"] = time;
        }

        /// <summary>
        /// Convenience method for adding rotation
        /// </summary>
        protected static void AddRotation(Vector3 r)
        {
            s_Dictionary["rx"] = r.x;
            s_Dictionary["ry"] = r.y;
            s_Dictionary["rz"] = r.z;
        }

        /// <summary>
        /// Convenience method for adding options to dict
        /// </summary>
        protected static void AddOptions(Dictionary<string, object> options)
        {
            if (options != null)
            {
                foreach (KeyValuePair<string, object> entry in options)
                {
                    s_Dictionary[entry.Key] = entry.Value;
                }
            }
        }
    }
}
