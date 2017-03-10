//#define FLOW_PLUGIN_HEATMAP
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

	public class WindowComponent : 
			#if FLOW_PLUGIN_HEATMAP
			UnityEngine.UI.Windows.Plugins.Heatmap.Components.HeatmapWindowComponent
			#else
			WindowComponentBase
			#endif
			, IComponent {

		private WindowLayoutBase layoutRoot;

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.layoutRoot = null;

		}

		public override void Setup(WindowLayoutBase layoutRoot) {
			
            base.Setup(layoutRoot);

			this.layoutRoot = layoutRoot;
			
		}
		
		public virtual void Setup(IComponentParameters parameters) {

		}

		/// <summary>
		/// Gets the layout root.
		/// Basicaly it's the parent transform from current. But it's not always so.
		/// Sometimes you need to get layout element where this component (or it's parent) stored to play animation (for example).
		/// </summary>
		/// <returns>The layout root.</returns>
		public WindowLayoutBase GetLayoutRoot() {
			
			return this.layoutRoot;
			
		}

		/// <summary>
		/// Registers the sub component.
		/// If you want to instantiate new component manualy but wants the window events - register this component here.
		/// </summary>
		/// <param name="subComponent">Sub component.</param>
		public override void RegisterSubComponent(WindowObjectElement subComponent) {

            if (this.GetWindow() != null) {

                subComponent.Setup(this.GetLayoutRoot());

            }

		    base.RegisterSubComponent(subComponent);

		}

		#if UNITY_EDITOR
		private UnityEditor.Editor[] graphicEditors;
		public void OnPreviewGUI(Rect r, GUIStyle background) {

			var i = 0;

			var graphics = this.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
			this.graphicEditors = new UnityEditor.Editor[graphics.Length];
			foreach (var graphic in graphics) {
				
				this.graphicEditors[i] = (this.graphicEditors[i] == null) ? UnityEditor.Editor.CreateEditor(graphic) : null;
				if (this.graphicEditors[i] != null && this.graphicEditors[i].HasPreviewGUI() == true) {

					var rectSize = graphic.rectTransform.sizeDelta;
					if (rectSize.x < r.width && rectSize.x > 0f) {

						r.x += r.width * 0.5f - rectSize.x * 0.5f;
						r.width = rectSize.x;

					}

					if (rectSize.y < r.height && rectSize.y > 0f) {
						
						r.y += r.height * 0.5f - rectSize.y * 0.5f;
						r.height = rectSize.y;

					}

					this.graphicEditors[i].OnPreviewGUI(r, background);
					
				}

				++i;
				
			}

		}
		#endif

	}

}