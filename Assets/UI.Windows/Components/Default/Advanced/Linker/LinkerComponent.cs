using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.Extensions;
using System.Collections.Generic;
using ME.UAB;

namespace UnityEngine.UI.Windows.Components {

	public class LinkerComponent : WindowComponent, IResourceReference {

		#if UNITY_EDITOR
		[SerializeField][BundleIgnore][ComponentChooser]
		private WindowComponent prefab;
		[SceneEditOnly]
		public bool editorPreload = false;
		[SceneEditOnly]
		public bool editorPreloadRefresh = false;
		#endif

		[ReadOnly]
		public WindowComponent prefabNoResource;
		[ReadOnly][SerializeField][ResourceParameters(ResourceAuto.ControlType.None)]
		public ResourceAuto prefabResource = new ResourceAuto(ResourceAuto.ControlType.None, async: false);

		[HideInInspector]
		[SerializeField]
		public WindowComponentParametersBase prefabParameters;

		public bool fitToRoot = true;

		[HideInInspector]
		[SerializeField]
		private WindowComponent _instance;
		private WindowComponent instance {

			get {

				return this._instance;

			}

		}

		public bool Unload() {

			if (this.instance != null) {

				this.UnregisterSubComponent(this.instance);
				this.instance.Recycle();

				this.UnloadSafe();

				return true;

			}

			this.UnloadSafe();

			return false;

		}

		public void UnloadSafe() {
			
			this._instance = null;

		}

		public T Get<T>(ref T instance) where T : IComponent {
			
			instance = this.Get<T>();

			return instance;

		}

		public T Get<T>() where T : IComponent {

			if (this.instance == null) {

				this.InitInstance();

			}

			return (T)(this.instance as IComponent);

		}

		public void GetAsync<T>(System.Action<T> callback = null) where T : IComponent {

			/*if (this.instance != null) {

				if (callback != null) callback.Invoke((T)(this.instance as IComponent));
				return;

			}*/

			this.InitInstance((asset) => {

				if (callback != null) callback.Invoke((T)(this.instance as IComponent));

			}, async: true);

		}

		public void InitPool(int capacity) {

			var prefab = this.prefabNoResource;
			if (prefab != null) {

				prefab.CreatePool(capacity, this.transform);
				if (prefab is LinkerComponent) {

					(prefab as LinkerComponent).InitPool(capacity);

				}

			}

		}

		public override void DoLoad(bool async, System.Action<WindowObjectElement> onItem, System.Action callback) {
			
			this.InitInstance(c => base.DoLoad(async, onItem, callback), async);

		}

		/*public override void OnInit() {

			base.OnInit();

			this.InitInstance();

		}*/

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.UnloadSafe();

		}

		private void InitInstance(System.Action<WindowComponent> callback = null, bool async = false) {

			if (this.instance != null) {

				//this.instance.OnInit();
				if (callback != null) callback.Invoke(this.instance);

			} else {

				if (this.prefabNoResource != null) {

					this.Load_INTERNAL(this.prefabNoResource);
					if (callback != null) callback.Invoke(this.instance);

				} else {

					if (this.prefabResource.IsLoadable() == true) {

						WindowSystemResources.LoadRefCounter<WindowComponent>(this, this.prefabResource, (asset) => {

							this.Load_INTERNAL(asset);
							WindowSystemResources.Unload(this, this.prefabResource, resetController: false);

							if (callback != null) callback.Invoke(asset);

						}, () => {

							Debug.LogError(string.Format("[LinkerComponent] Failed to load prefab at path `{0}`", this.prefabResource.assetPath), this);
							if (callback != null) callback.Invoke(null);

						}, this.prefabResource.async || async);

					} else {

						if (callback != null) callback.Invoke(null);

					}

				}

			}

		}

		private void Load_INTERNAL(WindowComponent asset) {

			if (this == null) return;

			this._instance = asset.Spawn();
			this.instance.SetParent(this, false);
			this.instance.transform.SetAsFirstSibling();
			this.instance.SetTransformAs(asset);

			if (this.prefabParameters != null) {

				this.instance.Setup(this.prefabParameters);

			}

			if (this.fitToRoot == true) {

				var rect = this.instance.transform as RectTransform;
				if (rect != null) {

					rect.anchorMin = Vector2.zero;
					rect.anchorMax = Vector2.one;
					rect.sizeDelta = Vector2.zero;

				}

			}

			this.instance.Setup(this.GetLayoutRoot());
			this.instance.Setup(this.GetWindow());

			this.RegisterSubComponent(this.instance);

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			if (this.editorPreload == false) {
					
				this.prefabResource.tempObject = this.prefab;
				this.prefabResource.Validate();

				this.prefabNoResource = null;
				if (this.prefabResource.IsLoadable() == false) {

					this.prefabNoResource = this.prefab;

				}

			}

			if (this.editorPreloadRefresh == true && ME.EditorUtilities.IsPrefab(this.gameObject) == false) {

				this.editorPreloadRefresh = false;

				if (this._instance != null) {

					this.UnregisterSubComponent(this._instance);
					var oldInstance = this._instance;
					UnityEditor.EditorApplication.CallbackFunction onDelay = null;
					onDelay = () => {

						UnityEditor.EditorApplication.delayCall -= onDelay;
						GameObject.DestroyImmediate(oldInstance.gameObject, true);

					};
					UnityEditor.EditorApplication.delayCall += onDelay;

				}

				if (this.editorPreload == true) {

					if (this.prefab != null) this.Load_INTERNAL(this.prefab);

					this.prefabNoResource = null;
					this.prefabResource.ResetToDefault();

				}

			}

		}
		#endif

	}

	[System.Serializable]
	public class LinkerComponent<T> : LinkerComponent where T : IComponent {

		public T Get(ref T instance) {

			return this.Get<T>(ref instance);

		}

		public T Get() {

			return this.Get<T>();

		}

		public void GetAsync(System.Action<T> callback) {

			this.GetAsync<T>(callback);

		}

	}

}