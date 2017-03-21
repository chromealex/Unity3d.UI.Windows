#if !UNITY_5_5_OR_NEWER
#define PARTICLESYSTEM_LEGACY
#endif
using UnityEngine;

namespace ME {

	public class ParticleSystemCachedItem : MonoBehaviour {

		public enum MinMaxState {

			MaxColor = 0,
			MaxGradient = 1,
			MinMaxColor = 2,
			MinMaxGradient = 3,

		};

		[System.Serializable]
		public struct StartColor {

			public Color minColor;
			public Color maxColor;
			public Gradient minGradient;
			public Gradient maxGradient;

			public MinMaxState minMaxState;

		};

		new public ParticleSystem particleSystem;
		public StartColor startColor;
		public Renderer particleSystemRenderer;

		//private float lastAlpha = -1f;

		public void SetLayer(string layerName, int order) {

			if (this.particleSystemRenderer == null) return;

			this.particleSystemRenderer.sortingLayerName = layerName;
			this.particleSystemRenderer.sortingOrder = order;

		}

		#if UNITY_EDITOR
		/*public void OnDisabled() {

			if (this.particleSystem != null) {

				this.particleSystem.startColor = this.startColor;

			}

		}*/

		[ContextMenu("Setup")]
		public void Setup() {

			this.particleSystem = this.GetComponent<ParticleSystem>();
			if (this.particleSystem != null) {

				//var alpha = this.lastAlpha;
				//this.SetStartAlpha(1f);

				var obj = new UnityEditor.SerializedObject(this.particleSystem);
				var module = obj.FindProperty("InitialModule").FindPropertyRelative("startColor");

				var minColor = module.FindPropertyRelative("minColor");
				var maxColor = module.FindPropertyRelative("maxColor");
				var minGradient = module.FindPropertyRelative("minGradient");
				var maxGradient = module.FindPropertyRelative("maxGradient");
				var minMaxState = module.FindPropertyRelative("minMaxState");
				this.startColor.minColor = minColor.colorValue;
				this.startColor.maxColor = maxColor.colorValue;
				this.startColor.minGradient = minGradient.GetGradient();
				this.startColor.maxGradient = maxGradient.GetGradient();
				this.startColor.minMaxState = (MinMaxState)minMaxState.intValue;

				this.particleSystemRenderer = this.particleSystem.GetComponent<Renderer>();

				//if (alpha >= 0f) this.SetStartAlpha(alpha);

			}

		}

		public void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			this.Setup();

		}
		#endif

		#if PARTICLESYSTEM_LEGACY
		private System.Type startColorType;
		private void SetColor_REFLECTION(ParticleSystem ps, string propertyName, object color) {

			if (this.startColorType == null) {

				var field = ps.GetType().GetProperty("InitialModule",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.Public |
					System.Reflection.BindingFlags.NonPublic |
					System.Reflection.BindingFlags.GetProperty);
				
				if (field == null) return;

				var module = field.GetValue(null, null);
				var startColor = module.GetType().GetField("startColor").GetValue(null);
				this.startColorType = startColor.GetType();

			}

			this.startColorType.GetField(propertyName).SetValue(null, color);

		}
		#endif

		private Color ApplyColor(ref Color baseColor, Color color) {

            baseColor = new Color(color.r, color.g, color.b, baseColor.a);
            return new Color(color.r, color.g, color.b, color.a * baseColor.a);

		}

		public void SetStartAlpha(float alpha) {

			//this.lastAlpha = alpha;

			if (this.startColor.minMaxState == MinMaxState.MaxColor) {

				var color = this.startColor.maxColor;
				color.a = alpha;
				this.SetStartColor(color);

			} else if (this.startColor.minMaxState == MinMaxState.MinMaxColor) {

				var minColor = this.startColor.minColor;
				minColor.a = alpha;
				var maxColor = this.startColor.maxColor;
				maxColor.a = alpha;
				this.SetStartColor(minColor, maxColor);

			}

		}

		public void SetStartColor(Color color) {

			if (Application.isPlaying == false) return;

			if (this.startColor.minMaxState == MinMaxState.MaxColor) {

				var _color = this.ApplyColor(ref this.startColor.maxColor, color);
				#if PARTICLESYSTEM_LEGACY
                this.particleSystem.startColor = _color;
				#else
				var main = this.particleSystem.main;
				main.startColor = new ParticleSystem.MinMaxGradient(_color);
				#endif
				this.SetColor_PARTICLES(_color);

			}

		}

