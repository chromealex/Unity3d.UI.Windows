using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace UnityEngine.UI.Windows {

	[RequireComponent(typeof(Canvas))]
	public class WindowLayout : WindowObject, ICanvasElement, IWindowEvents {
		
		public float factor;

		[ReadOnly]
		public WindowLayoutRoot root;
		[ReadOnly]
		public List<WindowLayoutElement> elements = new List<WindowLayoutElement>();
		
		[HideInInspector][SerializeField]
		public Canvas canvas;
		[HideInInspector][SerializeField]
		public UnityEngine.EventSystems.BaseRaycaster raycaster;
		[HideInInspector][SerializeField]
		public bool initialized = false;
		
		[HideInInspector][SerializeField]
		private bool isAlive = false;

		public void Init(float depth, int raycastPriority, int orderInLayer) {
			
			if (this.initialized == false) {
				
				Debug.LogError("Can't initialize window instance because of some components was not installed properly.");
				return;
				
			}

			this.canvas.sortingOrder = orderInLayer;
			this.canvas.planeDistance = 10f;// * orderInLayer;
			this.canvas.worldCamera = this.GetWindow().workCamera;

		}

		public void Rebuild(CanvasUpdate executing) {

			if (this.root != null) this.root.Rebuild();

		}

		public bool IsDestroyed() {

			return !this.isAlive;

		}

		public WindowLayoutElement GetRootByTag(LayoutTag tag) {

			return this.elements.FirstOrDefault((element) => element.tag == tag);

		}
		
		public void GetTags(List<LayoutTag> tags, List<string> descriptions) {
			
			tags.Clear();
			descriptions.Clear();
			
			foreach (var element in this.elements) {

				if (element == null) continue;

				tags.Add(element.tag);
#if UNITY_EDITOR
				descriptions.Add(element.comment);
#endif
				
			}
			
		}

		public void OnShowBegin() {

			this.isAlive = true;
			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);

			this.OnShowBeginEvent();

		}

		public void OnHideEnd() {

			this.isAlive = false;
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

			this.OnHideEndEvent();

		}
		
		public virtual void OnInit() {}
		public virtual void OnDeinit() {}
		public virtual void OnShowEnd() {}
		public virtual void OnHideBegin() {}

		public virtual void OnShowBeginEvent() {}
		public virtual void OnHideEndEvent() {}

	}

}

