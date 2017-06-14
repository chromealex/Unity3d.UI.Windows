using UnityEngine;
using System.Collections;

namespace ME {

	public static class TextureExt {

		public static Texture2D MirrorTexture(this Texture2D originalTexture, bool horizontal, bool vertical) {

			Texture2D newTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);;
			Color[] originalPixels = originalTexture.GetPixels(0);
			Color[] newPixels = newTexture.GetPixels(0);

			for (int y = 0; y < originalTexture.height; y++) {

				for (int x = 0; x < originalTexture.width; x++) {

					int newX = horizontal ? (newTexture.width-1-x) : x;
					int newY = vertical ? (newTexture.height-1-y) : y;
					newPixels[(newY * newTexture.width) + newX] = originalPixels[(y * originalTexture.width) + x];

				}

			}

			newTexture.SetPixels(newPixels, 0);
			newTexture.Apply();

			return newTexture;

		}

	}

}