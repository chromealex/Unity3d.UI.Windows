using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEngine.UI.Windows.Plugins.Services;

namespace UnityEngine.UI.Windows.Plugins.GameData {

	public interface IGameDataService : IService {

		System.Collections.Generic.IEnumerator<byte> GetData(GameDataSettings settings, System.Action<GameDataResult> onResult);

	};
	
	public class Result {
		
		public bool hasError = true;
		public string errorText;
		
	};
	
	public class GameDataResult : Result {
		
		public string data;
		
	};

	public abstract class GameDataService : ServiceBase, IGameDataService {

		public virtual System.Collections.Generic.IEnumerator<byte> GetData(GameDataSettings settings, System.Action<GameDataResult> onResult) {

			yield return 0;

		}

		#if UNITY_EDITOR
		public virtual void EditorLoad(GameDataSettings settings, GameDataServiceItem item) {
		}

		protected override void DoInspectorGUI(ScriptableObject settings, ServiceItem item, System.Action onReset, GUISkin skin) {

			this.OnInspectorGUI(settings as GameDataSettings, item as GameDataServiceItem, onReset, skin);
			
		}
		
		protected virtual void OnInspectorGUI(GameDataSettings settings, GameDataServiceItem item, System.Action onReset, GUISkin skin) {
			
			
			
		}
		#endif

	}

}