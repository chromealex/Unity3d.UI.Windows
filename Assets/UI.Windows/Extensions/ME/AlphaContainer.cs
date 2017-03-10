using UnityEngine;
using System.Collections;

namespace ME {

	[ExecuteInEditMode]
	public class AlphaContainer : MonoBehaviour {

		[System.Serializable]
		public struct SpriteRendererItem {

			public SpriteRenderer renderer;
			public Color sourceColor;

			public void SetAlpha(float value) {

				var color = this.sourceColor;
				color.a *= value;
				this.renderer.color = color;

			}

		};

		[System.Serializable]
		public struct ParticleSystemItem {

			public ParticleSystemCached renderer;
			public float sourceAlpha;

			public void SetAlpha(float value) {

				var alpha = this.sourceAlpha;
				alpha *= value;
				this.renderer.alpha = alpha;

			}

		};

		public SpriteRendererItem[] sprites;
		public ParticleSystemItem[] particleSystems;

		[SerializeField][Range(0f, 1f)]
		private float _alpha = 1f;

		[Header("Optional")]
		public CanvasGroup canvasGroup;

		private float tempLastAlpha = -1f;

		public void LateUpdate() {

			this.ValidateCanvasGroup();

			var canvasAlpha = 1f;

			if (this.canvasGroup != null) canvasAlpha = this.canvasGroup.alpha;

			var alpha = this._alpha * canvasAlpha;
			if (alpha != this.tempLastAlpha) {

				this.tempLastAlpha = alpha;

				this.SetAlpha(alpha);

			}

		}

		public void SetCanvasGroup(CanvasGroup canvasGroup) {

			this.canvasGroup = canvasGroup;

		}

		public void ValidateCanvasGroup() {

			if (this.canvasGroup == null) {

				this.canvasGroup = ME.Utilities.FindReferenceParent<CanvasGroup>(this);

			}

		}

		public void SetAlpha(float value) {

			for (int i = 0; i < this.sprites.Length; ++i) {

				this.sprites[i].SetAlpha(value);

			}

			for (int i = 0; i < this.particleSystems.Length; ++i) {

				this.particleSystems[i].SetAlpha(value);

			}

		}

		/*public void OnValidate() {

			if (Application.isPlaying == true) return;

			this.Setup();

		}*/

		[ContextMenu("Setup Reverse")]
		public void SetupReverse() {

			for (int i = 0; i < this.sprites.Length; ++i) {

				this.sprites[i].renderer.color = this.sprites[i].sourceColor;

			}

		}

		[ContextMenu("Setup")]
		public void Setup() {

			var sprites = this.GetComponentsInChildren<SpriteRenderer>(true);
			this.sprites = new SpriteRendererItem[sprites.Length];
			for (int i = 0; i < this.sprites.Length; ++i) {

				this.sprites[i].renderer = sprites[i];
				this.sprites[i].sourceColor = sprites[i].color;

			}

			var particles = this.GetComponentsInChildren<ParticleSystemCached>(true);
			this.particleSystems = new ParticleSystemItem[particles.Length];
			for (int i = 0; i < this.particleSystems.Length; ++i) {

				this.particleSystems[i].renderer = particles[i];
				this.particleSystems[i].sourceAlpha = particles[i].alpha;

			}

		}

	}

}