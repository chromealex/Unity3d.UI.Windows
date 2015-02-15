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

			if (this.editorActive == false) return;

			this.editorActive = false;

			ME.EditorUtilities.ClearLayoutElement(this);

		}

		public void TurnOn_EDITOR() {

			if (this.editorActive == true) return;

			this.editorActive = true;
			
			this.editorColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.7f);

			this.Update();

		}

		[HideInInspector][SerializeField]
		public GameObject editorLabel;
		public override void Update_EDITOR() {
			
			base.Update_EDITOR();

			if (this.editorActive == false) return;

			if (Application.isPlaying == true) {
				/*
				if (this.editorBody != null) {

					Component.Destroy(this.editorBody);
					Component.Destroy(this.GetComponent<CanvasRenderer>());

				}
				if (this.editorLabel != null) GameObject.Destroy(this.editorLabel);*/
				return;

			}

			if (this.editorLabel == null) {

				var t = this.transform.FindChild("__EditorLabel");
				if (t != null) this.editorLabel = t.gameObject;

			}

			var descr = this.tag.ToString() + ": " + this.comment + "\n" + "(Animation: " + (this.animation != null ? this.animation.name : "None") + ")";

			if (this.editorLabel == null) {

				this.editorLabel = new GameObject("__EditorLabel");
				this.editorLabel.transform.SetParent(this.transform);

				this.editorLabel.hideFlags = HideFlags.HideAndDontSave;
				this.CenterAndStretch(this.editorLabel.transform);

				var subLabel = new GameObject("Label");
				subLabel.transform.SetParent(this.editorLabel.transform);
				this.CenterAndStretch(subLabel.transform);
				
				var image = this.editorLabel.AddComponent<Image>();
				image.color = this.editorColor;

				var layout = this.editorLabel.AddComponent<LayoutElement>();
				layout.ignoreLayout = true;

				var shadow = subLabel.AddComponent<Shadow>();
				shadow.effectDistance = new Vector2(1f, -1f);
				shadow.useGraphicAlpha = true;

				var text = subLabel.AddComponent<Text>();
				text.alignment = TextAnchor.MiddleCenter;
				text.fontStyle = FontStyle.Bold;
				text.horizontalOverflow = HorizontalWrapMode.Overflow;
				text.verticalOverflow = VerticalWrapMode.Overflow;
				text.fontSize = 20;
				var color = Color.white;
				color.a = 0.5f;
				text.color = color;
				text.text = descr;

			} else {

				var text = this.editorLabel.GetComponentInChildren<Text>();

				this.editorLabel.transform.SetAsLastSibling();
				text.text = descr;

			}

		}

		private void CenterAndStretch(Transform tr) {

			var rect = tr as RectTransform;
			if (rect == null) rect = tr.gameObject.AddComponent<RectTransform>();

			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.pivot = Vector2.one * 0.5f;
			rect.anchoredPosition3D = Vector3.zero;
			rect.sizeDelta = Vector2.zero;
			rect.localScale = Vector3.one;

		}
		#endif
		
	}
	
}

