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
		[HideInInspector][SerializeField]
		public Image editorBody;
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
			
			this.editorBody = this.gameObject.GetComponent<Image>();
			if (this.editorBody == null) {
				
				Component.DestroyImmediate(this.GetComponent<Image>());
				Component.DestroyImmediate(this.GetComponent<CanvasRenderer>());
				this.editorBody = this.gameObject.AddComponent<Image>();
				this.editorBody.hideFlags = HideFlags.HideAndDontSave;

				this.editorBody.color = this.editorColor;

			}

			if (this.editorLabel == null) {

				var t = this.transform.FindChild("__EditorLabel");
				if (t != null) this.editorLabel = t.gameObject;

			}

			var descr = this.tag.ToString() + ": " + this.comment + "\n" + "(Animation: " + (this.animation != null ? this.animation.name : "None") + ")";

			if (this.editorLabel == null) {

				GameObject.DestroyImmediate(this.editorLabel);
				this.editorLabel = new GameObject("__EditorLabel");
				this.editorLabel.hideFlags = HideFlags.HideAndDontSave;

				var shadow = this.editorLabel.AddComponent<Shadow>();
				shadow.hideFlags = HideFlags.HideAndDontSave;
				shadow.effectDistance = new Vector2(1f, -1f);
				shadow.useGraphicAlpha = true;

				this.editorLabel.transform.SetParent(this.transform);
				var text = this.editorLabel.AddComponent<Text>();
				text.hideFlags = HideFlags.HideAndDontSave;
				text.alignment = TextAnchor.MiddleCenter;
				text.fontStyle = FontStyle.Bold;
				text.horizontalOverflow = HorizontalWrapMode.Overflow;
				text.verticalOverflow = VerticalWrapMode.Overflow;
				text.fontSize = 20;
				var color = Color.white;
				color.a = 0.5f;
				text.color = color;
				text.text = descr;

				var rect = this.editorLabel.transform as RectTransform;
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.pivot = Vector2.one * 0.5f;
				rect.anchoredPosition3D = Vector3.zero;
				rect.sizeDelta = Vector2.zero;

				rect.localScale = Vector3.one;

			} else {

				var text = this.editorLabel.GetComponent<Text>();

				this.editorLabel.transform.SetAsLastSibling();
				text.text = descr;

			}

		}
		#endif
		
	}
	
}

