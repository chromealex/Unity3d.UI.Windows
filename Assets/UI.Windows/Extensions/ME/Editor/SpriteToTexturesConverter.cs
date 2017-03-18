using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace ME {

	public static class SpriteToTexturesConverter {

		[MenuItem("Assets/Convert/Sprite[] to Texture[]", isValidateFunction: true)]
		public static bool ConvertToTexturesValidate() {

			var selected = Selection.activeObject as Texture2D;
			if (selected != null) {

				return true;

			}

			return false;

		}

		[MenuItem("Assets/Convert/Sprite[] to Texture[]")]
		public static void ConvertToTextures() {

			var selected = Selection.activeObject as Texture2D;
			if (selected != null) {

				var path = AssetDatabase.GetAssetPath(selected);
				var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

				var resultPath = EditorUtility.OpenFolderPanel("Destination Folder", path, "");
				if (string.IsNullOrEmpty(resultPath) == false) {

					var rPath = resultPath.Split(new [] { "/Assets/" }, StringSplitOptions.None);
					resultPath = "Assets/" + rPath[1];

					try {

						resultPath = resultPath + "/" + selected.name + "/";
						if (System.IO.Directory.Exists(resultPath) == false) {

							System.IO.Directory.CreateDirectory(resultPath);

						}

						for (int i = 0; i < assets.Length; ++i) {

							var asset = assets[i] as Sprite;
							if (asset != null) {
								
								var assetPath = resultPath + asset.name + ".png";

								EditorUtility.DisplayProgressBar("Converting...", assetPath, i / (float)assets.Length);

								var texture = new Texture2D((int)asset.rect.width, (int)asset.rect.height, TextureFormat.ARGB32, mipmap: false);
								texture.SetPixels(asset.texture.GetPixels((int)asset.rect.xMin, (int)asset.rect.yMin, (int)asset.rect.width, (int)asset.rect.height));
								texture.Apply();
								System.IO.File.WriteAllBytes(assetPath, texture.EncodeToPNG());
								AssetDatabase.ImportAsset(assetPath);
								var importer = TextureImporter.GetAtPath(assetPath) as TextureImporter;
								if (importer != null) {
									
									importer.mipmapEnabled = false;
									#if UNITY_5_5_OR_NEWER
									var settings = new TextureImporterPlatformSettings();
									settings.format = TextureImporterFormat.ARGB32;
									settings.maxTextureSize = 2048;
									settings.name = "default";
									importer.SetPlatformTextureSettings(settings);
									#else
									importer.SetPlatformTextureSettings("default", 2048, TextureImporterFormat.ARGB32);
									#endif
									importer.SaveAndReimport();

								} else {

									Debug.LogWarning("TextureImporter was not found at path: " + assetPath);

								}

							}

						}

					} catch (Exception ex) {

						Debug.LogException(ex);

					} finally {

						EditorUtility.ClearProgressBar();

					}

					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				}

			}

		}

	}

}