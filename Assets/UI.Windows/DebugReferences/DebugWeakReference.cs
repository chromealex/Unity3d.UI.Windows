using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows {

	public class DebugWeakReference : MonoBehaviour {
		
		private System.WeakReference rf;

		public void Setup(WindowObject obj) {

			this.rf = new System.WeakReference(obj);

		}

		public void Destroy() {

			this.DestroyWait();

		}

		[ContextMenu("Collect")]
		public void Collect() {

			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();

		}

		[ContextMenu("Destroy")]
		public void DestroyWait() {
			
			this.StartCoroutine(this.WaitAndDestroy());

		}

		private System.Collections.IEnumerator WaitAndDestroy() {
			
			//yield return Resources.UnloadUnusedAssets();

			yield return new WaitForSeconds(2f);

			if (this.rf.IsAlive == false) {

				GameObject.Destroy(this.gameObject);

			}

		}

	}

}