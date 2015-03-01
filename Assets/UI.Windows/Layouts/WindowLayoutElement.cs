using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class WindowLayoutElement : WindowLayoutBase {
		
		#if UNITY_EDITOR
		public string comment;
		#endif

		[ReadOnly]
		new public LayoutTag tag = LayoutTag.None;

		#if UNITY_EDITOR
		public void OnEnable() {

			if (Application.isPlaying == true) return;

			this.Reset();

		}

		[ContextMenu("Reset")]
		public void Reset() {

			this.tag = LayoutTag.None;

		}

		//[HideInInspector]
		public bool editorActive = false;
		[HideInInspector]
		public Color editorColor;
		public void TurnOff_EDITOR() {

			this.editorActive = false;
		}

		public void TurnOn_EDITOR() {

			if (this.editorActive == true) return;

			this.editorActive = true;
			
			this.editorColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.7f);

			this.Update();

		}
		
		public string GetDescription() {
			
			return this.tag + ": " + this.comment + "\n" + "(Animation: " + (this.animation != null ? this.animation.name : "None") + ")";
		}

		public GUIStyle GetActiveGUIStyle() {

			var result = new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 20, alignment = TextAnchor.MiddleCenter };

			var color = Color.white;
			color.a = 0.7f;

			result.normal.textColor = color;

			return result;
		}

		public GUIStyle GetInactiveGUIStyle() {

			var result = new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 20, alignment = TextAnchor.MiddleCenter };

			var color = Color.white;
			color.a = 0.2f;

			result.normal.textColor = color;

			return result;
		}

		public void DrawHandle(bool isActive = true) {

			var corners = new Vector3[4];

			var textScaleFactor = 100;
			var maxTextSize = 100;

			#region Layout pass

			( transform as RectTransform ).GetWorldCorners( corners );

			var targetColor = editorColor;
			if ( !isActive ) {

				var inactiveColor = Color.gray;
				inactiveColor.a = 0f;

				targetColor += inactiveColor * 0.3f;
			}

			UnityEditor.Handles.DrawSolidRectangleWithOutline( corners, targetColor, targetColor );

			#endregion

			#region Text pass

			var style = isActive ? GetActiveGUIStyle() : GetInactiveGUIStyle();
			style.fontSize = (int)( style.fontSize / UnityEditor.HandleUtility.GetHandleSize( transform.position ) * textScaleFactor );
			style.fontSize = Mathf.Clamp( style.fontSize, 1, maxTextSize );

			UnityEditor.Handles.Label( transform.position, GetDescription(), style );

			#endregion
		}

		#endif
		
	}
	
}

