using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Animations;
using UnityEditor.UI.Windows.Plugins.Flow;
using UnityEditor;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using UnityEditor.UI.Windows.Extensions;

namespace UnityEditor.UI.Windows.Animations {

	[CanEditMultipleObjects()]
	[CustomEditor(typeof(TransitionInputTemplateParameters), editorForChildClasses: true)]
	public class TransitionInputTemplateParametersEditor : Editor, IPreviewEditor {
		
		public const int WIDTH = 400;
		public const int HEIGHT = 300;

		private class Styles {
			
			public GUISkin skin;
			public GUIStyle transitionA;
			public GUIStyle transitionB;
			
			public Styles() {
				
				this.skin = Resources.Load<GUISkin>("UI.Windows/Core/Styles/" + (EditorGUIUtility.isProSkin == true ? "SkinDark" : "SkinLight"));
				this.transitionA = this.skin.FindStyle("TransitionA");
				this.transitionB = this.skin.FindStyle("TransitionB");

			}
			
		}
		
		private Styles styles = new Styles();
		
		private float sliderPosition = 0f;
		
		private GameObject sceneTestContainer;
		private LayoutWindowType windowA;
		private LayoutWindowType windowB;
		private RenderTexture targetTexture;
		
		private Material material;

		private bool changed = false;
		private bool hovered = false;

		public void OnEnable() {

			EditorApplication.update += this.OnUpdate;

		}

		public void OnDisable() {
			
			EditorApplication.update -= this.OnUpdate;

			this.sliderPosition = 0f;
			this.Repaint();

			if (this.sceneTestContainer != null) {

				this.sceneTestContainer.SetActive(false);
				GameObject.DestroyImmediate(this.sceneTestContainer);

				if (this.windowA != null) Object.DestroyImmediate(this.windowA.gameObject);
				if (this.windowB != null) Object.DestroyImmediate(this.windowB.gameObject);

			}

		}

		private double lastTime;
		public void OnUpdate() {
			
			var _target = this.target as TransitionVideoInputTemplateParameters;
			if (_target != null) {
				
				this.DrawVideo(_target);
				
			}

			var _targetAudio = this.target as TransitionAudioInputTemplateParameters;
			if (_targetAudio != null) {
				
				this.DrawAudio(_targetAudio);
				
			}

		}
		
		private AnimationCurve audioCurveIn;
		private AnimationCurve audioCurveOut;
		public void DrawAudio(TransitionAudioInputTemplateParameters _target) {
			
			if (_target.transition == null) return;

			/*if (this.audioCurve == null) {

				this.audioCurve = new AnimationCurve();

			}

			this.audioCurve.keys = new Keyframe[0];

			var keys = new List<Keyframe>();
			var pars = _target.GetParameters<TransitionBase.ParametersAudioBase>();
			for (float t = 0f; t <= pars.inDuration; t += 0.05f) {

				keys.Add(new Keyframe(t, pars.GetVolumeValueIn(t)));

			}
			
			this.audioCurve.postWrapMode = WrapMode.Clamp;
			this.audioCurve.preWrapMode = WrapMode.Clamp;
			this.audioCurve.keys = keys.ToArray();

			this.Repaint();*/

		}

