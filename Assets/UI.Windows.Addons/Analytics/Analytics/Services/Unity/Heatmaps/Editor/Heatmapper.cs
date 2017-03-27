/// <summary>
/// Heatmapper inspector.
/// </summary>
/// This code drives the Heatmapper inspector
/// The HeatmapDataParser handles loading and parsing the data.
/// The HeatmapRendererInspector speaks to the Renderer to achieve a desired look.

using System.Collections.Generic;
using UnityAnalyticsHeatmap;
using UnityEditor;
using UnityEngine;

public class Heatmapper : EditorWindow
{
    [MenuItem("Window/Heatmapper #%h")]
    static void HeatmapperMenuOption()
    {
        EditorWindow.GetWindow(typeof(Heatmapper));
    }

    // Views
    AggregationInspector m_AggregationView;
    HeatmapDataParserInspector m_ParserView;
    HeatmapRendererInspector m_RenderView;

    // Data handlers
    RawEventClient m_EventClient = new RawEventClient();
    HeatmapAggregator m_Aggregator = new HeatmapAggregator();

    GameObject m_HeatMapInstance;

    bool m_ShowAggregate = false;
    bool m_ShowRender = false;
    bool m_LocalOnly = false;

    Dictionary<string, object> m_PointData;

    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
        {
            SystemReset();
        }
        if (GUILayout.Button("Documentation"))
        {
            Application.OpenURL("https://bitbucket.org/strangeioc/heatmaps/wiki/Home");
        }
        if (GUILayout.Button("Purge"))
        {
            PurgeData();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        if (m_AggregationView == null)
        {
            m_AggregationView = AggregationInspector.Init(m_EventClient, m_Aggregator);
        }
        m_ShowAggregate = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowAggregate, "Aggregate Events", true);
        if (m_ShowAggregate)
        {
            m_AggregationView.OnGUI();
            GUILayout.BeginHorizontal();

            m_LocalOnly = GUILayout.Toggle(m_LocalOnly, new GUIContent("Local only", "If checked, don't attempt to download raw data from the server."));
            string fetchButtonText = m_LocalOnly ? "Process" : "Fetch and Process";
            if (GUILayout.Button(fetchButtonText))
            {
                SystemProcess();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        if (m_ParserView == null)
        {
            m_ParserView = HeatmapDataParserInspector.Init(OnPointData);
        }
        if (m_RenderView == null)
        {
            m_RenderView = HeatmapRendererInspector.Init(this);
        }

        m_ShowRender = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowRender, "Render", true);
        if (m_ShowRender && m_ParserView != null)
        {
            m_ParserView.OnGUI();
            m_RenderView.OnGUI();
        }

        if (m_HeatMapInstance)
        {
            m_RenderView.SetGameObject(m_HeatMapInstance);
        }
        GUILayout.EndVertical();
    }

    void Update()
    {
        if (m_HeatMapInstance != null)
        {
            m_HeatMapInstance.GetComponent<IHeatmapRenderer>().RenderHeatmap();
        }
        if (m_RenderView != null)
        {
            m_RenderView.Update();
        }

        if (m_PointData != null)
        {
            if (m_HeatMapInstance == null)
            {
                CreateHeatmapInstance();
            }

            if (m_HeatMapInstance.GetComponent<HeatmapMeshRenderer>() != null)
            {
                m_HeatMapInstance.GetComponent<HeatmapMeshRenderer>().UpdatePointData(m_PointData["heatData"] as HeatPoint[], (float)m_PointData["maxDensity"]);
            }

            if (m_RenderView != null)
            {
                m_RenderView.SetMaxTime((float)m_PointData["maxTime"]);
                m_RenderView.SetGameObject(m_HeatMapInstance);
                m_RenderView.Update(true);
            }

            m_PointData = null;
        }
    }

    void SystemProcess()
    {
        if (m_HeatMapInstance == null)
        {
            CreateHeatmapInstance();
        }
        if (m_AggregationView != null)
        {
            m_AggregationView.Fetch(OnAggregation, m_LocalOnly);
        }
    }

    void SystemReset()
    {
        if (m_HeatMapInstance)
        {
            m_HeatMapInstance.transform.parent = null;
            DestroyImmediate(m_HeatMapInstance);
        }
    }

    void PurgeData()
    {
        if (EditorUtility.DisplayDialog("Destroy local data?", "You are about to delete your local heatmaps data cache, meaning you'll have to reload from the server. Are you sure?", "Purge", "Cancel"))
        {
            if (m_AggregationView != null)
            {
                m_AggregationView.PurgeData();
            }
        }
    }

    void OnAggregation(string jsonPath)
    {
        m_ParserView.SetDataPath(jsonPath);
    }

    void OnPointData(HeatPoint[] heatData, float maxDensity, float maxTime)
    {
        // Creating this data allows the renderer to use it on the next Update pass
        m_PointData = new Dictionary<string, object>();
        m_PointData["heatData"] = heatData;
        m_PointData["maxDensity"] = maxDensity;
        m_PointData["maxTime"] = maxTime;
    }

    /// <summary>
    /// Creates the heat map instance.
    /// </summary>
    /// We've hard-coded the Component here. Everywhere else, we use the interface.
    /// If you want to write a custom Renderer, this is the place to sub it in.
    void CreateHeatmapInstance()
    {
        m_HeatMapInstance = new GameObject();
        m_HeatMapInstance.tag = "EditorOnly";
        m_HeatMapInstance.name = "UnityAnalytics__Heatmap";
        m_HeatMapInstance.AddComponent<HeatmapMeshRenderer>();
        m_HeatMapInstance.GetComponent<IHeatmapRenderer>().allowRender = true;
    }
}
