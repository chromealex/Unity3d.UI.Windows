using UnityEngine;
using UnityEngine.UI.Windows;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEditor.UI.Windows {
	
	[CustomEditor(typeof(WindowLayoutElement), true)]
	[CanEditMultipleObjects()]
	public class WindowLayoutElementEditor : WindowComponentBaseEditor {

		private WindowLayoutElement _target;
		private WindowLayout layout;

		public void OnEnable() {
			
			//SceneView.onSceneGUIDelegate -= this.OnSceneView;
			//SceneView.onSceneGUIDelegate += this.OnSceneView;

			this._target = this.target as WindowLayoutElement;
			if (this._target == null) return;

			this.layout = this._target.GetComponentInParent<WindowLayout>();

			/*if (this._target != null) {

				this.lastPivot = (this._target.transform as RectTransform).pivot;

			}*/

		}

		/*public void OnDisable() {
			
			SceneView.onSceneGUIDelegate -= this.OnSceneView;

		}

		private Vector2 lastPivot;
		public void OnSceneView(SceneView view) {

			if (this._target == null || this.layout == null) return;

			var pivot = (this._target.transform as RectTransform).pivot;

			if (this.layout.showGrid == true && this.lastPivot == pivot) {

				this.lastPivot = pivot;

				var gridSize = this.layout.gridSize;
				this.ArrangeByGrid(this._target, gridSize);

			}

		}

		private void ArrangeByGrid(WindowLayoutElement element, Vector2 grid) {

			var rect = element.transform as RectTransform;

			rect.anchoredPosition = this.GridBy(rect.anchoredPosition, grid);
			rect.sizeDelta = this.GridBy(rect.sizeDelta, grid);

		}
		
		private Vector2 GridBy(Vector2 position, Vector2 gridSize) {
			
			position.x = Mathf.Round(position.x / gridSize.x) * gridSize.x;
			position.y = Mathf.Round(position.y / gridSize.x) * gridSize.x;
			
			return position;
			
		}*/
		
		public override bool HasPreviewGUI() {
			
			return true;
			
		}

		private IPreviewEditor editor;
		public override void OnPreviewGUI(Rect r, GUIStyle background) {

			if (this.layout != null) {

				if (this.editor == null) this.editor = Editor.CreateEditor(this.layout) as IPreviewEditor;
				if (this.editor != null) {

					this.editor.OnPreviewGUI(Color.white, r, background, drawInfo: false, selectable: false, selectedElement: this._target);

				}

			}

		}

	}

}