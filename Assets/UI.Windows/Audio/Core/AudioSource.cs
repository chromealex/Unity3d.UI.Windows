using UnityEngine;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows.Audio {

	[System.Serializable]
	public class Source {
		
		public AudioSource source;
		
		private Dictionary<long, AudioSource> instances = new Dictionary<long, AudioSource>();
		private Dictionary<ClipType, List<AudioSource>> instancesByType = new Dictionary<ClipType, List<AudioSource>>();
		
		private float musicVolume = 0f;
		private float sfxVolume = 0f;

		public void Init() {

			this.source.CreatePool(10);

		}

		public float GetVolume(ClipType clipType) {

			var volume = 0f;

			if (clipType == ClipType.Music) {
				
				volume = this.musicVolume;

			} else if (clipType == ClipType.SFX) {
				
				volume = this.sfxVolume;
				
			}

			return volume;

		}

		public void SetVolume(ClipType clipType, float value) {

			if (clipType == ClipType.Music) {

				this.musicVolume = value;

			} else if (clipType == ClipType.SFX) {

				this.sfxVolume = value;

			}
			
			var sources = this.GetSources(null, clipType);
			if (sources != null) {

				foreach (var source in sources) {

					this.ApplyVolume(clipType, source);

				}

			}

		}

		public void ApplyVolume(ClipType clipType, AudioSource source) {

			if (source != null) source.volume = this.GetVolume(clipType);

		}

		public AudioSource GetSource(WindowBase window, ClipType clipType, int id) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) {
				
				return null;
				
			}
			#endif
			
			var key = (window != null) ? (long)window.GetInstanceID() : (long)(((int)clipType << 16) | (id & 0xffff));
			
			AudioSource value;
			if (this.instances.TryGetValue(key, out value) == false) {
				
				value = this.source.Spawn();
				this.instances.Add(key, value);
				
			}

			List<AudioSource> valuesByType;
			if (this.instancesByType.TryGetValue(clipType, out valuesByType) == false) {

				this.instancesByType.Add(clipType, new List<AudioSource>() { value });

			} else {

				valuesByType.Add(value);

			}
			
			this.ApplyVolume(clipType, value);

			return value;
			
		}
		
		public List<AudioSource> GetSources(WindowBase window, ClipType clipType) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) {
				
				return null;
				
			}
			#endif

			List<AudioSource> valuesByType;
			if (this.instancesByType.TryGetValue(clipType, out valuesByType) == false) {
				
				return null;
				
			} else {
				
				return valuesByType;
				
			}

		}

	}

}