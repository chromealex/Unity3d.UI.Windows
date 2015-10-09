using UnityEngine;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Audio {

	public class Manager {

		private static Data currentData = new Data();

		public static void InitAndAdd(Data data, bool overrideValues = false) {

			data.SetupCache();
			
			Manager.currentData.Add(ClipType.Music, data, overrideValues);
			Manager.currentData.Add(ClipType.SFX, data, overrideValues);

		}

		public static void Play(Source sourceInfo, ClipType clipType, int id) {

			if (sourceInfo.source == null) return;

			if (id == 0) {

				// Stop
				Manager.Stop(sourceInfo, clipType, id);
				return;

			}

			var state = Manager.currentData.GetState(clipType, id);
			if (state == null) {
				
				Manager.Stop(sourceInfo, clipType, id);
				return;

			}

			if (clipType == ClipType.Music) {

				sourceInfo.source.clip = state.clip;
				sourceInfo.source.Play();

			} else if (clipType == ClipType.SFX) {
				
				sourceInfo.source.PlayOneShot(state.clip);

			}

		}
		
		public static void Stop(Source sourceInfo, ClipType clipType, int id) {
			
			if (sourceInfo.source == null) return;
			
			sourceInfo.source.Stop();
			sourceInfo.source.clip = null;

		}

	}

}