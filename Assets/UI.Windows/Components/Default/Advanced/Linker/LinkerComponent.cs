using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.Extensions;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

	public class LinkerComponent : WindowComponent {

		[ComponentChooser]
		public WindowComponent prefab;
		[HideInInspector][SerializeField]
		public WindowComponentParametersBase prefabParameters;

		public bool fitToRoot = true;

		//[HideInInspector]
		private WindowComponent instance;

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
		
		public void InitPool(int capacity) {
			
			this.prefab.CreatePool(capacity, this.transform);
			if (this.prefab is LinkerComponent) {
				
				(this.prefab as LinkerComponent).InitPool(capacity);
				
			}
			
		}

		public override void OnInit() {
			
			base.OnInit();

			this.InitInstance();

		}

		private void InitInstance() {

			if (this.instance != null) {

				this.instance.OnInit();

			} else if (this.prefab != null) {

				this.instance = this.prefab.Spawn();
				this.instance.SetParent(this, false);
				this.instance.transform.SetAsFirstSibling();
				this.instance.SetTransformAs(this.prefab);

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

		}

	}

	[System.Serializable]
	public class LinkerComponent<T> : LinkerComponent where T : IComponent {

		public T Get() {

			return this.Get<T>();

		}

	}

}