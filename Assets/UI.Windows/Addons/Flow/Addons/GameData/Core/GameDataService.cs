using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.GameData {

	public interface IGameDataService : IService {

		IEnumerator GetData(string key, System.Action<GameDataResult> onResult);

	};
	
	public class Result {
		
		public bool hasError = true;
		
	};
	
	public class GameDataResult : Result {
		
		public string data;
		
	};

	public abstract class GameDataService : ServiceBase, IGameDataService {

		public virtual IEnumerator GetData(string key, System.Action<GameDataResult> onResult) {

			yield return false;

		}

		#if UNITY_EDITOR
		protected override void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			this.OnInspectorGUI(settings as GameDataSettings, item as GameDataServiceItem, onReset, skin);
			
		}
		
		protected virtual void OnInspectorGUI(GameDataSettings settings, GameDataServiceItem item, System.Action onReset, GUISkin skin) {
			
			
			
		}
		#endif

	}

}