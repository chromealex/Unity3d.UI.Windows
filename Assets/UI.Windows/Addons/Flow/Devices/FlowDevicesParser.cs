using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityEditor.UI.Windows.Plugins.Flow {

	public class DeviceInfo {
		
		public string manufacturer;
		public string model;
		public int width;
		public int height;
		public int ppi;

		public ScreenOrientation orientation;

	};

	public enum ParseDeviceResolutionType {

		PPI,
		INCHES,

	};

	public class FlowDevicesParser {

		public static List<DeviceInfo> popular = new List<DeviceInfo>();
		public static List<DeviceInfo> p720 = new List<DeviceInfo>();
		public static List<DeviceInfo> p1080 = new List<DeviceInfo>();
		public static List<DeviceInfo> p1440 = new List<DeviceInfo>();
		public static Dictionary<string, List<DeviceInfo>> manufacturerToDevices = new Dictionary<string, List<DeviceInfo>>();

		static FlowDevicesParser() {

			FlowDevicesParser.manufacturerToDevices.Clear();

			FlowDevicesParser.Parse_INTERNAL("720-800", FlowDevicesParser.p720, ParseDeviceResolutionType.PPI);
			FlowDevicesParser.Parse_INTERNAL("1080", FlowDevicesParser.p1080, ParseDeviceResolutionType.PPI);
			FlowDevicesParser.Parse_INTERNAL("1440", FlowDevicesParser.p1440, ParseDeviceResolutionType.PPI);
			FlowDevicesParser.Parse_INTERNAL("Popular", FlowDevicesParser.popular, ParseDeviceResolutionType.INCHES);

		}

		private static void Parse_INTERNAL(string file, List<DeviceInfo> output, ParseDeviceResolutionType type) {

			output.Clear();

			var data = Resources.Load("UI.Windows/Flow/Devices/" + file) as TextAsset;
			if (data != null) {

				var lines = data.text.Split('\n');
				foreach (var line in lines) {

					var info = line.Split(',');

					var device = new DeviceInfo();
					device.manufacturer = info[0].Trim();
					device.model = info[1].Trim();
					
					device.orientation = ScreenOrientation.Landscape;

					var res = info[2].Trim();
					var sres = res.Split('x');
					
					device.width = int.Parse(sres[0]);
					device.height = int.Parse(sres[1]);

					if (type == ParseDeviceResolutionType.PPI) {

						device.ppi = int.Parse(info[3].Trim());

					} else if (type == ParseDeviceResolutionType.INCHES) {

						device.ppi = FlowDevicesParser.ConvertInchesToPPI(float.Parse(info[3]), device.width, device.height);

					}

					output.Add(device);

					if (FlowDevicesParser.manufacturerToDevices.ContainsKey(device.manufacturer) == true) {

						FlowDevicesParser.manufacturerToDevices[device.manufacturer].Add(device);

					} else {

						FlowDevicesParser.manufacturerToDevices.Add(device.manufacturer, new List<DeviceInfo>() { device });

					}

				}

			}

		}

		public static int ConvertInchesToPPI(float inches, int width, int height) {

			var w2 = width * width;
			var h2 = height * height;

			var pix2 = w2 + h2;
			var pix2r = Mathf.Sqrt(pix2);

			return Mathf.RoundToInt(pix2r / inches);

		}

	}

}