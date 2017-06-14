using UnityEngine;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;
using System.Linq;

namespace UnityEngine.UI.Windows.Audio {

	[System.Serializable]
	public class Data {
		
		[System.Serializable]
		public class State {
			
			public int key;
			public string category;
			public AudioClip clip;
			
		}
		
		[SerializeField]
		private List<State> music = new List<State>();
		[SerializeField]
		private List<State> fx = new List<State>();
		
		private Cache<int, State> musicCache = new Cache<int, State>();
		private Cache<int, State> fxCache = new Cache<int, State>();

		public void Setup() {

			var k = 0;
			for (int i = 0; i < this.music.Count; ++i) {
				
				this.music[i].key = ++k;
				
			}

			k = 0;
			for (int i = 0; i < this.fx.Count; ++i) {
				
				this.fx[i].key = ++k;
				
			}

		}

		public void SetupCache() {
			
			this.musicCache.Fill(this.music, (item, index) => item.key, (item, index) => item);
			this.fxCache.Fill(this.fx, (item, index) => item.key, (item, index) => item);
			
		}

		public List<State> GetStates(ClipType clipType) {

			if (clipType == ClipType.Music) {

				return this.music;

			} else if (clipType == ClipType.SFX) {

				return this.fx;

			}

			return null;

		}

		public State GetState(ClipType clipType, int key) {

			#if UNITY_EDITOR
			if (Application.isPlaying == false) {
				
				if (clipType == ClipType.Music) {
					
					return this.music.FirstOrDefault(item => item.key == key);
					
				} else if (clipType == ClipType.SFX) {
					
					return this.fx.FirstOrDefault(item => item.key == key);
					
				}

			}
			#endif

			if (clipType == ClipType.Music) {
				
				return this.musicCache.GetValue(key);
				
			} else if (clipType == ClipType.SFX) {
				
				return this.fxCache.GetValue(key);
				
			}

			return null;
			
		}

		public bool ContainsKey(ClipType clipType, int key) {
			
			if (clipType == ClipType.Music) {
				
				return this.musicCache.ContainsKey(key);
				
			} else if (clipType == ClipType.SFX) {
				
				return this.fxCache.ContainsKey(key);
				
			}
			
			return false;

		}
		
		public void Add(ClipType clipType, Data other, bool overrideValues = false) {

			List<State> source = null;
			List<State> target = null;
			
			if (clipType == ClipType.Music) {
				
				source = other.music;
				target = this.music;
				
			} else if (clipType == ClipType.SFX) {
				
				source = other.fx;
				target = this.fx;

			}

			if (source == null || target == null) return;

			for (int i = 0; i < source.Count; ++i) {
				
				if (this.ContainsKey(clipType, source[i].key) == false) {
					
					target.Add(source[i]);
					
				} else if (overrideValues == true) {
					
					target.Remove(this.GetState(clipType, source[i].key));
					target.Add(source[i]);
					
				}
				
			}
			
			this.SetupCache();
			
		}
		
		public void Add(ClipType clipType, State state) {
			
			if (clipType == ClipType.Music) {

				this.music.Add(state);

			} else if (clipType == ClipType.SFX) {

				this.fx.Add(state);

			}

			this.SetupCache();
			
		}
		
		public void Remove(ClipType clipType, int key) {
			
			if (clipType == ClipType.Music) {
				
				this.music.RemoveAll((item) => item.key == key);

			} else if (clipType == ClipType.SFX) {
				
				this.fx.RemoveAll((item) => item.key == key);

			}

			this.SetupCache();
			
		}

	}

}