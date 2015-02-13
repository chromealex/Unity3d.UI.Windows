using UnityEngine;
using UnityEngine.UI.Windows;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEditor.UI.Windows {

	[CustomEditor(typeof(WindowLayout))]
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
				
				// Canvas
				_target.canvas.overrideSorting = true;
				_target.canvas.sortingLayerName = "Windows";
				_target.canvas.sortingOrder = 0;
				
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

				if (ME.EditorUtilities.IsPrefab(_target.gameObject) == true) {

					element.TurnOff_EDITOR();
					
				} else {

					element.TurnOn_EDITOR();
					
				}
				
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