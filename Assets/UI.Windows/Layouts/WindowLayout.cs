using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace UnityEngine.UI.Windows {

	[RequireComponent(typeof(Canvas))]
	public class WindowLayout : WindowObject, ICanvasElement, IWindowEvents {

		public enum ScaleMode {

			Normal,
			Fixed,
			Custom,

		};

		public CanvasScaler canvasScaler;

		[ReadOnly]
		public WindowLayoutRoot root;
		[ReadOnly]
		public List<WindowLayoutElement> elements = new List<WindowLayoutElement>();

		public bool showGrid = false;
		public Vector2 gridSize = Vector2.one * 5f;

		[HideInInspector][SerializeField]
		public Canvas canvas;
		[HideInInspector][SerializeField]
		public UnityEngine.EventSystems.BaseRaycaster raycaster;
		[HideInInspector][SerializeField]
		public bool initialized = false;
		
		[HideInInspector][SerializeField]
		private bool isAlive = false;
		
		#if UNITY_EDITOR
		[ContextMenu("Setup")]
		public void Setup() {

			if (this.GetComponent<CanvasScaler>() != null) Component.DestroyImmediate(this.GetComponent<CanvasScaler>());
			this.canvasScaler = this.gameObject.AddComponent<CanvasScaler>();

		}

		public virtual void OnValidate() {

			this.elements = this.GetComponentsInChildren<WindowLayoutElement>(true).ToList();

		}
		#endif

		public void Init(float depth, int raycastPriority, int orderInLayer, WindowLayout.ScaleMode scaleMode) {
			
			if (this.initialized == false) {
				
				Debug.LogError("Can't initialize window instance because of some components was not installed properly.");
				return;
				
			}

			this.canvas.sortingOrder = orderInLayer;
			this.canvas.planeDistance = 10f;// * orderInLayer;
			this.canvas.worldCamera = this.GetWindow().workCamera;

			this.SetScale(scaleMode);

		}

		public bool ValidateCanvasScaler() {

			var changed = false;

			if (this.canvasScaler == null) {

				this.canvasScaler = this.GetComponent<CanvasScaler>();
				changed = true;

			}

			if (this.canvasScaler == null) {

				this.canvasScaler = this.gameObject.AddComponent<CanvasScaler>();
				changed = true;

			}

			return changed;

		}

		public void SetNoScale() {

			this.ValidateCanvasScaler();
			
			this.canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			this.canvasScaler.enabled = false;

		}
		
		public void SetScale(WindowLayout.ScaleMode scaleMode) {

			this.ValidateCanvasScaler();

			if (scaleMode == ScaleMode.Normal) {

				this.SetNoScale();

			} else {
				
				this.canvasScaler.enabled = true;

				if (scaleMode == ScaleMode.Fixed) {

					this.canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
					this.canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
					this.canvasScaler.matchWidthOrHeight = 0f;
					this.canvasScaler.referenceResolution = new Vector2(1024f, 768f);	// TODO: Add to flow data settings

				} else if (scaleMode == ScaleMode.Custom) {

					// We should not do anything in canvasScaler

				}

			}

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

