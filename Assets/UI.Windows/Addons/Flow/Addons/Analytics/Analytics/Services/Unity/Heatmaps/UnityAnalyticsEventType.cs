using System;

namespace UnityAnalyticsHeatmap
{
    // Note variance from C# code standard. We need these to match consts on server.
    public enum UnityAnalyticsEventType
    {
        appStart,
        appRunning,
        custom,
        transaction,
        userInfo,
        deviceInfo,
    }
}
