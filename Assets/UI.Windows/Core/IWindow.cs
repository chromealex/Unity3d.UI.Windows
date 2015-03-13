using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

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
		void OnShowBegin(System.Action callback);
		void OnShowEnd();
		void OnHideBegin(System.Action callback);
		void OnHideEnd();
		
	}

}