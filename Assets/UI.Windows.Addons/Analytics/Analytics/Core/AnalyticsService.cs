using System.Collections;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;
using UnityEngine.UI.Windows.UserInfo;

namespace UnityEngine.UI.Windows.Plugins.Analytics {

	public interface IAnalyticsService : IService {

		System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2 = null, string group3 = null, int weight = 1);
		System.Collections.Generic.IEnumerator<byte> OnEvent(string eventName, string group1, string group2 = null, string group3 = null, int weight = 1);
		System.Collections.Generic.IEnumerator<byte> OnScreenTransition(int index, int screenId, int toScreenId, bool popup);
		System.Collections.Generic.IEnumerator<byte> OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature);
		System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature);
		System.Collections.Generic.IEnumerator<byte> OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y);
		
		System.Collections.Generic.IEnumerator<byte> SetUserId(long id);
		System.Collections.Generic.IEnumerator<byte> SetUserId(string id);
		System.Collections.Generic.IEnumerator<byte> SetUserGender(Gender gender);
		System.Collections.Generic.IEnumerator<byte> SetUserBirthYear(int birthYear);

		bool IsConnected();

		#if UNITY_EDITOR
		void GetScreen(int screenId, UserFilter filter, System.Action<ScreenResult> onResult);
		void GetScreenTransition(int index, int screenId, int toScreenId, UserFilter filter, System.Action<ScreenResult> onResult);
		void GetHeatmapData(int screenId, int screenWidth, int screenHeight, UserFilter filter, System.Action<HeatmapResult> onResult);
		#endif

	};

	public class Result {

		public bool hasError = true;

	};

	public class ScreenResult : Result {

		public int count;
		public int uniqueCount;
		public bool popup;

	};

	public class HeatmapResult : Result {

		public class Point {

			// Input: (Comes from server)
			public byte tag;
			public float x;
			public float y;
			public int weight;	// Количество точек в этом месте

			// Output:
			public Vector2 realPoint;

		};

		public Point[] points;

	};

	public abstract class AnalyticsService : ServiceBase, IAnalyticsService {

		public virtual System.Collections.Generic.IEnumerator<byte> SetUserId(long id) { yield return 0; }
		public virtual System.Collections.Generic.IEnumerator<byte> SetUserId(string id) { yield return 0; }
		public virtual System.Collections.Generic.IEnumerator<byte> SetUserGender(Gender gender) { yield return 0; }
		public virtual System.Collections.Generic.IEnumerator<byte> SetUserBirthYear(int birthYear) { yield return 0; }

		public abstract System.Collections.Generic.IEnumerator<byte> OnEvent(int screenId, string group1, string group2 = null, string group3 = null, int weight = 1);
		public abstract System.Collections.Generic.IEnumerator<byte> OnEvent(string eventName, string group1, string group2 = null, string group3 = null, int weight = 1);

		public abstract System.Collections.Generic.IEnumerator<byte> OnScreenTransition(int index, int screenId, int toScreenId, bool popup);

		public abstract System.Collections.Generic.IEnumerator<byte> OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature);
		public abstract System.Collections.Generic.IEnumerator<byte> OnTransaction(string eventName, string productId, decimal price, string currency, string receipt, string signature);
		
		public virtual System.Collections.Generic.IEnumerator<byte> OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {
			
			yield return 0;

		}

		public abstract bool IsConnected();

		#if UNITY_EDITOR
		public virtual void GetScreen(int screenId, UserFilter filter, System.Action<ScreenResult> onResult) {
			
			var result = new ScreenResult();
			result.count = Random.Range(100, 200);
			result.uniqueCount = Random.Range(0, 100);
			onResult(result);
			
		}

		public virtual void GetScreenTransition(int index, int screenId, int toScreenId, UserFilter filter, System.Action<ScreenResult> onResult) {

			var result = new ScreenResult();
			result.count = Random.Range(100, 200);
			result.uniqueCount = Random.Range(0, 100);
			onResult(result);

		}

		public virtual void GetHeatmapData(int screenId, int screenWidth, int screenHeight, UserFilter filter, System.Action<HeatmapResult> onResult) {
			
			var result = new HeatmapResult();
			result.points = new HeatmapResult.Point[Random.Range(200, 500)];
			for (int i = 0; i < result.points.Length; ++i) {

				result.points[i] = new HeatmapResult.Point();
				result.points[i].tag = 5;
				result.points[i].x = Random.Range(0f, 1f);
				result.points[i].y = Random.Range(0f, 1f);
				result.points[i].weight = Random.Range(1, 100);

			}

			onResult(result);

		}

		protected override void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			this.OnInspectorGUI(settings as HeatmapSettings, item as AnalyticsServiceItem, onReset, skin);

		}

		protected virtual void OnInspectorGUI(HeatmapSettings settings, AnalyticsServiceItem item, System.Action onReset, GUISkin skin) {



		}
		#endif

	}

}