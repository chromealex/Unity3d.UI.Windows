using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {
	
	public interface IPreviewEditor {
		
		bool HasPreviewGUI();
		void OnEnable();
		void OnDisable();
		void OnPreviewGUI(Rect rect, GUIStyle style);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, bool hovered);
		void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, WindowLayoutElement selectedElement);
		
	}

	public enum WindowEventType : byte {

		OnInit,
		OnDeinit,
		OnShowBegin,
		OnShowEnd,
		OnHideBegin,
		OnHideEnd,

	};
	/*
	public interface IWindowEvents {
		
		void OnInit();
		void OnDeinit();
		void OnShowBegin();
		void OnShowEnd();
		void OnHideBegin();
		void OnHideEnd();
		
	}*/
	
	public interface IWindowEventsAsync {
		
		void OnInit();
		void OnDeinit();
		void OnShowBegin(System.Action callback, bool resetAnimation = true);
		void OnShowEnd();
		void OnHideBegin(System.Action callback, bool immediately = false);
		void OnHideEnd();

	}

}