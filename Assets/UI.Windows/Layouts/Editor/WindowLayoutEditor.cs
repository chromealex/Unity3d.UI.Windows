using UnityEngine;
using UnityEngine.UI.Windows;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.UI.Windows.Styles;
using System.Reflection;

namespace UnityEditor.UI.Windows {

	[CustomEditor(typeof(WindowLayout), true)]
	[CanEditMultipleObjects()]
	public class WindowLayoutEditor : Editor, IPreviewEditor {

		private bool isDirty = false;

		public void OnEnable() {}
		public void OnDisable() {}

		/*public void OnEnable() {

			EditorApplication.update += this.ValidateElements;

		}

		public void OnDisable() {

			EditorApplication.update -= this.ValidateElements;

		}*/

		public override void OnInspectorGUI() {

			this.DrawDefaultInspector();

			if (Application.isPlaying == true) return;

			this.ValidateElements();

		}

		public void ValidateElements() {

			var _target = this.target as UnityEngine.UI.Windows.WindowLayout;
			if (_target == null) return;

			this.ApplyRoot(_target);
			this.UpdateLinks(_target);

			if (this.isDirty == true) {

				UnityEditor.EditorUtility.SetDirty(_target);
				this.isDirty = false;

			}

		}

		private float GetFactor(Vector2 inner, Vector2 boundingBox) {     

			var widthScale = 0f;
			var heightScale = 0f;
			if (inner.x != 0f) {

				widthScale = boundingBox.x / inner.x;

			}

			if (inner.y != 0f) {

				heightScale = boundingBox.y / inner.y;                

			}
			
			return Mathf.Min(widthScale, heightScale);

		}

		public void CalculateRects(UnityEngine.UI.Windows.WindowLayout _target) {
			
			var elements = _target.elements;

			var pos = _target.transform.localPosition;
			_target.transform.localPosition = Vector3.zero;

			for (int i = 0; i < elements.Count; ++i) {
				
				var element = elements[i];
				if (element == null) continue;
				
				var rectTransform = (element.transform as RectTransform);
				var corners = new Vector3[4];
				rectTransform.GetWorldCorners(corners);
				var rect = new Rect(corners[0].x, -corners[1].y, corners[2].x - corners[1].x, corners[2].y - corners[3].y);
				element.editorRect = rect;
				
				if (i == 0) {
					
					_target.root.editorRect = rect;
					
				}
				
				//element.autoStretchX = (rectTransform.anchorMin.x != rectTransform.anchorMax.x);
				//element.autoStretchY = (rectTransform.anchorMin.y != rectTransform.anchorMax.y);
				
			}
			
			_target.editorScale = _target.transform.localScale.x;
			_target.root.editorRectLocal = (_target.root.transform as RectTransform).rect;
			
			(_target.transform as RectTransform).localPosition = pos;

		}

		public override bool HasPreviewGUI() {

			return true;

		}

		public override void OnPreviewGUI(Rect rect, GUIStyle style) {
			
			this.OnPreviewGUI(Color.white, rect, style, drawInfo: true, selectable: false, hovered: false, selectedElement: null, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo: true, selectable: false, hovered: false, selectedElement: null, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo, selectable, hovered: false, selectedElement: null, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, bool hovered) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo, selectable, hovered, selectedElement: null, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable, WindowLayoutElement selectedElement) {
			
			this.OnPreviewGUI(color, r, background, drawInfo, selectable, hovered: false, selectedElement: selectedElement, onSelection: null, onElementGUI: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, WindowLayoutElement selectedElement, System.Action<WindowLayoutElement> onSelection, List<WindowLayoutElement> highlighted) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo: true, selectable: true, hovered: false, selectedElement: selectedElement, onSelection: onSelection, onElementGUI: null, highlighted: highlighted);
			
		}

