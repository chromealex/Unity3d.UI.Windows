using UnityEngine;
using UnityEngine.UI.Windows;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEditor.UI.Windows {

	[CustomEditor(typeof(WindowLayout), true)]
	[CanEditMultipleObjects()]
	public class WindowLayoutEditor : Editor {
		
		private bool isDirty = false;

		public override void OnInspectorGUI() {

			this.DrawDefaultInspector();

			if (Application.isPlaying == true) return;

			var _target = this.target as UnityEngine.UI.Windows.WindowLayout;
			if (_target == null) return;
			if (_target.hideFlags == HideFlags.HideAndDontSave) return;

			this.ApplyRoot(_target);
			this.UpdateLinks(_target);

			if (this.isDirty == true) {

				UnityEditor.EditorUtility.SetDirty(_target);
				this.isDirty = false;

			}

		}

		private float GetFactor(Vector2 inner, Vector2 boundingBox) {     

			float widthScale = 0, heightScale = 0;
			if (inner.x != 0)
				widthScale = boundingBox.x / inner.x;
			if (inner.y != 0)
				heightScale = boundingBox.y / inner.y;                
			
			return Mathf.Min(widthScale, heightScale);

		}

		public override void OnPreviewGUI(Rect r, GUIStyle background) {
			
			var oldColor = GUI.color;

			GUI.Box(r, string.Empty);

			var _target = this.target as UnityEngine.UI.Windows.WindowLayout;
			if (_target == null) return;
			
			var elements = _target.elements;

			if (ME.EditorUtilities.IsPrefab(_target.gameObject) == false) {

				var pos = (_target.transform as RectTransform).localPosition;
				(_target.transform as RectTransform).localPosition = Vector3.zero;

				foreach (var element in elements) {
					
					var rectTransform = (element.transform as RectTransform);

					var corners = new Vector3[4];
					rectTransform.GetWorldCorners(corners);

					var rect = new Rect(corners[0].x, -corners[1].y, corners[2].x - corners[1].x, corners[2].y - corners[3].y);

					element.editorRect = rect;

				}

				(_target.transform as RectTransform).localPosition = pos;

			}
			
			var scaleFactor = 0f;
			if (elements.Count > 0) scaleFactor = this.GetFactor(new Vector2(elements[0].editorRect.width, elements[0].editorRect.height), new Vector2(r.width, r.height));

			var style = new GUIStyle("flow node 0");

			foreach (var element in elements) {

				var rect = element.editorRect;
				
				rect.x *= scaleFactor;
				rect.y *= scaleFactor;
				rect.width *= scaleFactor;
				rect.height *= scaleFactor;

				rect.x += r.x + r.width * 0.5f;
				rect.y += r.y + r.height * 0.5f;

				var color = new Color(0.8f, 0.8f, 1f, 1f);
				color.a = 0.7f;
				GUI.color = color;
				GUI.Box(rect, string.Empty, style);
				
			}

			GUI.color = oldColor;

		}

		public override bool HasPreviewGUI() {

			return true;

		}

		private void ApplyRoot(UnityEngine.UI.Windows.WindowLayout _target) {
			
			if (_target.root == null) {
				
				// Trying to find root
				var root = _target.transform.FindChild("Root");
				if (root != null) {

					_target.root = root.GetComponent<WindowLayoutRoot>() as WindowLayoutRoot;
					if (_target.root == null) {
						
						_target.root = root.gameObject.AddComponent<WindowLayoutRoot>();
						
						this.isDirty = true;

					}

				} else {
					
					// Adding root
					var go = new GameObject("Root");
					if (go.GetComponent<RectTransform>() == null) go.AddComponent<RectTransform>();

					_target.root = go.AddComponent<WindowLayoutRoot>();
					
					_target.root.transform.SetParent(_target.transform);
					_target.root.transform.localPosition = Vector3.zero;
					_target.root.transform.localScale = Vector3.one;
					_target.root.transform.localRotation = Quaternion.identity;
					
					_target.root.rectTransform.pivot = Vector3.one * 0.5f;
					_target.root.rectTransform.anchorMin = new Vector2(0.5f, 0f);
					_target.root.rectTransform.anchorMax = new Vector2(0.5f, 1f);
					_target.root.rectTransform.sizeDelta = new Vector2(1080f, 0f);
					
					this.isDirty = true;

				}

			}

		}

		public bool drawEditorRects = false;
		private void UpdateLinks(UnityEngine.UI.Windows.WindowLayout _target) {

			this.Setup_EDITOR(_target);
			this.Update_EDITOR(_target);
			
		}
		
		private void Update_EDITOR(UnityEngine.UI.Windows.WindowLayout _target) {

			foreach (var element in _target.elements) element.Update_EDITOR();

			#region COMPONENTS
			_target.canvas = _target.GetComponentsInChildren<Canvas>(true)[0];
			var raycasters = _target.GetComponentsInChildren<UnityEngine.EventSystems.BaseRaycaster>(true);
			if (raycasters != null && raycasters.Length > 0) _target.raycaster = raycasters[0];
			#endregion
			
			_target.initialized = (_target.canvas != null);
			
			#region SETUP
			if (_target.initialized == true) {

				WindowSystem.ApplyToSettings(_target.canvas);

				// Raycaster
				if ((_target.raycaster as GraphicRaycaster) != null) {
					
					(_target.raycaster as GraphicRaycaster).GetType().GetField("m_BlockingMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue((_target.raycaster as GraphicRaycaster), (LayerMask)(1 << _target.gameObject.layer));
					
				}
				
			}
			#endregion
			
		}

		private void Setup_EDITOR(UnityEngine.UI.Windows.WindowLayout _target) {
			
			_target.elements = _target.elements.Where((e) => e != null).ToList();
			
			var usedTags = new List<LayoutTag>();
			var elements = _target.GetComponentsInChildren<WindowLayoutElement>(true);
			_target.elements = _target.elements.Where((e) => elements.Contains(e)).ToList();
			
			foreach (var element in elements) {
				
				if (usedTags.Contains(element.tag) == true) {
					
					element.Reset();
					
				} else {
					
					if (element.tag != LayoutTag.None) usedTags.Add(element.tag);
					
				}
				
				if (_target.elements.Contains(element) == true) {
					
					continue;
					
				}
				
				if (element.tag == LayoutTag.None) {
					
					// Element not installed
					_target.elements.Add(element);
					
					this.isDirty = true;

				}
				
			}
			
			// Update
			foreach (var element in _target.elements) {
				
				if (element.tag == LayoutTag.None) element.tag = this.GetTag(usedTags);

				/*if (ME.EditorUtilities.IsPrefab(_target.gameObject) == true) {

					element.TurnOff_EDITOR();
					
				} else {

					element.TurnOn_EDITOR();
					
				}*/
				
			}
			
		}
		
		private LayoutTag GetTag(List<LayoutTag> used) {
			
			var tags = System.Enum.GetValues(typeof(LayoutTag));
			for (int i = 1; i < tags.Length; ++i) {
				
				var tag = (LayoutTag)tags.GetValue(i);
				
				if (used.Contains(tag) == true) {
					
					continue;
					
				}
				
				used.Add(tag);
				return tag;
				
			}
			
			return LayoutTag.None;
			
		}
	}

}