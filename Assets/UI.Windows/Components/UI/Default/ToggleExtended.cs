using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI {

	public class ToggleExtended : Selectable, IEventSystemHandler, IPointerClickHandler, ISubmitHandler, ICanvasElement {
		//
		// Fields
		//
		public ToggleExtended.ToggleEvent onValueChanged = new ToggleExtended.ToggleEvent ();

		[FormerlySerializedAs ("m_IsActive"), SerializeField, Tooltip ("Is the toggle currently on or off?")]
		private bool m_IsOn;

		[SerializeField]
		private ToggleGroupExtended m_Group;

		public Graphic graphic;

		public ToggleExtended.ToggleTransition toggleTransition = ToggleExtended.ToggleTransition.Fade;

		//
		// Properties
		//
		public ToggleGroupExtended group {
			get {
				return this.m_Group;
			}
			set {
				this.m_Group = value;
				if (Application.isPlaying) {
					this.SetToggleGroup (this.m_Group, true);
					this.PlayEffect (true);
				}
			}
		}

		public bool isOn {
			get {
				return this.m_IsOn;
			}
			set {
				this.Set (value);
			}
		}

		//
		// Methods
		//
		public virtual void GraphicUpdateComplete ()
		{
		}

		private void InternalToggle ()
		{
			if (!this.IsActive () || !this.IsInteractable ()) {
				return;
			}
			this.isOn = !this.isOn;
		}

		public virtual void LayoutComplete ()
		{
		}

		protected override void OnDidApplyAnimationProperties ()
		{
			if (this.graphic != null) {
				bool flag = !Mathf.Approximately (this.graphic.canvasRenderer.GetColor ().a, 0);
				if (this.m_IsOn != flag) {
					this.m_IsOn = flag;
					this.Set (!flag);
				}
			}
			base.OnDidApplyAnimationProperties ();
		}

		protected override void OnDisable ()
		{
			this.SetToggleGroup (null, false);
			base.OnDisable ();
		}

		protected override void OnEnable ()
		{
			base.OnEnable ();
			this.SetToggleGroup (this.m_Group, false);
			this.PlayEffect (true);
		}

		public virtual void OnPointerClick (PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left) {
				return;
			}
			this.InternalToggle ();
		}

		public virtual void OnSubmit (BaseEventData eventData)
		{
			this.InternalToggle ();
		}

		#if UNITY_EDITOR
		protected override void OnValidate ()
		{
			base.OnValidate ();
			this.Set (this.m_IsOn, false);
			this.PlayEffect (this.toggleTransition == ToggleExtended.ToggleTransition.None);
			UnityEditor.PrefabType prefabType = UnityEditor.PrefabUtility.GetPrefabType (this);
			if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying) {
				CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild (this);
			}
		}
		#endif

		public virtual void Rebuild (CanvasUpdate executing)
		{
			if (executing == CanvasUpdate.Prelayout) {
				this.onValueChanged.Invoke (this.m_IsOn);
			}
		}

		private void Set (bool value, bool sendCallback)
		{
			if (this.m_IsOn == value) {
				return;
			}
			this.m_IsOn = value;
			if (this.m_Group != null && this.IsActive () && (this.m_IsOn || (!this.m_Group.AnyTogglesOn () && !this.m_Group.allowSwitchOff))) {
				this.m_IsOn = true;
				this.m_Group.NotifyToggleOn (this);
			}
			this.PlayEffect (this.toggleTransition == ToggleExtended.ToggleTransition.None);
			if (sendCallback) {
				this.onValueChanged.Invoke (this.m_IsOn);
			}
		}

		private void Set (bool value)
		{
			this.Set (value, true);
		}

		private void SetToggleGroup (ToggleGroupExtended newGroup, bool setMemberValue)
		{
			ToggleGroupExtended group = this.m_Group;
			if (this.m_Group != null) {
				this.m_Group.UnregisterToggle (this);
			}
			if (setMemberValue) {
				this.m_Group = newGroup;
			}
			if (this.m_Group != null && this.IsActive ()) {
				this.m_Group.RegisterToggle (this);
			}
			if (newGroup != null && newGroup != group && this.isOn && this.IsActive ()) {
				this.m_Group.NotifyToggleOn (this);
			}
		}

		protected override void Start ()
		{
			this.PlayEffect (true);
		}

		//
		// Nested Types
		//
		[System.Serializable]
		public class ToggleEvent : UnityEvent<bool>
		{
		}

		public enum ToggleTransition
		{
			None,
			Fade
		}

		[System.Serializable]
		public struct AnchorsTransitionBlock {

			public Vector4 normal;
			public Vector4 highlihted;
			public Vector4 pressed;
			public Vector4 disabled;
			public float duration;

		};

		[System.Serializable]
		public struct PivotTransitionBlock {

			public Vector2 normal;
			public Vector2 highlihted;
			public Vector2 pressed;
			public Vector2 disabled;
			public float duration;

		};

		public enum ToggleTransitionExtended : byte {

			None = 0x0,
			Fade = 0x1,
			Anchors = 0x2,
			Pivot = 0x4,
			Color = 0x8,
			Alpha = 0x10,

		};

		public AnchorsTransitionBlock anchorsTransitionOn;
		public AnchorsTransitionBlock anchorsTransitionOff;

		public PivotTransitionBlock pivotTransitionOn;
		public PivotTransitionBlock pivotTransitionOff;

		public ColorBlock colorTransitionOn;
		public ColorBlock colorTransitionOff;

		[SerializeField]
		private ButtonExtended.AlphaBlock m_AlphaOn = ButtonExtended.AlphaBlock.defaultAlphaBlock;
		public ButtonExtended.AlphaBlock alphaOn {
			get {
				return this.m_AlphaOn;
			}
			set {
				this.m_AlphaOn = value;
			}
		}

		[SerializeField]
		private ButtonExtended.AlphaBlock m_AlphaOff = ButtonExtended.AlphaBlock.defaultAlphaBlock;
		public ButtonExtended.AlphaBlock alphaOff {
			get {
				return this.m_AlphaOff;
			}
			set {
				this.m_AlphaOff = value;
			}
		}

		[BitMask(typeof(ToggleTransitionExtended))]
		public ToggleTransitionExtended toggleTransitionExtended;

		public void DoAnchorsTransition(Vector4 to, float duration, bool instant) {

			if (instant == true) {

				this.graphic.rectTransform.anchorMin = new Vector2(to.x, to.y);
				this.graphic.rectTransform.anchorMax = new Vector2(to.z, to.w);

			} else {

				var fromMin = this.graphic.rectTransform.anchorMin;
				var fromMax = this.graphic.rectTransform.anchorMax;

				TweenerGlobal.instance.removeTweens(this.graphic);
				TweenerGlobal.instance.addTween(this.graphic.rectTransform, duration, 0f, 1f).tag(this.graphic).onUpdate((obj, value) => {

					if (obj != null) {

						obj.anchorMin = Vector2.Lerp(fromMin, new Vector2(to.x, to.y), value);
						obj.anchorMax = Vector2.Lerp(fromMax, new Vector2(to.z, to.w), value);

					}

				});

			}

		}

		private void StartAlphaTween(bool state, float targetAlpha, bool instant) {

			var alpha = (state == true) ? this.alphaOn : this.alphaOff;
			if (alpha.canvasGroup == null) return;

			if (TweenerGlobal.instance != null) TweenerGlobal.instance.removeTweens(alpha.canvasGroup);

			if (instant == true) {

				alpha.canvasGroup.alpha = targetAlpha;

			} else {

				TweenerGlobal.instance.addTweenAlpha(alpha.canvasGroup, alpha.fadeDuration, targetAlpha).tag(alpha.canvasGroup);

			}

		}

		public void DoPivotTransition(Vector2 to, float duration, bool instant) {

			if (instant == true) {

				this.graphic.rectTransform.pivot = to;

			} else {

				var from = this.graphic.rectTransform.pivot;

				TweenerGlobal.instance.removeTweens(this.graphic);
				TweenerGlobal.instance.addTween(this.graphic.rectTransform, duration, 0f, 1f).tag(this.graphic).onUpdate((obj, value) => {

					if (obj != null) {

						obj.pivot = Vector2.Lerp(from, to, value);

					}

				});

			}

		}

		protected override void InstantClearState() {

			base.InstantClearState();

			if (this.toggleTransitionExtended > ToggleTransitionExtended.Fade) {

				var instant = true;

				float alpha;
				Color color;
				float alphaMultiplier;
				float colorMultiplier;
				this.GetParameters(this.interactable == true ? SelectionState.Normal : SelectionState.Disabled, out alpha, out color, out alphaMultiplier, out colorMultiplier);

				if (this.graphic != null) {

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Anchors) != 0) {

						this.DoAnchorsTransition(this.isOn == true ? this.anchorsTransitionOn.normal : this.anchorsTransitionOff.normal, this.isOn == true ? this.anchorsTransitionOn.duration : this.anchorsTransitionOff.duration, instant);

					}

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Pivot) != 0) {

						this.DoPivotTransition(this.isOn == true ? this.pivotTransitionOn.normal : this.pivotTransitionOff.normal, this.isOn == true ? this.pivotTransitionOn.duration : this.pivotTransitionOff.duration, instant);

					}

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Color) != 0) {

						this.graphic.CrossFadeColor(color * colorMultiplier, this.isOn == true ? this.colorTransitionOn.fadeDuration : this.colorTransitionOff.fadeDuration, ignoreTimeScale: true, useAlpha: true);

					}

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Alpha) != 0) {

						this.StartAlphaTween(this.isOn, alpha * alphaMultiplier, true);

					}

				}

			}

		}

		protected virtual void PlayEffect(bool instant) {

			if (this.toggleTransitionExtended > ToggleTransitionExtended.Fade) {

				float alpha;
				Color color;
				float alphaMultiplier;
				float colorMultiplier;
				this.GetParameters(this.interactable == true ? SelectionState.Normal : SelectionState.Disabled, out alpha, out color, out alphaMultiplier, out colorMultiplier);

				instant = (Application.isPlaying == false || TweenerGlobal.instance == null);

				if (this.graphic != null) {

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Anchors) != 0) {

						this.DoAnchorsTransition(this.isOn == true ? this.anchorsTransitionOn.normal : this.anchorsTransitionOff.normal, this.isOn == true ? this.anchorsTransitionOn.duration : this.anchorsTransitionOff.duration, instant);

					}

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Pivot) != 0) {

						this.DoPivotTransition(this.isOn == true ? this.pivotTransitionOn.normal : this.pivotTransitionOff.normal, this.isOn == true ? this.pivotTransitionOn.duration : this.pivotTransitionOff.duration, instant);

					}

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Color) != 0) {

						this.graphic.CrossFadeColor(color * colorMultiplier, this.isOn == true ? this.colorTransitionOn.fadeDuration : this.colorTransitionOff.fadeDuration, ignoreTimeScale: true, useAlpha: true);

					}

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Alpha) != 0) {

						this.StartAlphaTween(this.isOn, alpha * alphaMultiplier, true);

					}

				}

			} else {

				this.PlayEffectDefault(instant);

			}

		}

		private void GetParameters(Selectable.SelectionState state, out float alpha, out Color color, out float alphaMultiplier, out float colorMultiplier) {
			
			var alphaBlock = (this.isOn == true ? this.alphaOn : this.alphaOff);
			var colorBlock = (this.isOn == true ? this.colorTransitionOn : this.colorTransitionOff);

			alphaMultiplier = alphaBlock.alphaMultiplier;
			colorMultiplier = colorBlock.colorMultiplier;

			switch (state) {

				case Selectable.SelectionState.Normal:
					alpha = alphaBlock.normalAlpha;
					color = colorBlock.normalColor;
					break;
				case Selectable.SelectionState.Highlighted:
					alpha = alphaBlock.highlightedAlpha;
					color = colorBlock.highlightedColor;
					break;
				case Selectable.SelectionState.Pressed:
					alpha = alphaBlock.pressedAlpha;
					color = colorBlock.pressedColor;
					break;
				case Selectable.SelectionState.Disabled:
					alpha = alphaBlock.disabledAlpha;
					color = colorBlock.disabledColor;
					break;
				default:
					alpha = 0f;
					color = Color.black;
					break;

			}

		}

		protected override void DoStateTransition(Selectable.SelectionState state, bool instant) {

			float alpha;
			Color color;
			float alphaMultiplier;
			float colorMultiplier;
			this.GetParameters(state, out alpha, out color, out alphaMultiplier, out colorMultiplier);

			if (this.toggleTransitionExtended > ToggleTransitionExtended.Fade) {

				instant = (Application.isPlaying == false || TweenerGlobal.instance == null);

				if (this.graphic != null) {
					
					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Color) != 0) {

						this.graphic.CrossFadeColor(color * colorMultiplier, this.isOn == true ? this.colorTransitionOn.fadeDuration : this.colorTransitionOff.fadeDuration, ignoreTimeScale: true, useAlpha: true);

					}

					if ((this.toggleTransitionExtended & ToggleTransitionExtended.Alpha) != 0) {

						this.StartAlphaTween(this.isOn, alpha * alphaMultiplier, true);

					}

				}

			} else {

				base.DoStateTransition(state, instant);

			}

		}

		private void PlayEffectDefault(bool instant) {
			if (this.graphic == null) {
				return;
			}
			if (!Application.isPlaying) {
				this.graphic.canvasRenderer.SetAlpha ((!this.m_IsOn) ? 0 : 1);
			} else {
				this.graphic.CrossFadeAlpha ((!this.m_IsOn) ? 0f : 1f, (!instant) ? 0.1f : 0f, true);
			}
		}

	}

}