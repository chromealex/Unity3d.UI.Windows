/// <summary>
/// Interface for a Heat Map renderer
/// </summary>
/// If you choose to create your own custom renderer, we
/// recommend abiding by this interface.

using System;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public interface IHeatmapRenderer
    {
        /// <summary>
        /// Sets the heatmap data
        /// </summary>
        /// <param name="data">An array of HeatPoints defining the map and its density.</param>
        /// <param name="maxDensity">Density value considered to be 100%.</param>
        void UpdatePointData(HeatPoint[] data, float maxDensity);

        /// <summary>
        /// Defines the colors that draw the heatmap
        /// </summary>
        /// <param name="colors">An array of colors with which to display heat density.</param>
        void UpdateColors(Color[] colors);

        /// <summary>
        /// Tweak value thresholds that differentiate colors.
        /// </summary>
        /// By default, colors divide evenly. Use thesholds to arrange non-standard splits.
        /// <param name="thresholds">A list of floats (probably one less than the number of colors used in SetColors).</param>
        void UpdateThresholds(float[] thresholds);

        /// <summary>
        /// Updates the time limits.
        /// </summary>
        /// Allows the user to limit the display of data by time within the game.
        /// <param name="startTime">Start time.</param>
        /// <param name="endTime">End time.</param>
        void UpdateTimeLimits(float startTime, float endTime);

        /// <summary>
        /// Renders the heat map.
        /// </summary>
        void RenderHeatmap();

        /// <summary>
        /// Change the rendering style of this renderer.
        /// </summary>
        /// Currently, RenderShape includes the options CUBE, SQUARE, and TRI,
        /// and RenderDirection includes YZ, XZ and XY
        /// <param name="style">A RenderShape Enum.</param>
        /// /// <param name="style">A RenderDirection Enum.</param>
        void UpdateRenderStyle(RenderShape style, RenderDirection direction);

        /// <summary>
        /// Gets or sets the size of each point.
        /// </summary>
        /// <value>The size of the point (in Unity units).</value>
        float pointSize{ get; set; }

        /// <summary>
        /// Gating value to prevent the renderer from rendering.
        /// </summary>
        /// <value><c>true</c> if allow render; otherwise, <c>false</c>.</value>
        bool allowRender{ get; set; }

        /// <summary>
        /// The number of points currently displayed.
        /// </summary>
        /// <value>Count of currently displayed points</value>
        int currentPoints{ get; }

        /// <summary>
        /// The number of points in the current dataset.
        /// </summary>
        /// <value>Count of all points in the current set</value>
        int totalPoints{ get; }
    }
}
