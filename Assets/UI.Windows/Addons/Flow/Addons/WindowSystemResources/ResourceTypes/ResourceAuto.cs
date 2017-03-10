using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Resources;

namespace UnityEngine.UI.Windows {
	
	[System.Serializable]
	public class ResourceAuto : ResourceBase, IResourceValidationObject {

		public ResourceAuto() : base() {}
		public ResourceAuto(ControlType controlType, bool async) : base(controlType, async) {}

		public static ResourceAuto CreateResourceRequest(string path) {

			var item = new ResourceAuto();

			item.async = true;
			item.id = ResourceBase.GetJavaHash(path);
			item.controlType = ControlType.Show | ControlType.Hide;
			item.loadableResource = true;
			item.resourcesPath = path;

			return item;

		}

		public static ResourceAuto CreateStreamRequest(string path, int cacheVersion = 0) {

			var item = new ResourceAuto();

			item.async = true;
			item.id = ResourceBase.GetJavaHash(path);
			item.controlType = ControlType.Show | ControlType.Hide;
			item.loadableStream = true;
			item.cacheVersion = cacheVersion;
			item.streamingAssetsPathCommon = path;

			return item;

		}

		public static ResourceAuto CreateWebRequest(string path, int cacheVersion = 0) {

			var item = new ResourceAuto();

			item.async = true;
			item.id = ResourceBase.GetJavaHash(path);
			item.controlType = ControlType.Show | ControlType.Hide;
			item.loadableWeb = true;
			item.cacheVersion = cacheVersion;
			item.webPath = path;

			return item;

		}

		public Object GetValidationObject() {
			
			#if UNITY_EDITOR
			return this.tempObject;
			#else
			return null;
			#endif

		}

		#if UNITY_EDITOR
		//[HideInInspector]
		[BundleIgnore]
		public Object tempObject;
		#endif
		
		#if UNITY_EDITOR
		public override void ResetToDefault() {
			
			base.ResetToDefault();

			this.tempObject = null;
			
		}

		public override void Validate() {

			this.Validate(this.tempObject);

		}

		public override void Validate(Object item) {

			if (item == null) {

				this.ResetToDefault();
				return;

			}

			var tempSprite = item as Sprite;
			if (tempSprite != null) {
				
				var imp = UnityEditor.TextureImporter.GetAtPath(this.assetPath) as UnityEditor.TextureImporter;
				if (imp != null && imp.spriteImportMode == UnityEditor.SpriteImportMode.Multiple) {

					var allObjects = Resources.LoadAll(this.resourcesPath);
					ME.EditorUtilities.SetValueIfDirty(ref this.multiObjects, true);

					ME.EditorUtilities.SetValueIfDirty(ref this.objectIndex, System.Array.IndexOf(allObjects, item));

				} else {

					ME.EditorUtilities.SetValueIfDirty(ref this.multiObjects, false);

				}
				
			}

			ME.EditorUtilities.SetObjectIfDirty(ref this.tempObject, item);

			base.Validate(item);

		}
		#endif
		
	}

}