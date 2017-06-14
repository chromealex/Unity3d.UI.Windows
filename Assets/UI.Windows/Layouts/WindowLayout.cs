using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI.Extensions;

namespace UnityEngine.UI.Windows {

	[RequireComponent(typeof(Canvas))]
	[RequireComponent(typeof(CanvasUpdater))]
    public class WindowLayout : WindowObjectElement, ICanvasElement, IWindowEventsAsync, IWindowEventsController {

		public enum ScaleMode : byte {

			Normal,
			Fixed,
			Custom,
			Preferences,

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
		public CanvasUpdater canvasUpdater;
		[HideInInspector][SerializeField]
		public bool initialized = false;
		
		[HideInInspector][SerializeField]
		private bool isAlive = false;

		public override bool NeedToInactive() {

			return false;

		}

		#if UNITY_EDITOR
		[HideInInspector]
		public float editorScale;
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			WindowSystem.ApplyToSettings(this.canvas);

			if (ME.EditorUtilities.IsPrefab(this.gameObject) == true) {

				this.transform.localScale = Vector3.one * this.editorScale;

			}

			if (this.canvasUpdater == null || this.GetComponents<CanvasUpdater>().Length > 1) {

				if (this.GetComponent<CanvasUpdater>() != null) Component.DestroyImmediate(this.GetComponent<CanvasUpdater>());
				this.canvasUpdater = this.GetComponent<CanvasUpdater>();
				if (this.canvasUpdater == null) this.canvasUpdater = this.gameObject.AddComponent<CanvasUpdater>();
				if (this.canvasUpdater != null) this.canvasUpdater.OnValidate();

			}

			if (this.canvasScaler == null || this.GetComponents<CanvasScaler>().Length > 1) {

				if (this.GetComponent<CanvasScaler>() != null) Component.DestroyImmediate(this.GetComponent<CanvasScaler>());
				this.canvasScaler = this.GetComponent<CanvasScaler>();
				if (this.canvasScaler == null) this.canvasScaler = this.gameObject.AddComponent<CanvasScaler>();

			}

			/*var rectTransform = (this.transform as RectTransform);
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.pivot = Vector2.one * 0.5f;
			rectTransform.localScale = Vector3.one;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.anchoredPosition3D = Vector3.zero;*/

		}
		#endif

		public void Init(float depth, int raycastPriority, int orderInLayer) {
			
			if (this.initialized == false) {
				
				Debug.LogError("Can't initialize window instance because of some components was not installed properly.");
				return;
				
			}
			
			this.transform.localScale = Vector3.zero;

			this.canvas.worldCamera = this.GetWindow().workCamera;
			this.canvas.sortingOrder = orderInLayer;
			this.canvas.planeDistance = 10f;// * orderInLayer;
			
			CanvasUpdater.ForceUpdate(this.canvas, this.canvasScaler);

			for (int i = 0; i < this.elements.Count; ++i) {

				var element = this.elements[i];
				if (element.autoRegisterInRoot == true) {
					
					element.Setup(this.GetWindow());
					element.SetComponentState(WindowObjectState.NotInitialized, dontInactivate: true);

				}

			}

			this.root.Setup(this.GetWindow());

		}

		public void SetLayoutPreferences(WindowLayout.ScaleMode scaleMode, Vector2 fixedScaleResolution, WindowLayoutPreferences layoutPreferences) {

			var matchWidthOrHeight = 0f;

			if (scaleMode == ScaleMode.Preferences) {

				var fixedScale = layoutPreferences.fixedScale;
				if (fixedScale == true) {

					scaleMode = ScaleMode.Fixed;

				} else {

					scaleMode = ScaleMode.Normal;

				}

				fixedScaleResolution = layoutPreferences.fixedScaleResolution;
				matchWidthOrHeight = layoutPreferences.matchWidthOrHeight;

			}

			this.SetScale(scaleMode, fixedScaleResolution, matchWidthOrHeight);

		}

