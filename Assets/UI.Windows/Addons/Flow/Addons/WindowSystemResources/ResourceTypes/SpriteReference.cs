using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

	public class SpriteReference : ScriptableObject {

		public Sprite sprite;

		#if UNITY_EDITOR
		[UnityEditor.MenuItem("Assets/Create/Sprite Reference", true)]
		public static bool CreateInstanceFromSpriteValidate() {

			var objects = UnityEditor.Selection.objects;

			foreach (var obj in objects) {

				if (obj is Sprite) {
					
					return true;

				}

				if (obj is Texture) {

					var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(obj));
					foreach (var s in sprites) {

						if (s is Sprite) {

							return true;

						}

					}

				}

			}

			return false;

		}

		[UnityEditor.MenuItem("Assets/Create/Sprite Reference")]
		public static void CreateInstanceFromSprite() {

			var spritesToConvert = new List<Sprite>();
			var objects = UnityEditor.Selection.objects;

			foreach (var obj in objects) {

				if (obj is Sprite) {

					spritesToConvert.Add(obj as Sprite);

				}

				if (obj is Texture) {

					var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(obj));
					foreach (var s in sprites) {

						if (s is Sprite) {

							spritesToConvert.Add(s as Sprite);

						}

					}

				}

			}

			foreach (var s in spritesToConvert) {

				var rf = ME.EditorUtilities.CreateAsset<SpriteReference>(name: s.name + ".asset", pathWithObject: s);
				rf.sprite = s;
				UnityEditor.EditorUtility.SetDirty(rf);

			}

		}

		[UnityEditor.MenuItem("Assets/Create/UI Windows/Resources/Sprite Reference")]
		public static void CreateInstance() {
			
			ME.EditorUtilities.CreateAsset<SpriteReference>();
			
		}
		#endif

	}

}