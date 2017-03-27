using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows.Plugins.Console.SubModules {

	public class FPS : SubModuleBase {

		public class FPSCounter {

			private MonoBehaviour mb;
			private TextComponent textComponent;

			private bool alive = false;

			public FPSCounter(MonoBehaviour mb, TextComponent textComponent) {

				this.mb = mb;
				this.textComponent = textComponent;
				this.alive = true;

				this.Start();

			}

			public void Dispose() {

				this.alive = false;

			}

			/* Public Variables */
			public float frequency = 0.5f;
			
			/* **********************************************************************
			 * PROPERTIES
			 * *********************************************************************/
			public int framesPerSec { get; protected set; }
			public string text { get; protected set; }
			
			/* **********************************************************************
			 * EVENT HANDLERS
			 * *********************************************************************/
			/*
			 * EVENT: Start
			 */
			private void Start() {

				this.mb.StartCoroutine(this.FPS());

			}
			
			/*
			 * EVENT: FPS
			 */
			private System.Collections.IEnumerator FPS() {

				for (;;) {

					// Capture frame-per-second
					int lastFrameCount = UnityEngine.Time.frameCount;
					float lastTime = UnityEngine.Time.realtimeSinceStartup;

					yield return new WaitForSeconds(frequency);

					float timeSpan = UnityEngine.Time.realtimeSinceStartup - lastTime;
					int frameCount = UnityEngine.Time.frameCount - lastFrameCount;
					
					// Display it
					this.framesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
					this.text = string.Format("{0} fps", this.framesPerSec.ToString());
					
					this.textComponent.SetText(this.text);

					if (this.alive == false) {

						this.textComponent.SetText(string.Empty);
						yield break;

					}

				}

			}

		}

		private FPSCounter counter = null;

		[Executable, Help("Turn On FPS counter")]
		public string On() {

			this.counter = new FPSCounter(ConsoleManager.GetInstance(), this.screen.alwaysShownText);

			return string.Empty;

		}
		
		[Executable, Help("Turn Off FPS counter")]
		public string Off() {

			if (this.counter != null) {

				this.counter.Dispose();
				this.counter = null;

			}

			return string.Empty;

		}

	}

}