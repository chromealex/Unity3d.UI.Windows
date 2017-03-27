using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class WindowSystemResourcesMap : ScriptableObject {

		/*[System.Serializable]
		public class Item {
			
			#if UNITY_EDITOR
			public Object texture;
			#endif
			public WindowComponent component;

		}

		public List<Item> items = new List<Item>();

		public void Reset() {

			foreach (var item in this.items) {

				var image = item.component;
				if ((image as IImageComponent).GetResource().controlType != ResourceAuto.ControlType.None) (image as IImageComponent).ResetImage();

			}

		}*/

		#if UNITY_EDITOR
		/*public void CleanUp() {

			this.items.RemoveAll(x => x.component == null || x.texture == null);

		}

		public void Register(ILoadableResource component) {

			this.CleanUp();

			if (component == null) return;

			if (this.items.Any(x => x.component == component) == false) {
				
				this.items.Add(new Item() { component = component as WindowComponent, texture = (component.GetResource() as IResourceValidationObject).GetValidationObject() });
				
			}
			
		}
		
		public void Unregister(ILoadableResource component) {
			
			this.CleanUp();

			this.items.RemoveAll(x => x.component == component);
			
		}

		public void Validate(Texture texture) {
			
			this.CleanUp();

			foreach (var item in this.items) {

				if (item.texture == texture) {

					item.component.OnValidate();

				}

			}

		}

		public void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif

			this.CleanUp();

		}

		public static WindowSystemResourcesMap FindFirst() {

			var maps = ME.EditorUtilities.GetAssetsOfType<WindowSystemResourcesMap>(predicate: null, useCache: true);
			var mapInstance = maps.Length > 0 ? maps[0] : null;

			return mapInstance;

		}*/

		[UnityEditor.MenuItem("Assets/Create/UI Windows/Resources/Map")]
		public static void Create() {
			
			ME.EditorUtilities.CreateAsset<WindowSystemResourcesMap>();
			
		}
		#endif

	}

}