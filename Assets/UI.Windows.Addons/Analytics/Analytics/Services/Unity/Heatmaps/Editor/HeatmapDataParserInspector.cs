/// <summary>
/// Heat map data parser.
/// </summary>
/// This portion of the Heatmapper opens a JSON file and processes it into an array
/// of point data.
/// OnGUI functionality displays the state of the data in the Heatmapper inspector.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapDataParserInspector
    {
        const string k_DataPathKey = "UnityAnalyticsHeatmapDataPath";

        string m_Path = "";

        Dictionary<string, HeatPoint[]> m_HeatData;
        float m_MaxDensity = 0;
        float m_MaxTime = 0;
        int m_OptionIndex = 0;
        string[] m_OptionKeys;

        public delegate void PointHandler(HeatPoint[] heatData, float maxDensity, float maxTime);

        PointHandler m_PointHandler;

        HeatmapDataParser m_DataParser = new HeatmapDataParser();


        public HeatmapDataParserInspector(PointHandler handler)
        {
            m_PointHandler = handler;
            m_Path = EditorPrefs.GetString(k_DataPathKey);
        }

        public static HeatmapDataParserInspector Init(PointHandler handler)
        {
            return new HeatmapDataParserInspector(handler);
        }

        void Dispatch()
        {
            m_PointHandler(m_HeatData[m_OptionKeys[m_OptionIndex]], m_MaxDensity, m_MaxTime);
        }

        public void SetDataPath(string jsonPath)
        {
            m_Path = jsonPath;
            m_DataParser.LoadData(m_Path, ParseHandler);
        }

        public void OnGUI()
        {
            if (m_HeatData != null && m_OptionKeys != null && m_OptionIndex > -1 && m_OptionIndex < m_OptionKeys.Length && m_HeatData.ContainsKey(m_OptionKeys[m_OptionIndex]))
            {
                int oldIndex = m_OptionIndex;
                m_OptionIndex = EditorGUILayout.Popup("Option", m_OptionIndex, m_OptionKeys);
                if (m_OptionIndex != oldIndex)
                {
                    RecalculateMax();
                    Dispatch();
                }
            }
        }

        void RecalculateMax()
        {
            HeatPoint[] points = m_HeatData[m_OptionKeys[m_OptionIndex]];
            m_MaxDensity = 0;
            m_MaxTime = 0;

            for (int i = 0; i < points.Length; i++)
            {
                m_MaxDensity = Mathf.Max(m_MaxDensity, points[i].density);
                m_MaxTime = Mathf.Max(m_MaxTime, points[i].time);
            }
        }

        void ParseHandler(Dictionary<string, HeatPoint[]> heatData, float maxDensity, float maxTime, string[] options)
        {
            m_HeatData = heatData;
            if (heatData != null)
            {
                m_OptionKeys = options;
                m_OptionIndex = 0;
                m_MaxDensity = maxDensity;
                m_MaxTime = maxTime;
                Dispatch();
            }
        }
    }
}
