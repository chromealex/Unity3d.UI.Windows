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

		public void Init() {

			this.source.CreatePool(10);

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

			return value;

		}

	}

}