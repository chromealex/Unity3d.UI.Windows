using UnityEngine;
using System.Collections;
using ME;
using UnityEngine.UI.Extensions;

namespace UnityEngine.UI.Windows.Types {

	[ExecuteInEditMode()]
	public class LayoutTipWindowType : LayoutWindowType {

		public RectTransform layoutRoot;
		public RectTransform root;
		public WindowLayoutBase animationRoot;
		
		[HideInInspector][SerializeField]
		public Canvas canvas;
		[HideInInspector][SerializeField]
		public CanvasScaler canvasScaler;
		[HideInInspector][SerializeField]
		public UnityEngine.EventSystems.BaseRaycaster raycaster;
		[HideInInspector][SerializeField]
		public bool initializedChild = false;
		
		private RectTransform uiElement;
		private Canvas uiElementCanvas;
		
		private float scaleFactorIn = 1f;
		private float scaleFactorOut = 1f;

		private HUDItem tempHud;
		
		public override Canvas GetCanvas() {
			
			return this.canvas;
			
		}

		public override CanvasScaler GetCanvasScaler() {

			return this.canvasScaler;

		}

		public override void OnDeinit(System.Action callback) {

			this.uiElement = null;
			this.uiElementCanvas = null;
			if (this.tempHud != null) {

				this.tempHud.Reset();
				Component.Destroy(this.tempHud);

			}
			this.tempHud = null;

			base.OnDeinit(callback);

		}

		public void OnHover(Transform worldElement, Vector3 worldPoint, Camera gameCamera, Vector3 offset = default(Vector3)) {
			
			if (this.animationRoot != null) this.animationRoot.SetInState();

			if (this.tempHud == null) this.tempHud = this.root.GetComponent<HUDItem>();
			if (this.tempHud == null) this.tempHud = this.root.gameObject.AddComponent<HUDItem>();

			this.tempHud.InitHUD(worldElement, this.workCamera, gameCamera, offset);
			this.tempHud.enabled = true;

			if (this.animationRoot != null) this.animationRoot.SetResetState();
			
		}

		public void OnHover(RectTransform uiElement) {
			
			this.uiElement = uiElement;

			if (this.tempHud != null) this.tempHud.enabled = false;

			if (this.animationRoot != null) this.animationRoot.SetInState();

			var canvas = uiElement.root.GetComponentsInChildren<Canvas>()[0];
			uiElement.root.GetComponentInChildren<Canvas>();
			this.uiElementCanvas = canvas;

			this.ApplyPosition();

			if (this.animationRoot != null) this.animationRoot.SetResetState();

			if (this.layoutRoot != null) {

				this.CheckPivot(uiElement.rect.size, canvas.transform as RectTransform);

			} else {
				
				var anchor = this.root.anchoredPosition3D;
				anchor.z = 0f;
				this.root.anchoredPosition3D = anchor + Vector3.up * uiElement.rect.size.y * 0.5f;

			}

		}
		
		public void OnLeave() {
			
			
			
		}

		public float GetScaleFactor() {
			
			this.scaleFactorIn = this.canvas.transform.localScale.x;
			this.scaleFactorOut = this.uiElementCanvas.transform.localScale.x;
			
			var scaleIn = this.scaleFactorIn;
			var scaleOut = this.scaleFactorOut;
			
			var factor = scaleIn / scaleOut;

			return 1f / factor;

		}

		public void ApplyPosition() {

			var pos = this.uiElement.position;
			this.root.position = pos;
			
			pos = this.root.anchoredPosition3D;
			pos.z = 0f;
			this.root.anchoredPosition3D = pos;

		}

		public void CheckPivot(Vector2 size, RectTransform sourceRect) {
			
			this.StartCoroutine(this.CheckPivot_YIELD(size, sourceRect));

		}

