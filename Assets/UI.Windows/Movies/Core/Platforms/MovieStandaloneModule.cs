using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Movies;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Movies {

	#if UNITY_STANDALONE || UNITY_EDITOR
	[System.Serializable]
	public class MovieStandaloneModule : MovieModuleBase {

		protected override System.Collections.IEnumerator LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, MovieItem movieItem, ResourceBase resource) {

			var filePath = resource.GetStreamPath();

			var task = new WWW(filePath);
			while (task.isDone == false) {

				asyncOperation.SetValues(isDone: false, progress: task.progress, asset: null);
				yield return 0;

			}

			var movie = task.movie;

			asyncOperation.SetValues(isDone: false, progress: 1f, asset: movie);

			task.Dispose();
			task = null;
			System.GC.Collect();

			//Debug.LogWarning("GetTexture_YIELD: " + filePath + " :: " + movie.isReadyToPlay);

			while (movie.isReadyToPlay == false) {

				yield return 0;

			}

			asyncOperation.SetValues(isDone: true, progress: 1f, asset: movie);

		}

		public override bool IsMovie(Texture texture) {

			return texture is MovieTexture;

		}

		protected override void OnPlay(ResourceBase resource, Texture movie, System.Action onComplete) {

			var m = movie as MovieTexture;
			if (m != null) {

				m.Play();

			}

		}

		protected override void OnPlay(ResourceBase resource, Texture movie, bool loop, System.Action onComplete) {

			var m = movie as MovieTexture;
			if (m != null) {

				m.loop = loop;
				m.Play();

			}

		}

		protected override void OnPause(ResourceBase resource, Texture movie) {

			var m = movie as MovieTexture;
			if (m != null) {

				m.Pause();

			}

		}

		protected override void OnStop(ResourceBase resource, Texture movie) {

			var m = movie as MovieTexture;
			if (m != null) {

				m.Stop();

			}

		}

		protected override bool IsPlaying(ResourceBase resource, Texture movie) {

			var m = movie as MovieTexture;
			if (m != null) {

				return m.isPlaying;

			}

			return false;

		}

	}
	#endif

}