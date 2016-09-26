using UnityEngine;

namespace UnityEngine.UI.Windows.Components.Modules {

	public class ComponentModuleBase {

		[SceneEditOnly][SerializeField] private bool enabled;

		public bool IsValid() {

			return this.enabled;

		}

		public virtual void ValidateTexture(Texture texture) {}

		public virtual void ValidateMaterial(Material material) {}

		#if UNITY_EDITOR
		public virtual void OnValidateEditor() {

		}
		#endif

	};

}