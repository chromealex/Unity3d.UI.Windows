using UnityEngine;
using UnityEngine.EventSystems;

namespace ME {

	[RequireComponent(typeof(CanvasRenderer))]
	[ExecuteInEditMode()]
	public class ParticleSystemCached : UIBehaviour {

		[Header("Base")]
		public ParticleSystem mainParticleSystem;
		public ParticleSystemCachedItem mainParticleSystemItem;
		public ParticleSystem[] particleSystems;
		public ParticleSystemCachedItem[] particleSystemItems;
		public int count;
		public bool resetOnStop;

		[SerializeField][Range(0f, 1f)]
		private float _alpha = 1f;

		[Header("Optional")]
		public CanvasGroup canvasGroup;

		private float tempLastAlpha = -1f;

		public void SetCanvasGroup(CanvasGroup canvasGroup) {

			this.canvasGroup = canvasGroup;

		}

		public void ValidateCanvasGroup() {

			if (this.canvasGroup == null) {

				this.canvasGroup = ME.Utilities.FindReferenceParent<CanvasGroup>(this);

			}

		}

		#if UNITY_EDITOR
		[ContextMenu("Setup")]
		public void Setup() {

			this.particleSystems = this.GetComponentsInChildren<ParticleSystem>(true);
			this.mainParticleSystem = (this.particleSystems.Length > 0) ? this.particleSystems[0] : null;
			this.count = this.particleSystems.Length;

			this.ValidateCanvasGroup();

			this.particleSystemItems = new ParticleSystemCachedItem[this.particleSystems.Length];
			for (int i = 0; i < this.particleSystems.Length; ++i) {
				
				var ps = this.particleSystems[i];

				var item = ps.gameObject.GetComponent<ParticleSystemCachedItem>();
				if (item == null) item = ps.gameObject.AddComponent<ParticleSystemCachedItem>();

				item.Setup();
				this.particleSystemItems[i] = item;

			}

			var index = System.Array.IndexOf(this.particleSystems, this.mainParticleSystem);
			if (index >= 0) this.mainParticleSystemItem = this.particleSystemItems[index];

		}

		protected override void OnValidate() {

			if (Application.isPlaying == true) return;

			this.Setup();
			this.LateUpdate();

		}
		#endif

		public Color32 startColor {

			set {

				this.SetStartColor(value, withChildren: true);

			}

			get {

				return this.GetStartColor();

			}

		}

		public float alpha {

			set {

				this.SetStartAlpha(value, withChildren: true);

			}

			get {

				return this.GetStartAlpha();

			}

		}

		public bool loop {
			
			set {
				
				if (this.mainParticleSystem != null) this.mainParticleSystem.loop = value;
				
			}
			
			get {
				
				return (this.mainParticleSystem != null) ? this.mainParticleSystem.loop : false;
				
			}
			
		}

		public float duration {

			get {

				return (this.mainParticleSystem != null) ? this.mainParticleSystem.duration : 0f;

			}

		}

		public float GetStartAlpha() {

			var color = this.mainParticleSystem.startColor;
			return color.a;

		}

		public void SetStartAlpha(float alpha, bool withChildren) {

			if (withChildren == true) {

				for (int i = 0; i < this.count; ++i) {

					this.particleSystemItems[i].SetStartAlpha(alpha);

				}

			} else if(this.mainParticleSystemItem != null) {

				this.mainParticleSystemItem.SetStartAlpha(alpha);

			}

		}

		public Color GetStartColor() {

			return this.mainParticleSystem.startColor;

		}

		public void SetStartColor(Color color, bool withChildren) {

			if (withChildren == true) {

				for (int i = 0; i < this.count; ++i) {

					this.particleSystemItems[i].SetStartColor(color);

				}

			} else if (this.mainParticleSystemItem != null) {

                this.mainParticleSystemItem.SetStartColor(color);

			}

		}

	    public void SetTime(float time, bool withChildren) {

			if (withChildren == true) {

				for (int i = 0; i < this.count; ++i) {

					this.particleSystemItems[i].SetTime(time);

				}

			} else if (this.mainParticleSystemItem != null) {

                this.mainParticleSystemItem.SetTime(time);
				
			}

	    }

	    public void Rewind(float time, bool withChildren, bool noRestart) {
	        
	        if (withChildren == true) {
				
				for (int i = 0; i < this.count; ++i) {

					this.particleSystemItems[i].Rewind(time, noRestart);

				}

			} else if (this.mainParticleSystemItem != null) {

                this.mainParticleSystemItem.Rewind(time, noRestart);

			}

		}

		public void Play() {

			if (this.mainParticleSystem != null) this.mainParticleSystem.Play();

		}

		public void Play(bool withChildren) {
			
			if (withChildren == true) {

				for (int i = 0; i < this.count; ++i) {

					this.particleSystemItems[i].Play();

				}

			} else if (this.mainParticleSystemItem != null) {

                this.mainParticleSystemItem.Play();

			}

		}

		public void Stop() {

			if (this.resetOnStop == true) {

				if (this.mainParticleSystem != null) {

					this.mainParticleSystem.Clear();
					this.mainParticleSystem.Stop();

				}

			} else {

				if (this.mainParticleSystem != null) {

					this.mainParticleSystem.Stop();

				}

			}

		}

		public void Stop(bool withChildren) {

			this.Stop(withChildren, this.resetOnStop);

		}

	    public void Stop(bool withChildren, bool resetOnStop) {

	        if (withChildren == true) {

	            for (int i = 0; i < this.count; ++i) {

					this.particleSystemItems[i].Stop(reset: resetOnStop);

	            }

	        } else if (this.mainParticleSystemItem != null) {

                this.mainParticleSystemItem.Stop(reset: resetOnStop);

	        }
	        
	    }

		public void Stop(bool withChildren, float time) {

			if (withChildren == true) {

				for (int i = 0; i < this.count; ++i) {

					this.particleSystemItems[i].Stop(time);

				}
			
			} else if (this.mainParticleSystemItem != null) {

                this.mainParticleSystemItem.Stop(time);

			}

		}

		public void LateUpdate() {

			this.ValidateCanvasGroup();

			var canvasAlpha = 1f;

			if (this.canvasGroup != null) canvasAlpha = this.canvasGroup.alpha;

			var alpha = this._alpha * canvasAlpha;
			if (alpha != this.tempLastAlpha) {

				this.tempLastAlpha = alpha;

				this.SetStartAlpha(alpha, withChildren: true);

			}

		}
		
	}

}