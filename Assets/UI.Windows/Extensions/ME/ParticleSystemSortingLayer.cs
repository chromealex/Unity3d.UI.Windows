using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;

[ExecuteInEditMode()]
public class ParticleSystemSortingLayer : MonoBehaviour {

	public WindowObject windowObject;
	new public ParticleSystem particleSystem;
	public int orderDelta;

	public void Start() {

		if (Application.isPlaying == false) return;

		this.Init();

	}

	public void Init() {

		if (this.windowObject != null) {

			var window = this.windowObject.GetWindow();
			if (window == null) return;

			window.events.eventsInstance.Unregister(WindowEventType.OnShowBegin, this.UpdateLayer);
			window.events.eventsInstance.Register(WindowEventType.OnShowBegin, this.UpdateLayer);

			this.UpdateLayer();

		}

	}

#if UNITY_EDITOR
	[ContextMenu("Find Root Window Object")]
	public void FindRootWindowObject() {

		var objs = this.GetComponentsInParent<WindowObject>(true);
		if (objs == null || objs.Length == 0) return;

		this.windowObject = objs[0];

	}

	public void OnValidate() {

		this.FindRootWindowObject();
		this.UpdateLayer();
		
	}
#endif

	public void UpdateLayer() {

		if (this.particleSystem != null && this.windowObject != null) {

			var window = this.windowObject.GetWindow();
			if (window == null) return;

			var renderer = this.particleSystem.GetComponent<Renderer>();

			renderer.sortingLayerName = window.GetSortingLayerName();
			renderer.sortingOrder = window.GetSortingOrder() + this.orderDelta;

			//Debug.Log("Layer `" + window.GetSortingLayerName() + "` updated: " + this.particleSystem.renderer.sortingOrder);

		}

	}

}
