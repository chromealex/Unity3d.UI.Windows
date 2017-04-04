using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

namespace ME {

	[CustomPropertyDrawer(typeof(Ease.Type))]
	public class TweenerEasePopup : PropertyDrawer {

		private TweenerEasePopupWindow.Item item;
		private Editor editor;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			var labelPosition = position;
			labelPosition.width = EditorGUIUtility.labelWidth;

			var previewPosition = position;
			previewPosition.width = 100f;
			previewPosition.x = position.x + position.width - previewPosition.width;

			var fieldPosition = position;
			fieldPosition.width -= labelPosition.width + previewPosition.width;
			fieldPosition.x += labelPosition.width;

			EditorGUI.LabelField(labelPosition, label);
			var value = property.hasMultipleDifferentValues == true ? "-" : property.enumDisplayNames[property.enumValueIndex];

			if (this.item == null) {

				this.item = new TweenerEasePopupWindow.Item((Ease.Type)property.enumValueIndex);

			}

			if (this.editor == null) {

				this.editor = Editor.CreateEditor(property.serializedObject.targetObjects);

			}

			this.item.OnGUI(previewPosition);

			if (Event.current.type == EventType.Repaint) {

				if (position.Contains(Event.current.mousePosition) == true) {
					
					this.item.Play();

				} else {

					this.item.Pause();

				}

			}

			this.editor.Repaint();

			if (GUI.Button(fieldPosition, value, EditorStyles.popup) == true) {

				var offset = new Vector2(0f, 0f);
				var rect = fieldPosition;
				var vector = GUIUtility.GUIToScreenPoint(new Vector2(fieldPosition.x, fieldPosition.y));
				rect.x = vector.x;
				rect.y = vector.y;

				var popup = TweenerEasePopupWindow.Create(new Rect(rect.x + offset.x, rect.y + rect.height + offset.y, rect.width - offset.x, 300f), (newType) => {

					property.enumValueIndex = (int)newType;
					property.serializedObject.ApplyModifiedProperties();
					this.item = null;

				});
				popup.ShowAsDropDown();

			}

		}

	}

	internal class TweenerEasePopupWindow : EditorWindow {

		public static class Styles {

			public static GUIStyle label;
			public static GUIStyle progressBar;
			public static GUIStyle sliderDot;

			static Styles() {

				Styles.label = new GUIStyle((GUIStyle)"ChannelStripAttenuationBar");

				Styles.progressBar = new GUIStyle((GUIStyle)"MeLivePlayBar");
				Styles.progressBar.fixedHeight = 0f;

				Styles.sliderDot = new GUIStyle((GUIStyle)"U2D.dragDot");

			}

		}

		public class Item {

			public Ease.Type type;
			public Tweener.ITransition transition;
			public double progress;
			private double prevTime;

			public Item(Ease.Type type) {

				this.type = type;
				this.transition = Ease.GetByType(this.type);

			}

			public string GetCaption() {

				return this.type.ToString();

			}

			public void OnGUI(Rect rect) {

				TweenerEasePopupWindow.DrawProgress(rect, this.transition, (float)this.progress, this.GetCaption());

			}

			public void Play() {

				var time = EditorApplication.timeSinceStartup;
				var deltaTime = time - this.prevTime;
				this.prevTime = time;

				this.progress += deltaTime;

				if (this.progress >= 1d) {

					this.progress = 0d;

				}

			}

			public void Pause() {

				this.progress = 0d;
				this.prevTime = 0d;

			}

		}

		private Rect screenRect;
		private System.Action<Ease.Type> onSelect;
		private System.Array types;
		private Vector2 scrollPosition;

		public List<Item> items = new List<Item>();

		public static void DrawProgress(Rect rect, Tweener.ITransition transition, float progress, string caption) {

			var sizeX = 10f;
			var sizeY = 10f;

			var progressSizeY = 2f;

			GUI.Box(rect, Texture2D.blackTexture);

			GUI.Label(rect, caption, TweenerEasePopupWindow.Styles.label);

			var progressRect = new Rect(rect.x, rect.y + rect.height - progressSizeY - 1f, rect.width * progress, progressSizeY);
			GUI.Box(progressRect, string.Empty, TweenerEasePopupWindow.Styles.progressBar);

			var boxRect = new Rect(transition.interpolate(rect.x, rect.width - sizeX, progress, 1f), rect.y + rect.height * 0.5f - sizeY * 0.5f, sizeX, sizeY);
			GUI.Box(boxRect, string.Empty, TweenerEasePopupWindow.Styles.sliderDot);

		}

		public static TweenerEasePopupWindow Create(Rect screenRect, System.Action<Ease.Type> onSelect) {

			var types = System.Enum.GetValues(typeof(Ease.Type));

			var popup = EditorWindow.CreateInstance<TweenerEasePopupWindow>();
			popup.screenRect = screenRect;
			popup.onSelect = onSelect;
			popup.types = types;
			popup.items = new List<Item>();
			foreach (var item in popup.types) {

				popup.items.Add(new Item((Ease.Type)item));

			}

			return popup;

		}

		public void ShowAsDropDown() {

			this.Init();

		}

		private void Init() {

			this.ShowAsDropDown(new Rect(this.screenRect.x, this.screenRect.y, 1f, 1f), new Vector2(this.screenRect.width, this.screenRect.height));
			this.Focus();
			this.wantsMouseMove = true;

		}

		public void OnGUI() {

			var margin = new RectOffset(10, 10, 10, 10);
			var padding = new RectOffset(2, 2, 2, 2);
			var cellWidth = 80f;
			var cellHeight = 40f;

			var maxWidth = this.position.width - margin.horizontal - 20f;

			var scrollRect = new Rect(0f, 0f, this.position.width, this.position.height);
			var viewRect = new Rect(0f, 0f, maxWidth, Mathf.Ceil(this.items.Count / (float)(Mathf.CeilToInt(maxWidth / cellWidth) - 1)) * cellHeight + cellHeight - margin.vertical);
			this.scrollPosition = GUI.BeginScrollView(scrollRect, this.scrollPosition, viewRect, false, false);
			{
				
				var curX = (float)margin.left;
				var curY = (float)margin.top;
				foreach (var item in this.items) {
					
					var rect = new Rect(curX + padding.left, curY + padding.top, cellWidth - padding.horizontal, cellHeight - padding.vertical);
					{

						item.OnGUI(rect);
						if (GUI.Button(rect, string.Empty, GUIStyle.none) == true) {

							if (this.onSelect != null) this.onSelect.Invoke(item.type);
							this.Close();

						}

						if (Event.current.type == EventType.Repaint) {
							
							if (rect.Contains(Event.current.mousePosition) == true) {

								// Start Anim
								item.Play();

							} else {

								// End Anim
								item.Pause();

							}

						}

					}

					curX += cellWidth;

					if (curX >= maxWidth - cellWidth) {

						curX = (float)margin.left;
						curY += cellHeight;

					}

				}

			}
			GUI.EndScrollView();

			this.Repaint();

		}

	}

}