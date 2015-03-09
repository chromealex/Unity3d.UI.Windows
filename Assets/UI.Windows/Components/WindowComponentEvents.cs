using System;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows.Components.Events {
	
	/*public delegate void ComponentAction();
	public delegate void ComponentAction<T0>();
	public delegate void ComponentAction<T0, T1>();
	public delegate void ComponentAction<T0, T1, T2>();
	public delegate void ComponentAction<T0, T1, T2, T3>();
*/
	[Serializable]
	public class ComponentEvent : UnityEvent {
		
		public void AddListenerDistinct(UnityAction action) { this.RemoveListener(action); this.AddListener(action); }

	}

	[Serializable]
	public class ComponentEvent<T0> : UnityEvent<T0> {
		
		public void AddListenerDistinct(UnityAction<T0> action) { this.RemoveListener(action); this.AddListener(action); }

	}

	[Serializable]
	public class ComponentEvent<T0, T1> : UnityEvent<T0, T1> {
		
		public void AddListenerDistinct(UnityAction<T0, T1> action) { this.RemoveListener(action); this.AddListener(action); }

	}

	[Serializable]
	public class ComponentEvent<T0, T1, T2> : UnityEvent<T0, T1, T2> {
		
		public void AddListenerDistinct(UnityAction<T0, T1, T2> action) { this.RemoveListener(action); this.AddListener(action); }

	}

	[Serializable]
	public class ComponentEvent<T0, T1, T2, T3> : UnityEvent<T0, T1, T2, T3> {
		
		public void AddListenerDistinct(UnityAction<T0, T1, T2, T3> action) { this.RemoveListener(action); this.AddListener(action); }

	}

}