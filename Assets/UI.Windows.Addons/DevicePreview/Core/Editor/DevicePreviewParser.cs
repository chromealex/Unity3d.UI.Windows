using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityEditor.UI.Windows.Plugins.DevicePreview {

	public class DeviceInfo {
		
		public string manufacturer;
		public string model;
		public int width;
		public int height;
		public float ppi;
		public string deviceOutput;

		public bool current = false;

		private ScreenOrientation _orientation;
		public ScreenOrientation orientation {

			set {
				
				if (value == ScreenOrientation.Landscape) {
					
					var w = this.width;
					var h = this.height;
					
					this.width = Mathf.Max(w, h);
					this.height = Mathf.Min(w, h);
					
				}
				
				if (value == ScreenOrientation.Portrait) {
					
					var w = this.width;
					var h = this.height;
					
					this.width = Mathf.Min(w, h);
					this.height = Mathf.Max(w, h);
					
				}

				this._orientation = value;

			}

			get {

				return this._orientation;

			}

		}

	};

	public enum ParseDeviceResolutionType {

		PPI,
		INCHES,

	};

	public class Parser {

		public static List<DeviceInfo> popular = new List<DeviceInfo>();
		public static List<DeviceInfo> p720 = new List<DeviceInfo>();
		public static List<DeviceInfo> p1080 = new List<DeviceInfo>();
		public static List<DeviceInfo> p1440 = new List<DeviceInfo>();
		public static Dictionary<string, List<DeviceInfo>> manufacturerToDevices = new Dictionary<string, List<DeviceInfo>>();

		public static void Collect(bool forced = true) {

			if (Parser.manufacturerToDevices.Count == 0 || forced == true) {

				Parser.manufacturerToDevices.Clear();

				Parser.Parse_INTERNAL("Popular", Parser.popular, ParseDeviceResolutionType.INCHES);
				
				//Parser.Parse_INTERNAL("720-800", Parser.p720, ParseDeviceResolutionType.PPI);
				//Parser.Parse_INTERNAL("1080", Parser.p1080, ParseDeviceResolutionType.PPI);
				//Parser.Parse_INTERNAL("1440", Parser.p1440, ParseDeviceResolutionType.PPI);

			}

		}

		private static void Parse_INTERNAL(string file, List<DeviceInfo> output, ParseDeviceResolutionType type) {

			output.Clear();

			var data = UnityEngine.Resources.Load("UI.Windows/DevicePreview/" + file) as TextAsset;
			if (data != null) {

				var lines = data.text.Split('\n');
				foreach (var line in lines) {

					var info = line.Trim().Split(',');

					var device = new DeviceInfo();
					device.manufacturer = info[0].Trim();
					device.model = info[1].Trim();
					
					device.orientation = ScreenOrientation.Landscape;

					if (device.model == "{SCREEN}") {

						device.current = true;

						device.model = "MY SCREEN";
						device.width = Screen.width;
						device.height = Screen.height;
						device.ppi = Screen.dpi;

					} else {
						
						device.current = false;

						var res = info[2].Trim();
						var sres = res.Split('x');
						
						device.width = int.Parse(sres[0]);
						device.height = int.Parse(sres[1]);

						if (type == ParseDeviceResolutionType.PPI) {

							device.ppi = float.Parse(info[3].Trim());

						} else if (type == ParseDeviceResolutionType.INCHES) {

							device.ppi = Parser.ConvertInchesToPPI(float.Parse(info[3]), device.width, device.height);

						}

					}

					var devOutput = info.Length > 4 ? info[4].Trim() : string.Empty;
					device.deviceOutput = devOutput;

					output.Add(device);
					
					if (Parser.manufacturerToDevices.ContainsKey(device.manufacturer) == true) {
						
						Parser.manufacturerToDevices[device.manufacturer].Add(device);
						
					} else {
						
						Parser.manufacturerToDevices.Add(device.manufacturer, new List<DeviceInfo>() { device });
						
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