using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class WindowSystemResourcesMap : ScriptableObject {

		[System.Serializable]
		public class Item {
			
			#if UNITY_EDITOR
			public Texture texture;
			#endif
			public ImageComponent component;

		}

		public List<Item> items = new List<Item>();

		public void Reset() {

			foreach (var item in this.items) {

				var image = item.component;
				if (image.GetResource().controlType != AutoResourceItem.ControlType.None) image.ResetImage();

			}

		}

		#if UNITY_EDITOR
		public void CleanUp() {

			this.items.RemoveAll(x => x.component == null || x.texture == null);

		}

		public void Register(ImageComponent component) {

			this.CleanUp();

			if (this.items.Any(x => x.component == component) == false) {
				
				this.items.Add(new Item() { component = component, texture = component.GetTexture() });
				
			}
			
		}
		
		public void Unregister(ImageComponent component) {
			
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

			this.CleanUp();

		}

		public static WindowSystemResourcesMap FindFirst() {

			var maps = ME.EditorUtilities.GetAssetsOfType<WindowSystemResourcesMap>(predicate: null, useCache: true);
			var mapInstance = maps.Length > 0 ? maps[0] : null;

			return mapInstance;

		}

		[UnityEditor.MenuItem("Assets/Create/UI Windows/Resources Map")]
		public static void Create() {
			
			ME.EditorUtilities.CreateAsset<WindowSystemResourcesMap>();
			
		}
		#endif

	}

}