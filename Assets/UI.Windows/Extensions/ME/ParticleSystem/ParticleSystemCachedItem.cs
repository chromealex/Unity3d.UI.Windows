using UnityEngine;
using System.Collections;

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

		//private float lastAlpha = -1f;

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

				//if (alpha >= 0f) this.SetStartAlpha(alpha);

			}

		}

		public void OnValidate() {

			if (Application.isPlaying == true) return;

			this.Setup();

		}
		#endif

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
                this.particleSystem.startColor = _color;
				this.SetColor_PARTICLES(_color);

			}

		}

		public void SetStartColor(Color minColor, Color maxColor) {

			if (Application.isPlaying == false) return;

			if (this.startColor.minMaxState == MinMaxState.MinMaxColor) {

				var _minColor = this.ApplyColor(ref this.startColor.minColor, minColor);
				var _maxColor = this.ApplyColor(ref this.startColor.maxColor, maxColor);

				this.SetColor_REFLECTION(this.particleSystem, "minColor", _minColor);
				this.particleSystem.startColor = _maxColor;
				this.SetColor_PARTICLES(Color.Lerp(_minColor, _maxColor, 0.5f));

			}

		}

		public void SetStartColor(Gradient gradient) {

			if (Application.isPlaying == false) return;

			if (this.startColor.minMaxState == MinMaxState.MaxGradient) {

				this.SetColor_REFLECTION(this.particleSystem, "maxGradient", gradient);
				this.SetColor_PARTICLES(gradient.Evaluate(0f));

			}

		}

		public void SetStartColor(Gradient minGradient, Gradient maxGradient) {

			if (Application.isPlaying == false) return;

			if (this.startColor.minMaxState == MinMaxState.MinMaxGradient) {

				this.SetColor_REFLECTION(this.particleSystem, "minGradient", minGradient);
				this.SetColor_REFLECTION(this.particleSystem, "maxGradient", maxGradient);
				this.SetColor_PARTICLES(maxGradient.Evaluate(0f));

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

		public void Stop(float time) {
			
			var particles = new ParticleSystem.Particle[this.particleSystem.particleCount];
			var count = this.particleSystem.GetParticles(particles);
			for (int p = 0; p < count; ++p) {

				particles[p].lifetime = Random.Range(0f, time);

			}
			this.particleSystem.SetParticles(particles, count);
			this.particleSystem.Stop();

		}

		private void SetColor_PARTICLES(Color color) {

			var particles = new ParticleSystem.Particle[this.particleSystem.particleCount];
			var count = this.particleSystem.GetParticles(particles);
			for (int p = 0; p < count; ++p) {

				particles[p].startColor = color;

			}
			this.particleSystem.SetParticles(particles, count);

		}

	}

}