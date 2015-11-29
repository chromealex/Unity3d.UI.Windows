using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows.Types;

namespace UnityEngine.UI.Windows.Extensions {
	
	public class SpriteRendererLinker : MonoBehaviour {
		
		[Header("Any Window Object (Required)")]
		public WindowObject windowObject;

		[Header("Targets")]
		public SpriteRenderer spriteRenderer;
		
		[Header("Order Delta (From the Root Canvas)")]
		public int orderDelta;

		public void Start() {

			this.Init();

		}

		public void Init() {

			if (this.spriteRenderer != null && this.windowObject != null) {
				
				var window = this.windowObject.GetWindow();
				if (window == null) {

					Debug.LogWarning("[ SpriteRendererLinker ] WindowObject::GetWindow() is null", this);
					return;

				}
				
				this.spriteRenderer.sortingLayerName = window.GetSortingLayerName();
				this.spriteRenderer.sortingOrder = window.GetSortingOrder() + this.orderDelta;

			}

		}
		
	}

}