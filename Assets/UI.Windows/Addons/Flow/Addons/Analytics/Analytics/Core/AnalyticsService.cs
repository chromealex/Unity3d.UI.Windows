using System.Collections;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.Analytics {

	public interface IAnalyticsService : IService {

		IEnumerator OnEvent(int screenId, string group1, string group2 = null, string group3 = null, int weight = 1);
		IEnumerator OnScreenTransition(int index, int screenId, int toScreenId, bool popup);
		IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature);
		IEnumerator OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y);
		
		IEnumerator SetUserId(long id);
		IEnumerator SetUserId(string id);
		IEnumerator SetUserGender(User.Gender gender);
		IEnumerator SetUserBirthYear(int birthYear);

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

		public virtual IEnumerator SetUserId(long id) { yield return false; }
		public virtual IEnumerator SetUserId(string id) { yield return false; }
		public virtual IEnumerator SetUserGender(User.Gender gender) { yield return false; }
		public virtual IEnumerator SetUserBirthYear(int birthYear) { yield return false; }

		public abstract IEnumerator OnEvent(int screenId, string group1, string group2 = null, string group3 = null, int weight = 1);

		public abstract IEnumerator OnScreenTransition(int index, int screenId, int toScreenId, bool popup);

		public abstract IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature);
		
		public virtual IEnumerator OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {
			
			yield return false;

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