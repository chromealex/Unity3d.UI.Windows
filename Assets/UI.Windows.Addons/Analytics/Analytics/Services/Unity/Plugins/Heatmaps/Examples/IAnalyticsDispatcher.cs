/// <summary>
/// A possible interface for any Analytics dispatcher
/// </summary>

namespace UnityAnalyticsHeatmap
{
    public interface IAnalyticsDispatcher
    {

        void DisableAnalytics();

        void EnableAnalytics();
    }
}
