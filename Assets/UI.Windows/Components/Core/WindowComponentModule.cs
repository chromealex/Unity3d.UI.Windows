using UnityEngine;

namespace UnityEngine.UI.Windows.Components.Modules {

	public class ComponentModuleBase {

		[HideInInspector][SerializeField] protected IImageComponent image;
		[ReadOnlyBeginGroup("enabled", state: false)]
		[SerializeField] private bool enabled;

		public bool IsValid() {

			return this.enabled;

		}

		public virtual void Init(IImageComponent source) {

			this.image = source;

		}

		public virtual void Prepare(IImageComponent source) {}

		public virtual void ValidateTexture(Texture texture) {}

		public virtual void ValidateMaterial(Material material) {}

		#if UNITY_EDITOR
		public virtual void OnValidateEditor() {

		}
		#endif

	};

}