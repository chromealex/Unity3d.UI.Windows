using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class WindowSystemAssetBundlesMap : ScriptableObject
	{

		#if UNITY_EDITOR
		[System.Serializable]
		public class Item {
			
			public string name;
			public string[] assetIndexMap;
			
		}

		public List<Item> items = new List<Item>();

		[UnityEditor.MenuItem("Assets/Create/UI Windows/AssetBundles Map")]
		public static void Create() {

			ME.EditorUtilities.CreateAsset<WindowSystemAssetBundlesMap>();

		}

		public void Reset() {

			this.items.Clear();

			UnityEditor.EditorUtility.SetDirty(this);

		}

		public void AddItem(string assetBundleName, string[] assetIndexMap)
		{
			for (int i = 0, size = this.items.Count; i < size; ++i)
			{
				if (this.items[i].name.Equals(assetBundleName))
				{
					Debug.LogError("items already contains " + assetBundleName);
					return;
				}
			}
			var item = new Item()
			{
				name = assetBundleName,
				assetIndexMap = assetIndexMap
			};
			this.items.Add(item);

			UnityEditor.EditorUtility.SetDirty(this);

		}

		public int GetIndex(string assetBundleName, string assetPath)
		{
			int result = -1;
			for (int i = 0, size = this.items.Count; i < size; ++i)
			{
				if (this.items[i].name.Equals(assetBundleName))
				{
					result = System.Array.IndexOf(this.items[i].assetIndexMap, assetPath);
					if (result == -1)
					{
						Debug.LogError("Asset name: " + assetPath + " not found in " + assetBundleName);
					}
					break;
				}
				
			}
			
			return result;
		}
		#endif

	}

}