		private bool processReconnection = false;
		public void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable, bool hovered, WindowLayoutElement selectedElement, System.Action<WindowLayoutElement> onSelection, System.Action<WindowLayoutElement, Rect, bool> onElementGUI, List<WindowLayoutElement> highlighted) {

			GUI.BeginClip(r);

			var oldColor = GUI.color;
			var c = Color.white;
			c.a = 0.3f;
			GUI.color = c;
			GUI.Box(r, string.Empty, background);
			GUI.color = oldColor;

			var _target = this.target as UnityEngine.UI.Windows.WindowLayout;
			if (_target == null) return;

			color.a = 0.4f;
			var selectedColor = color;
			var notSelectedColor = color;
			selectedColor.a = 0.8f;
			notSelectedColor.a = 0.4f;

			var elements = _target.elements;

			if (ME.EditorUtilities.IsPrefab(_target.gameObject) == false && _target.gameObject.activeInHierarchy == true) {

				this.CalculateRects(_target);

			}
			
			var scaleFactor = 0f;
			if (elements.Count > 0) {

				scaleFactor = this.GetFactor(new Vector2(_target.root.editorRectLocal.width, _target.root.editorRectLocal.height), new Vector2(r.width, r.height));

			}

			if (ME.EditorUtilities.IsPrefab(_target.gameObject) == true) {

				if (this.processReconnection == false && scaleFactor == 0f) {

					this.processReconnection = true;

					// We must re-attach prefab
					var tempGo = UnityEditor.PrefabUtility.InstantiatePrefab(_target.gameObject) as GameObject;
					Selection.activeGameObject = tempGo;
					foreach (var component in tempGo.GetComponents<Component>()) {

						var editor = Editor.CreateEditor(component) as WindowLayoutEditor;
						if (editor != null) {

							editor.CalculateRects(component as UnityEngine.UI.Windows.WindowLayout);
							EditorUtility.SetDirty(component);

						}

					}

					var i = 0;
					EditorApplication.update += () => {

						++i;

						if (i == 30) {

							UnityEditor.PrefabUtility.ReplacePrefab(tempGo, PrefabUtility.GetPrefabParent(tempGo), ReplacePrefabOptions.ConnectToPrefab);
							GameObject.DestroyImmediate(tempGo);

							this.processReconnection = false;

						}

					};

				}

			}

			var scaleFactorCanvas = _target.editorScale > 0f ? 1f / _target.editorScale : 1f;
			scaleFactor *= scaleFactorCanvas;

			var selected = WindowLayoutStyles.styles.boxSelected;
			var styles = WindowLayoutStyles.styles.boxes;
			var stylesSelected = WindowLayoutStyles.styles.boxesSelected;
			var elementSelectedStyle = WindowLayoutStyles.styles.layoutElementSelected;
			var elementHighlightStyle = WindowLayoutStyles.styles.layoutElementHighlighted;
			
			var horArrowsStyle = ME.Utilities.CacheStyle("WindowLayout.GetEditorStyle.horArrowsStyle", "ColorPickerHorizThumb", (style) => new GUIStyle(style));
			var vertArrowsStyle = ME.Utilities.CacheStyle("WindowLayout.GetEditorStyle.vertArrowsStyle", "ColorPickerVertThumb", (style) => new GUIStyle(style));
			
			var horStyle = ME.Utilities.CacheStyle("WindowLayout.GetEditorStyle.horStyle", "box", (style) => new GUIStyle(style));
			var vertStyle = ME.Utilities.CacheStyle("WindowLayout.GetEditorStyle.vertStyle", "box", (style) => new GUIStyle(style));

			var tagSelectedRectBool = false;
			var tagSelectedRect = new Rect();

			var selectedRect = new Rect();
			foreach (var element in elements) {

				if (element == null) continue;

				element.editorDrawDepth = element.GetComponentsInParent<WindowLayoutElement>(true).Length - 1;

				var rect = element.editorRect;

				rect.x *= scaleFactor;
				rect.y *= scaleFactor;
				rect.x += r.x + r.width * 0.5f;
				rect.y += r.y + r.height * 0.5f;
				rect.x -= r.x;
				rect.y -= r.y;
				rect.width *= scaleFactor;
				rect.height *= scaleFactor;

				var styleDepth = Mathf.Clamp(element.editorDrawDepth, 0, WindowLayoutStyles.MAX_DEPTH - 1);

				var style = styles[styleDepth];
				if (rect.Contains(Event.current.mousePosition) == true) {

					style = stylesSelected[styleDepth];

					element.editorHovered = true;
					this.Repaint();

				} else {

					element.editorHovered = false;
					this.Repaint();
					
				}

				if (/*WindowLayoutElement.waitForComponentConnectionElementTemp == element ||*/ Selection.gameObjects.Contains(element.gameObject) == true) {

					style = selected;
					selectedRect = rect;

				}

				if (selectedElement != null) {

					GUI.color = (selectedElement == element) ? selectedColor : notSelectedColor;
					if (selectedElement == element) {

						style = selected;
						selectedRect = rect;

					}

				} else {

					GUI.color = color;

				}

				GUI.Label(rect, string.Empty, style);

				GUI.color = oldColor;

				var boxColor = EditorGUIUtility.isProSkin ? oldColor : new Color(1f, 1f, 1f, 0.2f);

				var marginX = 4f;
				var marginY = 2f;

				if (element.autoStretchX == true) {
					
					var vRect = new Rect(rect);
					vRect.x += marginX;
					vRect.y = vRect.y + vRect.height * 0.5f - vertArrowsStyle.fixedHeight * 0.5f;
					vRect.width -= marginX * 2f;
					vRect.height = vertArrowsStyle.fixedHeight;

					GUI.color = oldColor;
					GUI.Label(vRect, string.Empty, vertArrowsStyle);
					GUI.color = boxColor;
					GUI.Label(vRect, string.Empty, vertStyle);
					
				}

				if (element.autoStretchY == true) {
					
					var vRect = new Rect(rect);
					vRect.x = vRect.x + vRect.width * 0.5f - horArrowsStyle.fixedWidth * 0.5f;
					vRect.y += marginY;
					vRect.width = horArrowsStyle.fixedWidth;
					vRect.height -= marginY * 2f;
					
					GUI.color = oldColor;
					GUI.Label(vRect, string.Empty, horArrowsStyle);
					GUI.color = boxColor;
					GUI.Label(vRect, string.Empty, horStyle);
					
				}

				if (Event.current.type == EventType.Repaint) element.tempEditorRect = rect;

				if (UnityEditor.UI.Windows.Plugins.Flow.Layout.LayoutSettingsEditor.selectedTagIndex == (int)element.tag) {

					tagSelectedRectBool = true;
					tagSelectedRect = rect;

				}

			}

			if (highlighted != null) {

				foreach (var element in elements) {

					if (highlighted.Contains(element) == true) {
						
						GUI.Box(element.tempEditorRect, string.Empty, elementHighlightStyle);

					}

				}

			}

			GUI.Box(selectedRect, string.Empty, elementSelectedStyle);

			if (drawInfo == true || onElementGUI != null || selectable == true) {
				
				WindowLayoutElement maxDepthElement = null;
				var _maxDepth = -1;
				foreach (var element in elements) {

					if (element.showInComponentsList == false) continue;
					if (element.editorHovered == false) continue;
					
					if (_maxDepth < element.editorDrawDepth) {
						
						_maxDepth = element.editorDrawDepth;
						maxDepthElement = element;
						
					}
					
				}

				foreach (var element in elements) {

					if (element.showInComponentsList == false) continue;
					if (element.editorHovered == false || maxDepthElement != element) {
						
						continue;
						
					}

					if (GUI.Button(element.tempEditorRect, string.Empty, GUIStyle.none) == true) {

						if (selectable == true) {

							//WindowLayoutElement.waitForComponentConnectionElementTemp = element;
							//WindowLayoutElement.waitForComponentConnectionTemp = true;
							if (onSelection != null) onSelection(element);

							if (onElementGUI != null) onElementGUI(element, element.tempEditorRect, true);

						}

					} else {
						
						if (onElementGUI != null) onElementGUI(element, element.tempEditorRect, false);

					}

				}

			}

			GUI.color = oldColor;

			GUI.EndClip();

			if (tagSelectedRectBool == true) {

				var scrollValue = Vector2.zero;

				var inspectorType = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
				if (EditorWindow.mouseOverWindow != null) {

					var scrollType = inspectorType.GetField("m_ScrollPosition", BindingFlags.Instance | BindingFlags.Public);
					if (scrollType != null) scrollValue = (Vector2)scrollType.GetValue(EditorWindow.mouseOverWindow);

				}

				var tagSelectedRectOffset = tagSelectedRect;
				tagSelectedRectOffset.center += r.center;
				tagSelectedRectOffset.center -= new Vector2(r.width * 0.5f, r.height * 0.5f);
				var rc = UnityEditor.UI.Windows.Plugins.Flow.Layout.LayoutSettingsEditor.selectedTagRect;
				rc.center -= scrollValue;
				GUI.Box(tagSelectedRectOffset, string.Empty, elementHighlightStyle);
				GUI.Box(rc, string.Empty, elementHighlightStyle);
				WindowLayoutEditor.DrawNavigationArrow(true, Vector2.up, tagSelectedRectOffset.xMin <= 20f ? Vector2.down : Vector2.left, rc, tagSelectedRectOffset);

			}

		}

		private void ApplyRoot(UnityEngine.UI.Windows.WindowLayout _target) {
			
			if (_target.root == null) {
				
				// Trying to find root
				var root = _target.transform.Find("Root");
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
		
		private void UpdateLinks(UnityEngine.UI.Windows.WindowLayout _target) {
			
			this.Setup_EDITOR(_target);
			this.Update_EDITOR(_target);
			
		}
		
		private void Setup_EDITOR(UnityEngine.UI.Windows.WindowLayout _target) {
			
			var elements = _target.GetComponentsInChildren<WindowLayoutElement>(true);
			
			_target.elements = _target.elements.Where((e) => e != null).ToList();
			foreach (var e in elements) {
				
				if (_target.elements.Contains(e) == false) {
					
					// New elements will be added
					e.tag = LayoutTag.None;
					
				}
				
			}
			
			var usedTags = new List<LayoutTag>();
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
				
			}
			
		}

		private void Update_EDITOR(UnityEngine.UI.Windows.WindowLayout _target) {

			foreach (var element in _target.elements) element.OnValidateEditor();
			
			#region COMPONENTS
			_target.canvas = ME.Utilities.FindReferenceChildren<Canvas>(_target);
			_target.raycaster = ME.Utilities.FindReferenceChildren<UnityEngine.EventSystems.BaseRaycaster>(_target);
			#endregion
			
			_target.initialized = (_target.canvas != null);
			
			#region SETUP
			if (_target.initialized == true) {
				
				//WindowSystem.ApplyToSettings(_target.canvas);
				_target.OnValidateEditor();
				
				// Raycaster
				if ((_target.raycaster as GraphicRaycaster) != null) {
					
					(_target.raycaster as GraphicRaycaster).GetType().GetField("m_BlockingMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue((_target.raycaster as GraphicRaycaster), (LayerMask)(1 << _target.gameObject.layer));
					
				}
				
			}
			#endregion
			
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

		private static Vector2 GetPointOnRectEdge(Rect rect, Vector2 dir) {

			if (dir != Vector2.zero) dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
			dir = rect.center + Vector2.Scale(rect.size, dir * 0.5f);

			return dir;

		}

		const float kArrowThickness = 4f;
		const float kArrowHeadSize = 6f;
		const float kTargetOffset = 0f;
		const float kMinSplineForce = 0.2f;
		const float kMaxSplineForce = 0.8f;
		const float maxLength = 300f;

		private static void DrawNavigationArrow(bool selected, Vector2 directionFrom, Vector2 directionTo, Rect fromRect, Rect toRect) {

			UnityEditor.Handles.color = new Color(1.0f, 0.9f, 0.1f, selected == true ? 1f : 0.3f);

			Vector2 sideDirFrom = new Vector2(directionFrom.y, -directionFrom.x);
			Vector2 sideDirTo = new Vector2(directionTo.y, -directionTo.x);

			var fromPoint = WindowLayoutEditor.GetPointOnRectEdge(fromRect, directionFrom);
			var toPoint = WindowLayoutEditor.GetPointOnRectEdge(toRect, directionTo);
			fromPoint.y += kTargetOffset;
			toPoint.y -= kTargetOffset;
			float fromSize = UnityEditor.HandleUtility.GetHandleSize(fromPoint) * 0.05f;
			float toSize = UnityEditor.HandleUtility.GetHandleSize(toPoint) * 0.05f;
			fromPoint += sideDirFrom * fromSize;
			toPoint += sideDirTo * toSize;
			float length = Vector2.Distance(fromPoint, toPoint);
			var force = Mathf.Lerp(kMaxSplineForce, kMinSplineForce, length / maxLength);
			Vector2 fromTangent = directionFrom * length * force;
			Vector2 toTangent = directionTo * length * force;

			UnityEditor.Handles.DrawBezier(fromPoint, toPoint, fromPoint + fromTangent, toPoint + toTangent, UnityEditor.Handles.color, null, kArrowThickness);
			var color = UnityEditor.Handles.color;
			color.a = selected == true ? 1f : 0.5f;
			UnityEditor.Handles.color = color;
			UnityEditor.Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + (directionTo - sideDirTo) * toSize * kArrowHeadSize);
			UnityEditor.Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + (directionTo + sideDirTo) * toSize * kArrowHeadSize);

		}

	}

}