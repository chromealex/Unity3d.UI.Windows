using System.Collections;
using UnityEngine.UI.Windows.Plugins.Heatmap.Core;

namespace UnityEngine.UI.Windows.Plugins.Analytics {

	public interface IAnalyticsService {

		bool isActive { set; get; }
		IEnumerator Auth(string key);
		string GetPlatformName();
		IEnumerator OnEvent(int screenId, string group1, string group2 = null, string group3 = null, int weight = 1);
		IEnumerator OnScreenTransition(int index, int screenId, int toScreenId);
		IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature);
		IEnumerator OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y);
		
		IEnumerator SetUserId(long id);
		IEnumerator SetUserId(string id);
		IEnumerator SetUserGender(User.Gender gender);
		IEnumerator SetUserBirthYear(int birthYear);

		#if UNITY_EDITOR
		void GetScreen(string key, int screenId, System.Action<Result> onResult);
		void GetScreenTransition(string key, int index, int screenId, int toScreenId, System.Action<Result> onResult);
		void OnInspectorGUI(HeatmapSettings settings, AnalyticsItem item, System.Action onReset, GUISkin skin);
		#endif

	};

	public class Result {

		public bool hasError = true;

	};

	public class ScreenResult : Result {

		public int count;
		public int uniqueCount;

	};

	public abstract class AnalyticsService : MonoBehaviour, IAnalyticsService {

		public Analytics analytics;
		public bool isActive { set; get; }

		public virtual void Awake() {

			this.analytics.Register(this);

		}

		public virtual IEnumerator Auth(string key) {

			yield return false;

		}
		
		public virtual IEnumerator SetUserId(long id) { yield return false; }
		public virtual IEnumerator SetUserId(string id) { yield return false; }
		public virtual IEnumerator SetUserGender(User.Gender gender) { yield return false; }
		public virtual IEnumerator SetUserBirthYear(int birthYear) { yield return false; }

		public abstract string GetPlatformName();

		public abstract IEnumerator OnEvent(int screenId, string group1, string group2 = null, string group3 = null, int weight = 1);

		public abstract IEnumerator OnScreenTransition(int index, int screenId, int toScreenId);
		
		public abstract IEnumerator OnTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature);
		
		public virtual IEnumerator OnScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) {
			
			yield return false;

		}

		#if UNITY_EDITOR
		public virtual void GetScreen(string key, int screenId, System.Action<Result> onResult) {
			
			var result = new ScreenResult();
			result.count = Random.Range(100, 200);
			result.uniqueCount = Random.Range(0, 100);
			onResult(result);
			
		}

		public virtual void GetScreenTransition(string key, int index, int screenId, int toScreenId, System.Action<Result> onResult) {

			var result = new ScreenResult();
			result.count = Random.Range(100, 200);
			result.uniqueCount = Random.Range(0, 100);
			onResult(result);

		}

		public virtual void OnInspectorGUI(HeatmapSettings settings, AnalyticsItem item, System.Action onReset, GUISkin skin) {



		}

		public virtual void OnValidate() {

			if (Application.isPlaying == true) return;

			if (this.analytics == null) this.analytics = this.GetComponent<Analytics>();

		}
		#endif

	}

}