using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public interface IWindowAnimation : IWindowEventsAsync {
		
		float GetAnimationDuration(bool forward);
		void Setup(WindowComponent component);
		
	}

	public interface IWindowComponent : IWindowEvents {

	}

}