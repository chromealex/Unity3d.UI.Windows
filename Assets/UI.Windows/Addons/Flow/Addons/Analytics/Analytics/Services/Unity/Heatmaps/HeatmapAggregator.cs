/// <summary>
/// Handles aggregation of raw data into heatmap data.
/// </summary>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapAggregator
    {

        int m_ReportFiles = 0;
        int m_ReportRows = 0;
        int m_ReportLegalPoints = 0;

        Dictionary<Tuplish, Dictionary<string, float>> m_PointDict;

        // Stored because we need to apply it at non-main-thread time
        string m_PersistentDataPath = Application.persistentDataPath;

        public delegate void CompletionHandler(string jsonPath);

        CompletionHandler m_CompletionHandler;


        public HeatmapAggregator()
        {
        }

        /// <summary>
        /// Process the specified inputFiles, using the other specified parameters.
        /// </summary>
        /// <param name="inputFiles">A list of one or more raw data text files.</param>
        /// <param name="startDate">Any timestamp prior to this ISO 8601 date will be trimmed.</param>
        /// <param name="endDate">Any timestamp after to this ISO 8601 date will be trimmed.</param>
        /// <param name="space">A smoothing value for x/y/z coordinates.</param>
        /// <param name="time">A smoothing value for time.</param>
        /// <param name="disaggregateTime">If set to <c>true</c> events that match in space but not time will not be aggregated.</param>
        /// <param name="events">A list of events to explicitly include.</param>
        public void Process(CompletionHandler completionHandler,
            List<string> inputFiles, DateTime startDate, DateTime endDate,
            float space, float time, float angle,
            bool disaggregateTime, bool disaggregateAngle,
            List<string> events = null)
        {
            m_CompletionHandler = completionHandler;

            string outputFileName = System.IO.Path.GetFileName(inputFiles[0]).Replace(".txt", ".json");
            var outputData = new Dictionary<string, List<Dictionary<string, float>>>();

            m_ReportFiles = 0;
            m_ReportLegalPoints = 0;
            m_ReportRows = 0;
            m_PointDict = new Dictionary<Tuplish, Dictionary<string, float>>();

            foreach (string file in inputFiles)
            {
                m_ReportFiles++;
                LoadStream(outputData, file, startDate, endDate, space, time, angle, disaggregateTime, disaggregateAngle, events, outputFileName);
            }

            // Test if any data was generated
            bool hasData = false;
            var reportList = new List<int>{ };
            foreach (var generated in outputData)
            {
                hasData = generated.Value.Count > 0;
                reportList.Add(generated.Value.Count);
                if (!hasData)
                {
                    break;
                }
            }
            if (hasData)
            {
                var reportArray = reportList.Select(x => x.ToString()).ToArray();

                //Output what happened
                string report = "Report of " + m_ReportFiles + " files:\n";
                report += "Total of " + reportList.Count + " groups numbering [" + string.Join(",", reportArray) + "]\n";
                report += "Total rows: " + m_ReportRows + "\n";
                report += "Total points analyzed: " + m_ReportLegalPoints;
                Debug.Log(report);

                SaveFile(outputFileName, outputData);
            }
            else
            {
                Debug.LogWarning("The aggregation process yielded no results.");
            }
        }

        protected void LoadStream(Dictionary<string, List<Dictionary<string, float>>> outputData,
            string path, 
            DateTime startDate, DateTime endDate, 
            float space, float time, float angle,
            bool disaggregateTime, bool disaggregateAngle,
            List<string> events, string outputFileName)
        {
            var reader = new StreamReader(path);
            using (reader)
            {
                string tsv = reader.ReadToEnd();
                string[] rows = tsv.Split('\n');
                m_ReportRows += rows.Length;

                for (int a = 0; a < rows.Length; a++)
                {
                    string[] rowData = rows[a].Split('\t');

                    if (string.IsNullOrEmpty(rowData[0]) || string.IsNullOrEmpty(rowData[2]) || string.IsNullOrEmpty(rowData[3]))
                    {
                        // Re-enable this log if you want to see empty lines
                        //Debug.Log ("Empty Line...skipping");
                        continue;
                    }

                    DateTime rowDate = DateTime.Parse(rowData[0]);

                    // Pass on rows outside any date trimming
                    if (rowDate < startDate || rowDate > endDate)
                    {
                        continue;
                    }

                    string eventName = rowData[2];
                    Dictionary<string, object> datum = MiniJSON.Json.Deserialize(rowData[3]) as Dictionary<string, object>;
					
                    // If we're filtering events, pass if not in list
                    if (events.Count > 0 && events.IndexOf(eventName) == -1)
                    {
                        continue;
                    }

                    // If no x/y, this isn't a Heatmap Event. Pass.
                    if (!datum.ContainsKey("x") || !datum.ContainsKey("y"))
                    {
                        // Re-enable this log line if you want to be see events that aren't valid for heatmapping
                        //Debug.Log ("Unable to find x/y in: " + datum.ToString () + ". Skipping...");
                        continue;
                    }

                    // Passed all checks. Consider as legal point
                    m_ReportLegalPoints++;

                    float x = float.Parse((string)datum["x"]);
                    float y = float.Parse((string)datum["y"]);

                    // z is optional
                    float z = datum.ContainsKey("z") ? float.Parse((string)datum["z"]) : 0;

                    // Round
                    x = Divide(x, space);
                    y = Divide(y, space);
                    z = Divide(z, space);

                    // t is optional and always 0 if we're not disaggregating
                    float t = !datum.ContainsKey("t") || !disaggregateTime ? 0 : float.Parse((string)datum["t"]);
                    t = Divide(t, time);

                    // rotation values are optional and always 0 if we're not disaggragating
                    float rx = !datum.ContainsKey("rx") || !disaggregateAngle ? 0 : float.Parse((string)datum["rx"]);
                    rx = Divide(rx, angle);
                    float ry = !datum.ContainsKey("ry") || !disaggregateAngle ? 0 : float.Parse((string)datum["ry"]);
                    ry = Divide(ry, angle);
                    float rz = !datum.ContainsKey("rz") || !disaggregateAngle ? 0 : float.Parse((string)datum["rz"]);
                    rz = Divide(rz, angle);

                    // Tuple-like key to determine if this point is unique, or needs to be merged with another
                    var tuple = new Tuplish(new object[]{ eventName, x, y, z, t, rx, ry, rz });

                    Dictionary<string, float> point;
                    if (m_PointDict.ContainsKey(tuple))
                    {
                        // Use existing point if it exists
                        point = m_PointDict[tuple];
                        point["d"] = point["d"] + 1;
                    }
                    else
                    {
                        // Create point if it doesn't exist
                        point = new Dictionary<string, float>();
                        point["x"] = x;
                        point["y"] = y;
                        point["z"] = z;
                        point["t"] = t;
                        point["rx"] = rx;
                        point["ry"] = ry;
                        point["rz"] = rz;
                        point["d"] = 1;
                        m_PointDict[tuple] = point;

                        // Create the event list if it doesn't exist
                        if (!outputData.ContainsKey(eventName))
                        {
                            outputData.Add(eventName, new List<Dictionary<string, float>>());
                        }
                        // Add the new point to the list
                        outputData[eventName].Add(point);
                    }
                }
            }
        }

        protected void SaveFile(string outputFileName, Dictionary<string, List<Dictionary<string, float>>> outputData)
        {
            string savePath = System.IO.Path.Combine(m_PersistentDataPath, "HeatmapData");
            if (!System.IO.Directory.Exists(savePath))
            {
                System.IO.Directory.CreateDirectory(savePath);
            }

            var json = MiniJSON.Json.Serialize(outputData);
            string jsonPath = savePath + Path.DirectorySeparatorChar + outputFileName;
            System.IO.File.WriteAllText(jsonPath, json);

            m_CompletionHandler(jsonPath);
        }

        protected float Divide(float value, float divisor)
        {
            float mod = value % divisor;
            float rounded = Mathf.Round(value / divisor) * divisor;
            if (mod > divisor / 2f)
            {
                rounded -= divisor / 2f;
            }
            else
            {
                rounded += divisor / 2f;
            }
            return rounded;
        }
    }

    // Unity doesn't support Tuple, so here's a Tuple-like standin
    internal class Tuplish
    {

        List<object> objects;

        internal Tuplish(params object[] args)
        {
            objects = new List<object>(args);
        }

        public override bool Equals(object other)
        {
            return objects.SequenceEqual((other as Tuplish).objects);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (object o in objects)
            {
                hash = hash * 23 + (o == null ? 0 : o.GetHashCode());
            }
            return hash;
        }

        public override string ToString()
        {
            string s = "";
            foreach (var o in objects)
            {
                s += o.ToString() + "   |||   ";
            }
            return "Tuplish: " + s + " >>> " + GetHashCode();
        }
    }
}
