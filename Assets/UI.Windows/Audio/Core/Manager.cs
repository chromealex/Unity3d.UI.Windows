using UnityEngine;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows.Audio {

	public static class Manager {

		private static Data currentData = new Data();

		public static void InitAndAdd(Data data, bool overrideValues = false) {

			data.SetupCache();
			
			Manager.currentData.Add(ClipType.Music, data, overrideValues);
			Manager.currentData.Add(ClipType.SFX, data, overrideValues);

		}

		public static void Change(WindowBase window, Source sourceInfo, ClipType clipType, int id, Audio.Window audioSettings) {

			var source = sourceInfo.GetSource(window, clipType, id);
			if (source == null) return;
			
			source.bypassEffects = audioSettings.bypassEffect;
			source.bypassListenerEffects = audioSettings.bypassListenerEffect;
			source.bypassReverbZones = audioSettings.bypassReverbEffect;
			source.loop = audioSettings.loop;
			
			source.priority = audioSettings.priority;
			source.volume = audioSettings.volume * sourceInfo.GetVolume(clipType);
			source.pitch = audioSettings.pitch;
			source.panStereo = audioSettings.panStereo;
			source.spatialBlend = audioSettings.spatialBlend;
			source.reverbZoneMix = audioSettings.reverbZoneMix;

		}

		public static void Reset(AudioSource source) {

			source.clip = null;

			source.bypassEffects = false;
			source.bypassListenerEffects = false;
			source.bypassReverbZones = false;
			source.loop = true;
			
			source.priority = 128;
			source.volume = 1f;
			source.pitch = 1f;
			source.panStereo = 0f;
			source.spatialBlend = 0f;
			source.reverbZoneMix = 1f;

		}

		public static void Play(WindowBase window, Source sourceInfo, ClipType clipType, int id, bool replaceOnEquals) {

			if (clipType == ClipType.Music) {

				var currentMusicId = sourceInfo.GetCurrentMusicId();
				if (currentMusicId > 0) {

					var equals = (currentMusicId == id);
					if (equals == false || replaceOnEquals == true) {

						// Stop
						Manager.Stop(window, sourceInfo, clipType, currentMusicId);

					} else if (equals == true) {

						// Don't play anything
						return;

					}

				}

			}

			var source = sourceInfo.GetSource(window, clipType, id);
			if (source == null) return;

			if (id == 0) {

				// Stop
				Manager.Stop(window, sourceInfo, clipType, id);
				return;

			}

			var state = Manager.currentData.GetState(clipType, id);
			if (state == null) {
				
				Manager.Stop(window, sourceInfo, clipType, id);
				return;

			}

			Manager.Reset(source);

			sourceInfo.ApplyVolume(clipType, source);

			if (clipType == ClipType.Music) {

				source.clip = state.clip;
				source.Play();

			} else if (clipType == ClipType.SFX) {
				
				source.PlayOneShot(state.clip);

			}

		}
		
		public static void Stop(WindowBase window, Source sourceInfo, ClipType clipType, int id) {

			if (id == 0) {

				// Stop all
				var sources = sourceInfo.GetPlayingSources(window, clipType);
				if (sources == null) return;

				foreach (var source in sources) {

					if (source != null) {

						source.Stop();
						source.clip = null;

					}

				}

			} else {

				var source = sourceInfo.GetPlayingSource(window, clipType, id);
				if (source == null) return;

				source.Stop();
				source.clip = null;

			}

		}

	}

}