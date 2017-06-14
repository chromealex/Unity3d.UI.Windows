using UnityEngine;

namespace UnityEngine.UI.Windows.Components.Modules {

	public class ComponentModuleBase {

		[HideInInspector][SerializeField] protected IComponent sourceComponent;
		[ReadOnlyBeginGroup("enabled", state: false)]
		[SerializeField] private bool enabled;

		public bool IsValid() {

			return this.enabled;

		}

		public virtual void Init(IComponent source) {

			this.sourceComponent = source;

		}

		public virtual void Prepare(IComponent source) {}

		public virtual void ValidateTexture(Texture texture) {}

		public virtual void ValidateMaterial(Material material) {}

		protected virtual T MakeCopy<T>(RectTransform transform) where T : Graphic {

			var name = (this.sourceComponent as WindowComponent).name;
			var go = new GameObject(string.Format("{0}_copy", name), typeof(RectTransform), typeof(T));
			go.layer = (this.sourceComponent as WindowComponent).gameObject.layer;
			go.transform.SetParent(transform);

			var component = go.GetComponent<T>();

			this.CopyRect(transform, go.transform as RectTransform);

			return component;

		}

		protected void CopyRect(RectTransform source, RectTransform copy) {

			var rect = copy;
			rect.sizeDelta = Vector2.zero;
			rect.pivot = source.pivot;
			rect.anchoredPosition3D = Vector3.zero;
			rect.localScale = Vector3.one;
			rect.localRotation = Quaternion.identity;
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;

		}

		public virtual void SetEnabled() {

			this.enabled = true;

		}

		public virtual void SetDisabled() {

			this.enabled = false;

		}

		public void SetEnabledState(bool state) {

			if (state == true) {

				this.SetEnabled();

			} else {

				this.SetDisabled();

			}

		}

		#if UNITY_EDITOR
		public virtual void OnValidateEditor() {

		}
		#endif

	};

}