		public Vector2 GetSize() {

			this.ValidateCanvasScaler();

			var scaleFactor = 1f;
			var width = Screen.width;
			var height = Screen.height;

			if (this.canvasScaler != null) {
				
				//scaleFactor = this.canvasScaler.scaleFactor;

				if (this.canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize) {

					scaleFactor = this.GetConstantPixelSize();

				} else if (this.canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize) {
					
					scaleFactor = this.GetConstantPhysicalSize();

				} else if (this.canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize) {
					
					scaleFactor = this.GetScaleWithScreenSize();

				}

			}

			return new Vector2(width * scaleFactor, height * scaleFactor);

		}

		#region GET SIZE
		protected float GetConstantPhysicalSize() {

			float dpi = Screen.dpi;
			float num = (dpi != 0f) ? dpi : this.canvasScaler.fallbackScreenDPI;
			float num2 = 1f;
			switch (this.canvasScaler.physicalUnit) {
				case CanvasScaler.Unit.Centimeters:
					num2 = 2.54f;
					break;
				case CanvasScaler.Unit.Millimeters:
					num2 = 25.4f;
					break;
				case CanvasScaler.Unit.Inches:
					num2 = 1f;
					break;
				case CanvasScaler.Unit.Points:
					num2 = 72f;
					break;
				case CanvasScaler.Unit.Picas:
					num2 = 6f;
					break;
			}
			var scaleFactor = num / num2;

			return scaleFactor;

		}
		
		protected float GetConstantPixelSize() {
			
			return this.canvasScaler.scaleFactor;

		}
		
		protected float GetScaleWithScreenSize() {

			Vector2 vector = new Vector2(Screen.width, Screen.height);
			float scaleFactor = 0f;
			switch (this.canvasScaler.screenMatchMode) {
				case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
					{
						float num = Mathf.Log(vector.x / this.canvasScaler.referenceResolution.x, 2f);
						float num2 = Mathf.Log(vector.y / this.canvasScaler.referenceResolution.y, 2f);
						float num3 = Mathf.Lerp(num, num2, this.canvasScaler.matchWidthOrHeight);
						scaleFactor = Mathf.Pow(2f, num3);
						break;
					}
				case CanvasScaler.ScreenMatchMode.Expand:
					scaleFactor = Mathf.Min(vector.x / this.canvasScaler.referenceResolution.x, vector.y / this.canvasScaler.referenceResolution.y);
					break;
				case CanvasScaler.ScreenMatchMode.Shrink:
					scaleFactor = Mathf.Max(vector.x / this.canvasScaler.referenceResolution.x, vector.y / this.canvasScaler.referenceResolution.y);
					break;
			}

			return scaleFactor;

		}
		#endregion

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
		
		public void SetScale(WindowLayout.ScaleMode scaleMode, Vector2 fixedResolution, float matchWidthOrHeight) {

			this.ValidateCanvasScaler();

			if (scaleMode == ScaleMode.Normal) {

				this.SetNoScale();

			} else {
				
				this.canvasScaler.enabled = true;

				if (scaleMode == ScaleMode.Fixed) {

					this.canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
					this.canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
					this.canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
					this.canvasScaler.referenceResolution = new Vector2(fixedResolution.x, fixedResolution.y);

				} else if (scaleMode == ScaleMode.Custom) {

					// We should not do anything in canvasScaler

				}

			}

		}

		public void LayoutComplete() {
		}

		public void GraphicUpdateComplete() {
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
		
		public void GetTags(List<LayoutTag> tags) {
			
			tags.Clear();
			
			foreach (var element in this.elements) {

				if (element == null) continue;

				tags.Add(element.tag);
				
			}
			
		}

		public override void OnShowBegin() {

			this.isAlive = true;
			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
			
			base.OnShowBegin();

		}

		public override void OnHideEnd() {

			this.isAlive = false;
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

			base.OnHideEnd();

		}

		public override void OnDeinit(System.Action callback) {

			ME.Utilities.CallInSequence<System.Action<System.Action>>(callback, /*waitPrevious:*/ true, (item, c) => {

				item.Invoke(c);

			},
				this.root.OnDeinit,
				base.OnDeinit
			);

		}

	}

}