		public void SetStartColor(Color minColor, Color maxColor) {

			if (Application.isPlaying == false) return;

			if (this.startColor.minMaxState == MinMaxState.MinMaxColor) {

				var _minColor = this.ApplyColor(ref this.startColor.minColor, minColor);
				var _maxColor = this.ApplyColor(ref this.startColor.maxColor, maxColor);

				#if PARTICLESYSTEM_LEGACY
				this.SetColor_REFLECTION(this.particleSystem, "minColor", _minColor);
				this.particleSystem.startColor = _maxColor;
				this.SetColor_PARTICLES(Color.Lerp(_minColor, _maxColor, 0.5f));
				#else
				var main = this.particleSystem.main;
				main.startColor = new ParticleSystem.MinMaxGradient(_minColor, _maxColor);
				this.SetColor_PARTICLES(Color.Lerp(_minColor, _maxColor, 0.5f));
				#endif

			}

		}

		public void SetStartColor(Gradient gradient) {

			if (Application.isPlaying == false) return;

			if (this.startColor.minMaxState == MinMaxState.MaxGradient) {

				#if PARTICLESYSTEM_LEGACY
				this.SetColor_REFLECTION(this.particleSystem, "maxGradient", gradient);
				this.SetColor_PARTICLES(gradient.Evaluate(0f));
				#else
				var main = this.particleSystem.main;
				main.startColor = new ParticleSystem.MinMaxGradient(gradient);
				this.SetColor_PARTICLES(gradient.Evaluate(0f));
				#endif

			}

		}

		public void SetStartColor(Gradient minGradient, Gradient maxGradient) {

			if (Application.isPlaying == false) return;

			if (this.startColor.minMaxState == MinMaxState.MinMaxGradient) {

				#if PARTICLESYSTEM_LEGACY
				this.SetColor_REFLECTION(this.particleSystem, "minGradient", minGradient);
				this.SetColor_REFLECTION(this.particleSystem, "maxGradient", maxGradient);
				this.SetColor_PARTICLES(maxGradient.Evaluate(0f));
				#else
				var main = this.particleSystem.main;
				main.startColor = new ParticleSystem.MinMaxGradient(minGradient, maxGradient);
				this.SetColor_PARTICLES(maxGradient.Evaluate(0f));
				#endif

			}

		}

		public void SetTime(float time) {

			this.particleSystem.time = time;

		}

		public void Rewind(float time, bool noRestart) {

			var seed = this.particleSystem.randomSeed;
			this.particleSystem.randomSeed = 1;

			if (noRestart == false) {

				this.particleSystem.Simulate(time, withChildren: false, restart: true);

			}

			this.particleSystem.Simulate(time, withChildren: false, restart: false);

			this.particleSystem.randomSeed = seed;

		}

		public void Play() {

			this.particleSystem.Play(withChildren: false);

		}

		public void Stop(bool reset) {

			if (reset == true) {
				
				this.particleSystem.Clear();

			}

			this.particleSystem.Stop();

		}

	    public void Pause() {
	        
            this.particleSystem.Pause(withChildren: true);

	    }

        private ParticleSystem.Particle[] particles;

	    private void InitParticles() {

	        if (this.particleSystem == null || this.particles != null) return;

			#if PARTICLESYSTEM_LEGACY
			this.particles = new ParticleSystem.Particle[this.particleSystem.maxParticles];
			#else
			this.particles = new ParticleSystem.Particle[this.particleSystem.main.maxParticles];
			#endif

	    }

	    public void SetAlpha(float alpha) {

	        var a = (byte)(alpha * 255);

            var count = this.particleSystem.GetParticles(this.particles);
            for (var p = 0; p < count; ++p) {

                var particle = particles[p];
                var color = particle.GetCurrentColor(this.particleSystem);
                color.a = a;
                particle.startColor = color;

            }

            this.particleSystem.SetParticles(particles, count);

        }

        public void Stop(float time) {

            this.InitParticles();
            
            var count = this.particleSystem.GetParticles(this.particles);
			for (int p = 0; p < count; ++p) {

				#if PARTICLESYSTEM_LEGACY
				particles[p].lifetime = Random.Range(0f, time);
				#else
				particles[p].remainingLifetime = Random.Range(0f, time);
				#endif

			}

            this.particleSystem.SetParticles(particles, count);
			this.particleSystem.Stop();

		}

		private void SetColor_PARTICLES(Color color) {

            this.InitParticles();
            
            var count = this.particleSystem.GetParticles(this.particles);
			for (int p = 0; p < count; ++p) {

				particles[p].startColor = color;

			}

			this.particleSystem.SetParticles(particles, count);

		}

	}

}