		public void DrawVideo(TransitionVideoInputTemplateParameters _target) {

			if (_target.transition == null) return;

			if (this.hovered == true) {

				var lastTime = this.lastTime;
				var curTime = EditorApplication.timeSinceStartup;
				var delta = (float)(curTime - lastTime);
				this.lastTime = curTime;

				this.sliderPosition += delta;
				if (this.sliderPosition > 2f) {
					
					this.sliderPosition = -0.5f;
					
				}

				this.changed = true;
				this.Repaint();

			}

			if (this.sceneTestContainer == null || this.windowA == null) {

				this.sceneTestContainer = new GameObject("Transition-Temp");
				this.sceneTestContainer.hideFlags = HideFlags.DontSave;//HideFlags.HideAndDontSave;
				
				var screenSource = Resources.Load<GameObject>("UI.Windows/Core/Transitions/TransitionScreen");
				var windowSource = screenSource.GetComponent<LayoutWindowType>();
				
				ImageComponent image = null;
				
				// Create 2 windows
				
				this.windowA = WindowSystem.Show(windowSource);
				if (this.windowA != null) {

					this.windowA.transform.SetParent(this.sceneTestContainer.transform);
					image = this.windowA.GetLayoutComponent<ImageComponent>();
					if (image != null) image.SetImage(this.styles.transitionA.normal.background);
					
					this.windowB = WindowSystem.Show(windowSource);
					if (this.windowB != null) {

						this.windowB.transform.SetParent(this.sceneTestContainer.transform);
						image = this.windowB.GetLayoutComponent<ImageComponent>();
						if (image != null) image.SetImage(this.styles.transitionB.normal.background);
						
						this.changed = true;

					} else {

						Object.DestroyImmediate(this.windowA.gameObject);

					}

				}

			}
			
			if (this.targetTexture == null) {
				
				this.targetTexture = new RenderTexture(TransitionInputTemplateParametersEditor.WIDTH, TransitionInputTemplateParametersEditor.HEIGHT, 24, RenderTextureFormat.ARGB32);
				
				this.changed = true;
				
			}
			
			if (this.changed == true) {
				
				var videoParameters = _target.GetParameters<TransitionBase.ParametersVideoBase>();
				if (videoParameters != null && this.windowA != null && this.windowB != null) {

					this.sceneTestContainer.SetActive(true);

					this.material = videoParameters.GetMaterialInstance();
					var lerpA = videoParameters.materialLerpA;
					var lerpB = videoParameters.materialLerpB;

					// Take screenshot
					Graphics.Blit(Texture2D.blackTexture, this.targetTexture);

					// Rewind to value
					this.windowA.transition.Setup(this.windowA);
					_target.transition.SetInState(_target, this.windowA, null);
					this.windowA.transition.Apply(_target.transition, _target, forward: false, value: this.sliderPosition, reset: false);

					if (this.material == null || lerpA == false) {
						
						Graphics.Blit(this.TakeScreenshot(this.windowA.workCamera), this.targetTexture);
						
					} else {
						
						Graphics.Blit(this.TakeScreenshot(this.windowA.workCamera), this.targetTexture, this.material);
						
					}

					this.windowB.transition.Setup(this.windowB);
					this.windowB.transition.Apply(_target.transition, _target, forward: true, value: this.sliderPosition, reset: true);

					if (this.material == null || lerpB == false) {
						
						Graphics.Blit(this.TakeScreenshot(this.windowB.workCamera), this.targetTexture);
						
					} else {
						
						Graphics.Blit(this.TakeScreenshot(this.windowB.workCamera), this.targetTexture, this.material);
						
					}

					// Deactivate
					this.sceneTestContainer.SetActive(false);

				}

			}

			if (this.hovered == true) {
				
				this.Repaint();

			}

		}

		public override bool HasPreviewGUI() {
			
			var _target = this.target as TransitionInputTemplateParameters;
			if (_target == null) return false;
			if (_target.transition == null) return false;

			return true;

		}

