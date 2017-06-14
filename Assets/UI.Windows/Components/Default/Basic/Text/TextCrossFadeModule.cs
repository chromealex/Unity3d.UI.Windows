using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using UnityEngine.Extensions;
using ME;

namespace UnityEngine.UI.Windows.Components.Modules {

	[System.Serializable]
	public class TextCrossFadeModule : ComponentModuleBase {
		
		public float duration = 0.5f;
		//[ReadOnly("enabled", state: false)]
		public bool fadeIfWasNull = false;
		//[ReadOnly("enabled", state: false)]
		public bool fadeOutSource = false;
		//[ReadOnly("enabled", state: false)]
		public ME.Ease.Type easeType;

		//[ReadOnly("enabled", state: false)]
		public Color startColor = new Color(1f, 1f, 1f, 0f);
		//[ReadOnly("enabled", state: false)]
		[ReadOnlyEndGroup("enabled", state: false)]
		public Color targetColor = new Color(1f, 1f, 1f, 1f);

		private ITextComponent text;
		private Graphic source;

		public override void Init(IComponent source) {

			base.Init(source);

			this.text = source as ITextComponent;

		}

		public override void Prepare(IComponent source) {

			base.Prepare(source);

			this.source = this.text.GetGraphicSource();

			if (this.source.mainTexture == null) {

				this.source.color = this.startColor;

			}

		}

		public void FadeTo(ITextComponent source, string to, System.Action callback = null) {

			this.FadeTo<Text>(source, to, this.duration, this.fadeIfWasNull, callback);

		}

		public void FadeTo(ITextComponent source, string to, float duration, System.Action callback = null) {

			this.FadeTo<Text>(source, to, duration, this.fadeIfWasNull, callback);

		}

		private void FadeTo<T>(ITextComponent textSource, string to, float duration, bool fadeIfWasNull, System.Action callback) where T : Graphic {

			var hasSourceText = false;

			var hasChanged = ((this.source as Text).text != to);
			if (hasChanged == false) {

				if (callback != null) callback.Invoke();
				return;

			}

			hasSourceText = (string.IsNullOrEmpty((this.source as Text).text) == false);
			var copy = this.MakeCopy<T>(this.source.rectTransform);

			var sourceText = this.source as Text;
			var copyText = (copy as Text);

			copyText.material = (this.source.defaultMaterial == this.source.material ? null : this.source.material);
			copyText.text = to;
			copyText.horizontalOverflow = sourceText.horizontalOverflow;//HorizontalWrapMode.Overflow;
			copyText.verticalOverflow = sourceText.verticalOverflow;//VerticalWrapMode.Overflow;
			copyText.supportRichText = sourceText.supportRichText;
			copyText.resizeTextForBestFit = sourceText.resizeTextForBestFit;
			copyText.resizeTextMinSize = sourceText.resizeTextMinSize;
			copyText.resizeTextMaxSize = sourceText.resizeTextMaxSize;
			copyText.lineSpacing = sourceText.lineSpacing;
			copyText.font = sourceText.font;
			copyText.fontSize = sourceText.fontSize;
			copyText.fontStyle = sourceText.fontStyle;
			copyText.alignByGeometry = sourceText.alignByGeometry;
			copyText.alignment = sourceText.alignment;
			
			TweenerGlobal.instance.removeTweens(source, immediately: true);
			TweenerGlobal.instance.removeTweens(copy, immediately: true);

			var sourceColor = this.targetColor;

			if (hasSourceText == true || fadeIfWasNull == true) {

				copy.color = this.startColor;//new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0f);

				if (hasSourceText == false) {

					source.color = this.startColor;//new Color(sourceColor.r, sourceColor.g, sourceColor.b, 0f);

				}

				if (this.fadeOutSource == true) TweenerGlobal.instance.addTweenAlpha(source, duration, 0f).tag(source).ease(ME.Ease.GetByType(this.easeType));
				TweenerGlobal.instance.addTweenAlpha(copy, this.duration, 1f).tag(copy).ease(ME.Ease.GetByType(this.easeType)).onComplete(() => {

					if (source != null) source.color = sourceColor;
					this.Finalize(source, copy);

					if (callback != null) callback.Invoke();

				}).onCancel(() => {

					if (callback != null) callback.Invoke();

				});

			} else {

				if (copy != null) copy.color = sourceColor;
				if (source != null) source.color = sourceColor;
				this.Finalize(source, copy);

				if (callback != null) callback.Invoke();

			}

		}

		private void Finalize(Graphic source, Graphic copy) {

			var sourceImage = source as Text;
			if (sourceImage != null) {

				sourceImage.text = (copy as Text).text;

			}

			if (copy != null) {

				GameObject.Destroy(copy.gameObject);

			}

		}

		protected override T MakeCopy<T>(RectTransform transform) {

			var component = base.MakeCopy<T>(transform);
			return component;

		}

	}

}