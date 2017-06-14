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

		private int currentMusicId;

		private bool mute = false;

		public void Init() {

			this.source.CreatePool(2);

		}

		public void Mute() {

			this.mute = true;

			foreach (var item in this.instances) {

				item.Value.mute = true;

			}

		}

		public void Unmute() {

			this.mute = false;

			foreach (var item in this.instances) {

				item.Value.mute = false;

			}

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
			
			var sources = this.GetPlayingSources(null, clipType);
			if (sources != null) {

				foreach (var source in sources) {

					this.ApplyVolume(clipType, source);

				}

			}

		}

		public void ApplyVolume(ClipType clipType, AudioSource source) {

			if (source != null) source.volume = this.GetVolume(clipType);

		}

		public int GetCurrentMusicId() {

			return this.currentMusicId;

		}

		public long GetKey(WindowBase window, ClipType clipType, int id) {
			
			var key = (long)(((int)clipType << 16) | (id & 0xffff));

			return key;

		}

		public AudioSource GetSource(WindowBase window, ClipType clipType, int id) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) {
				
				return null;
				
			}
			#endif

			if (clipType == ClipType.Music) {
				
				this.currentMusicId = id;
				
			}

			var key = this.GetKey(window, clipType, id);

			AudioSource value;
			if (this.instances.TryGetValue(key, out value) == false) {
				
				value = this.source.Spawn();
				#if UNITY_EDITOR
				value.gameObject.name = string.Format("[ AudioSource ] {0} {1} {2}", (window != null ? window.name : string.Empty), clipType, id);
				#endif
				value.mute = this.mute;
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

		public AudioSource GetPlayingSource(WindowBase window, ClipType clipType, int id) {
			
			#if UNITY_EDITOR
			if (Application.isPlaying == false) {
				
				return null;
				
			}
			#endif

			var key = this.GetKey(window, clipType, id);

			AudioSource value;
			if (this.instances.TryGetValue(key, out value) == false) {
				
				return null;
				
			} else {
				
				return value;
				
			}

		}

		public List<AudioSource> GetPlayingSources(WindowBase window, ClipType clipType) {
			
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