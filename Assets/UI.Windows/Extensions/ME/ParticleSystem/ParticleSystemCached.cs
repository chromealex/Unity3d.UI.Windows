#if !UNITY_5_5_OR_NEWER
#define PARTICLESYSTEM_LEGACY
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows;

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

		public WindowComponent windowObject;
		public int orderDelta = 0;

		[SerializeField][Range(0f, 1f)]
		private float _alpha = 1f;

		[Header("Optional")]
		public CanvasGroup canvasGroup;

		private float tempLastAlpha = -1f;

		protected override void Start() {

			base.Start();

			if (Application.isPlaying == false) return;

			this.Init();

		}

		public void SetCanvasGroup(CanvasGroup canvasGroup) {

			this.canvasGroup = canvasGroup;

		}

		public void ValidateCanvasGroup() {

			if (this.canvasGroup == null) {

				this.canvasGroup = ME.Utilities.FindReferenceParent<CanvasGroup>(this);

			}

		}

		public void Init() {

			if (this.windowObject != null) {

				var window = this.windowObject.GetWindow();
				if (window == null) return;

				window.events.onEveryInstance.Unregister(WindowEventType.OnShowBegin, this.UpdateLayer);
				window.events.onEveryInstance.Register(WindowEventType.OnShowBegin, this.UpdateLayer);

				this.UpdateLayer();

			}

		}

		public void UpdateLayer() {

			if (this.windowObject != null) {

				var window = this.windowObject.GetWindow();
				if (window == null) return;

				for (int i = 0; i < this.particleSystemItems.Length; ++i) {

					this.particleSystemItems[i].SetLayer(window.GetSortingLayerName(), window.GetSortingOrder() + this.orderDelta);

				}

			}

		}

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

		public ParticleSystemSimulationSpace simulationSpace {

			#if PARTICLESYSTEM_LEGACY
			set {
				
				if (this.mainParticleSystem != null) {
					
					this.mainParticleSystem.simulationSpace = value;
					
				}
				
			}

			get {
				
				return (this.mainParticleSystem != null) ? this.mainParticleSystem.simulationSpace : ParticleSystemSimulationSpace.Local;
				
			}
			#else
			set {

				if (this.mainParticleSystem != null) {

					var main = this.mainParticleSystem.main;
					main.simulationSpace = value;

				}

			}

			get {

				return (this.mainParticleSystem != null) ? this.mainParticleSystem.main.simulationSpace : ParticleSystemSimulationSpace.Custom;

			}
			#endif

		}

		public bool loop {

			#if PARTICLESYSTEM_LEGACY
			set {
				
				if (this.mainParticleSystem != null) this.mainParticleSystem.loop = value;
				
			}
			
			get {
				
				return (this.mainParticleSystem != null) ? this.mainParticleSystem.loop : false;
				
			}
			#else
			set {

				if (this.mainParticleSystem != null) {

					var main = this.mainParticleSystem.main;
					main.loop = value;

				}

			}

			get {

				return (this.mainParticleSystem != null) ? this.mainParticleSystem.main.loop : false;

			}
			#endif
			
		}

	    public float startLifeTime {

	        get {

				#if PARTICLESYSTEM_LEGACY
				return (this.mainParticleSystem != null) ? this.mainParticleSystem.startLifetime : 0f;
				#else
				return (this.mainParticleSystem != null) ? this.mainParticleSystem.main.startLifetime.constant : 0f;
				#endif

            }

	    }

		public float duration {

			get {

				#if PARTICLESYSTEM_LEGACY
				return (this.mainParticleSystem != null) ? this.mainParticleSystem.duration : 0f;
				#else
				return (this.mainParticleSystem != null) ? this.mainParticleSystem.main.duration : 0f;
				#endif

			}

		}

		public float GetStartAlpha() {
			
			return this.GetStartColor().a;

		}

		public Color GetStartColor() {

			#if PARTICLESYSTEM_LEGACY
			var color = this.mainParticleSystem.startColor;
			#else
			var color = this.mainParticleSystem.main.startColor.color;
			#endif
			return color;

		}

	    public void SetAlpha(float alpha, bool withChildren) {

            if (withChildren == true) {

                for (var i = 0; i < this.count; ++i) {

                    this.particleSystemItems[i].SetAlpha(alpha);

                }

            } else if (this.mainParticleSystemItem != null) {

                this.mainParticleSystemItem.SetAlpha(alpha);

            }

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

	    public void Pause(bool withChildren) {

            if (withChildren == true) {

                for (int i = 0; i < this.count; ++i) {

                    this.particleSystemItems[i].Pause();

                }

            } else if (this.mainParticleSystemItem != null) {

                this.mainParticleSystemItem.Pause();

            }

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
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			this.Setup();
			this.LateUpdate();
			
		}
		#endif

	}

}