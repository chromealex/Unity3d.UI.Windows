/// <summary>
/// Position tracker.
/// </summary>
/// Example of a Heatmap event that tracks the position of an object.
/// Be careful of tracking position (or anything else) too frequently, 
/// as this might result in ridiculous amounts of data.

using System.Collections;
using System.Collections.Generic;
using UnityAnalyticsHeatmap;
using UnityEngine;

public class PositionTracker : MonoBehaviour, IAnalyticsDispatcher
{

    [Range(0.1f, 60.0f)]
    public float trackIntervalInSeconds = 30.0f;

    private bool analyticsEnabled;

    void Start()
    {
        StartCoroutine(TrackingTick());
    }

    IEnumerator TrackingTick()
    {
        yield return new WaitForSeconds(trackIntervalInSeconds);
        if (analyticsEnabled)
        {
            HeatmapEvent.Send("PlayerPosition", transform.position, Time.timeSinceLevelLoad);
            StartCoroutine(TrackingTick());
        }
    }

    public void DisableAnalytics()
    {
        analyticsEnabled = false;
    }

    public void EnableAnalytics()
    {
        analyticsEnabled = true;
        StartCoroutine(TrackingTick());
    }
}