		public override void OnPreviewGUI(Rect rect, GUIStyle style) {

			this.OnPreviewGUI(Color.white, rect, style, drawInfo: true, selectable: false, hovered: false, selectedElement: null, onSelection: null, highlighted: null);

		}

		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style) {

			this.OnPreviewGUI(color, rect, style, drawInfo: true, selectable: false, hovered: false, selectedElement: null, onSelection: null, highlighted: null);

		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo, selectable, hovered: false, selectedElement: null, onSelection: null, highlighted: null);
			
		}
		
		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, bool hovered) {
			
			this.OnPreviewGUI(color, rect, style, drawInfo, selectable, hovered, selectedElement: null, onSelection: null, highlighted: null);
			
		}

		public void OnPreviewGUI(Color color, Rect r, GUIStyle background, bool drawInfo, bool selectable, WindowLayoutElement selectedElement) {
			
			this.OnPreviewGUI(color, r, background, drawInfo, selectable, hovered: false, selectedElement: selectedElement, onSelection: null, highlighted: null);
			
		}

		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, WindowLayoutElement selectedElement, System.Action<WindowLayoutElement> onSelection, List<WindowLayoutElement> highlighted) {

			this.OnPreviewGUI(color, rect, style, drawInfo: true, selectable: true, hovered: false, selectedElement: selectedElement, onSelection: onSelection, highlighted: highlighted);

		}

		public void OnPreviewGUI(Color color, Rect rect, GUIStyle style, bool drawInfo, bool selectable, bool hovered, WindowLayoutElement selectedElement, System.Action<WindowLayoutElement> onSelection, List<WindowLayoutElement> highlighted) {
			
			const float sliderHeight = 20f;
			const float padding = 40f;

			if (this.styles == null) this.styles = new Styles();
			
			var _target = this.target as TransitionInputTemplateParameters;
			if (_target == null) return;
			
			this.changed = false;

			if (drawInfo == true) {
				
				var sliderRect = new Rect(rect.x + padding, rect.y, rect.width - padding * 2f, sliderHeight);
				rect.y += sliderHeight;
				rect.height -= sliderHeight;
				
				var newValue = GUI.HorizontalSlider(sliderRect, this.sliderPosition, 0f, 1f);
				if (newValue != this.sliderPosition) this.changed = true;
				this.sliderPosition = newValue;

			} else {

				this.changed = true;
				this.hovered = hovered;

			}

			var _targetVideo = this.target as TransitionVideoInputTemplateParameters;
			if (_targetVideo != null) {
				
				if (this.targetTexture != null) {
					
					GUI.DrawTexture(rect, this.targetTexture);
					
				}

			}
			
			var _targetAudio = this.target as TransitionAudioInputTemplateParameters;
			if (_targetAudio != null) {
				
				var pars = _target.GetParameters<TransitionBase.ParametersAudioBase>();
				var oldEvent = new Event(Event.current);
				//Event.current.Use();
				this.audioCurveIn = new AnimationCurve();
				for (float t = 0f; t <= pars.inDuration + 0.01f; t += 0.05f) {
					
					var value = pars.GetVolumeValueIn(t);
					var key = new Keyframe(t, value);
					this.audioCurveIn.AddKey(key);
					
				}
				
				this.audioCurveOut = new AnimationCurve();
				for (float t = 0f; t <= pars.outDuration + 0.01f; t += 0.05f) {
					
					var value = pars.GetVolumeValueOut(t);
					var key = new Keyframe(t, value);
					this.audioCurveOut.AddKey(key);
					
				}

				AnimationCurveUtility.SetLinear(ref this.audioCurveIn);
				AnimationCurveUtility.SetLinear(ref this.audioCurveOut);
				
				this.audioCurveIn.postWrapMode = WrapMode.Clamp;
				this.audioCurveIn.preWrapMode = WrapMode.Clamp;

				this.audioCurveOut.postWrapMode = WrapMode.Clamp;
				this.audioCurveOut.preWrapMode = WrapMode.Clamp;

				{
					if (Event.current.type == EventType.MouseDown) Event.current.type = EventType.Repaint;
					EditorGUI.CurveField(new Rect(rect.x, rect.y, rect.width, rect.height * 0.5f), string.Empty, this.audioCurveIn, Color.red, new Rect(0f, 0f, pars.inDuration, 1f));
					EditorGUI.CurveField(new Rect(rect.x, rect.y + rect.height * 0.5f, rect.width, rect.height * 0.5f), string.Empty, this.audioCurveOut, Color.cyan, new Rect(0f, 0f, pars.outDuration, 1f));

					if (this.sliderPosition > 0f) {

						GUI.Box(new Rect(rect.x + (this.sliderPosition * rect.width), rect.y, 3f, rect.height * 0.5f), string.Empty);
						GUI.Box(new Rect(rect.x + (this.sliderPosition * rect.width), rect.y + rect.height * 0.5f, 3f, rect.height * 0.5f), string.Empty);

					}

					EditorGUI.DropShadowLabel(new Rect(rect.x, rect.y, rect.width, rect.height * 0.5f), "In");
					EditorGUI.DropShadowLabel(new Rect(rect.x, rect.y + rect.height * 0.5f, rect.width, rect.height * 0.5f), "Out");
				}
				Event.current = oldEvent;
				
			}

		}

		public Texture2D TakeScreenshot(Camera camera) {
			
			var width = this.targetTexture.width;
			var height = this.targetTexture.height;
			
			Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
			
			camera.targetTexture = this.targetTexture;
			camera.Render();
			
			RenderTexture.active = this.targetTexture;
			
			screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			screenshot.Apply(false);
			
			camera.targetTexture = null;
			
			RenderTexture.active = null;
			
			return screenshot;
			
		}

	}

}