		public System.Collections.Generic.IEnumerator<byte> CheckPivot_YIELD(Vector2 size, RectTransform sourceTransform) {

			// Get pivot point according on screen rect

			// Check points
			var checks = new Vector2[] {
				new Vector2(0.5f, 1f), 	// top center
				new Vector2(0.5f, 0f), 	// bottom center
				new Vector2(0f, 0f), 	// bottom left
				new Vector2(1f, 0f), 	// bottom right
				new Vector2(0f, 1f), 	// top left
				new Vector2(1f, 1f) 	// top right
			};

			var offsetDirs = new Vector2[] {
				Vector2.down,
				Vector2.up,
				Vector2.up,
				Vector2.up,
				Vector2.down,
				Vector2.down
			};

			CanvasUpdater.ForceUpdate(this.canvas, this.canvasScaler);
			yield return 0;
			
			var scaleFactor = this.GetScaleFactor();
			size *= scaleFactor;

			this.ApplyPosition();

			var centerPoint = this.root.anchoredPosition;
			var sourceRect = (this.canvas.transform as RectTransform).rect;

			for (int i = 0; i < checks.Length; ++i) {

				var centerOffset = offsetDirs[i] * size.y * 0.5f;
				var pivot = checks[i];
				this.layoutRoot.pivot = pivot;

				this.root.anchoredPosition = centerPoint + centerOffset;

				Vector3[] points = new Vector3[4];
				this.layoutRoot.GetLocalCorners(points);

				var offset = this.root.anchoredPosition;
				var bottomLeft = offset + points[0].XY();
				var topLeft = offset + points[1].XY();
				var topRight = offset + points[2].XY();
				var bottomRight = offset + points[3].XY();

				if (sourceRect.Contains(bottomLeft) == false) {
					
					//Debug.Log("Failed bottom left: " + sourceRect + " << " + bottomLeft);
					continue;
					
				}
				
				if (sourceRect.Contains(topLeft) == false) {
					
					//Debug.Log("Failed top left: " + sourceRect + " << " + topLeft);
					continue;
					
				}
				
				if (sourceRect.Contains(topRight) == false) {
					
					//Debug.Log("Failed top right: " + sourceRect + " << " + topRight);
					continue;
					
				}
				
				if (sourceRect.Contains(bottomRight) == false) {
					
					//Debug.Log("Failed bottom right: " + sourceRect + " << " + bottomRight);
					continue;
					
				}

				break;

			}

		}

		public void PrepareFor(WindowComponent component) {

			var canvasScaler = this.canvasScaler;
			if (canvasScaler != null) {

				var sourceWindow = component.GetWindow<LayoutWindowType>();
				if (sourceWindow != null) {

					var depth = this.workCamera.depth;
					var cullingMask = this.workCamera.cullingMask;
					var clearFlags = this.workCamera.clearFlags;
					var farClipPlane = this.workCamera.farClipPlane;
					var nearClipPlane = this.workCamera.nearClipPlane;

					var pos = this.transform.position;
					this.workCamera.CopyFrom(sourceWindow.workCamera);
					this.transform.position = pos;

					this.workCamera.depth = depth;
					this.workCamera.cullingMask = cullingMask;
					this.workCamera.clearFlags = clearFlags;
					this.workCamera.farClipPlane = farClipPlane;
					this.workCamera.nearClipPlane = nearClipPlane;

					//var sourceCanvasScaler = sourceWindow.layout.layout.canvasScaler;
					/*
					canvasScaler.defaultSpriteDPI = sourceCanvasScaler.defaultSpriteDPI;
					canvasScaler.dynamicPixelsPerUnit = sourceCanvasScaler.dynamicPixelsPerUnit;
					canvasScaler.fallbackScreenDPI = sourceCanvasScaler.fallbackScreenDPI;
					canvasScaler.matchWidthOrHeight = sourceCanvasScaler.matchWidthOrHeight;
					canvasScaler.physicalUnit = sourceCanvasScaler.physicalUnit;
					canvasScaler.referencePixelsPerUnit = sourceCanvasScaler.referencePixelsPerUnit;
					canvasScaler.referenceResolution = sourceCanvasScaler.referenceResolution;
					canvasScaler.scaleFactor = sourceCanvasScaler.scaleFactor;
					canvasScaler.screenMatchMode = sourceCanvasScaler.screenMatchMode;
					canvasScaler.uiScaleMode = sourceCanvasScaler.uiScaleMode;*/

					//this.scaleFactor = 1f / this.uiElementCanvas.transform.localScale.x;//1f / (this.canvas.scaleFactor / sourceWindow.layout.layout.canvas.scaleFactor);

				}
				
			}

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {
			
			base.OnValidateEditor();

			this.Update_EDITOR();
			
		}

		private void Update_EDITOR() {

			#region COMPONENTS
			var canvases = this.GetComponentsInChildren<Canvas>(true);
			if (canvases != null && canvases.Length > 0) this.canvas = canvases[0];
			var scalers = this.GetComponentsInChildren<CanvasScaler>(true);
			if (scalers != null && scalers.Length > 0) this.canvasScaler = scalers[0];
			var raycasters = this.GetComponentsInChildren<UnityEngine.EventSystems.BaseRaycaster>(true);
			if (raycasters != null && raycasters.Length > 0) this.raycaster = raycasters[0];
			#endregion
			
			this.initializedChild = (this.canvas != null);
			
			#region SETUP
			if (this.initializedChild == true) {
				
				// Canvas
				WindowSystem.ApplyToSettings(this.canvas);

				// Raycaster
				if ((this.raycaster as GraphicRaycaster) != null) {
					
					(this.raycaster as GraphicRaycaster).GetType().GetField("m_BlockingMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue((this.raycaster as GraphicRaycaster), (LayerMask)(1 << this.gameObject.layer));
					
				}
				
			}
			#endregion

		}
		#endif

	}
	
}