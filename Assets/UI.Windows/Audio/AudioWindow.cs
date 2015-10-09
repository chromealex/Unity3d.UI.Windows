using UnityEngine;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Plugins.Flow;

namespace UnityEngine.UI.Windows.Audio {
	
	[System.Serializable]
	public class Component {
		
		#if UNITY_EDITOR
		//[HideInInspector]
		public FlowData flowData;
		#endif

		[SerializeField][AudioPopup(ClipType.SFX)]
		private int id = 0;

		public void Play() {

			if (this.id > 0) WindowSystem.AudioPlay(ClipType.SFX, this.id);

		}
		
	}

	[System.Serializable]
	public class Window : IWindowEventsAsync {

		public enum PlayType : byte {

			KeepCurrent,
			RestartIfEquals,

		};
		
		#if UNITY_EDITOR
		//[HideInInspector]
		public FlowData flowData;
		#endif

		[ReadOnly("flowData", null)]
		public PlayType playType = PlayType.KeepCurrent;
		[ReadOnly("flowData", null)]
		public ClipType clipType;
		[SerializeField][AudioPopup("clipType")]
		private int id = 0;

		// Events
		public void OnInit() { }
		public void OnDeinit() { }
		public void OnShowBegin(System.Action callback, bool resetAnimation = true) {

			var equals = false;

			var prevWindow = WindowSystem.GetPreviousWindow();
			if (prevWindow != null) {

				equals = (prevWindow.audio.id == this.id || prevWindow.audio.id == 0);

				if (this.playType == PlayType.RestartIfEquals && equals == true) {

					WindowSystem.AudioStop(this.clipType, prevWindow.audio.id);

				}

			}

			//Debug.Log(prevWindow + " :: " + this.playType + " :: " + equals + " :: " + WindowSystem.GetCurrentWindow());
			if (this.playType == PlayType.KeepCurrent && equals == true) {

				// Keep current

			} else {

				if (this.id > 0) WindowSystem.AudioPlay(this.clipType, this.id);

			}

			if (callback != null) callback();
			
		}
		public void OnShowEnd() { }
		public void OnHideBegin(System.Action callback, bool immediately = false) {

			if (callback != null) callback();

		}
		public void OnHideEnd() { }

	}

}