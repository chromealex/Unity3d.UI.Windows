using UnityEngine;
using System.Collections;

namespace ME.UAB.Tests {

	public static class UABTests {

		public static void OneSelectionUnpack() {

			var obj = UnityEditor.Selection.activeObject;
			if (obj != null && obj is TextAsset) {

				var serializers = Builder.GetAllSerializers();

				var bytes = (obj as TextAsset).bytes;

				// unpack
				var gos = ME.UAB.Builder.Unpack(bytes, null, serializers);
				for (int i = 0; i < gos.Length; ++i) {

					GameObject.Instantiate(gos[i]);

				}

			} else {

				Debug.LogError("Please, select one TextAsset to pack and unpack");

			}

		}

		public static void OneSelectionPackUnpack() {

			var obj = UnityEditor.Selection.gameObjects;
			if (obj != null) {

				var serializers = Builder.GetAllSerializers();

				// pack
				var data = ME.UAB.Builder.Pack(obj, null, serializers);
				var dataSerialized = UABSerializer.SerializeValueType(data);
				var zipped = Zipper.ZipString(dataSerialized);
				Debug.Log(dataSerialized);
				Debug.Log(string.Format("Json length: {0}, Zipped length: {1}, Objects inside: {2}, Binaries inside: {3}.", dataSerialized.Length, zipped.Length, data.objects.Length, data.binaryData.Length));

				// unpack
				var gos = ME.UAB.Builder.Unpack(zipped, null, serializers);
				for (int i = 0; i < gos.Length; ++i) {

					GameObject.Instantiate(gos[i]);

				}

			} else {

				Debug.LogError("Please, select one gameobject to pack and unpack");

			}

		}

		public static void OneSelectionPack() {

			var obj = UnityEditor.Selection.gameObjects;
			if (obj != null) {

				var serializers = Builder.GetAllSerializers();

				Debug.Log(string.Format("Packing... Objects: {0}", obj.Length));

				// pack
				var data = ME.UAB.Builder.Pack(obj, null, serializers);
				var dataSerialized = UABSerializer.SerializeValueType(data);
				var zipped = Zipper.ZipString(dataSerialized);
				Debug.Log(dataSerialized);
				Debug.Log(string.Format("Json length: {0}, Zipped length: {1}, Objects inside: {2}, Binaries inside: {3}.", dataSerialized.Length, zipped.Length, data.objects.Length, data.binaryData.Length));

			} else {

				Debug.LogError("Please, select one gameobject to pack and unpack");

			}

		}

		public static void OneSelectionPackToFile(string filename) {

			var obj = UnityEditor.Selection.gameObjects;
			if (obj != null) {

				var serializers = Builder.GetAllSerializers();

				Debug.Log(string.Format("Packing... Objects: {0}", obj.Length));

				// pack
				var data = ME.UAB.Builder.Pack(obj, null, serializers);
				var dataSerialized = UABSerializer.SerializeValueType(data);
				var zipped = Zipper.ZipString(dataSerialized);
				Debug.Log(dataSerialized);
				Debug.Log(string.Format("Json length: {0}, Zipped length: {1}, Objects inside: {2}, Binaries inside: {3}.", dataSerialized.Length, zipped.Length, data.objects.Length, data.binaryData.Length));

				System.IO.File.WriteAllBytes(filename, zipped);

			} else {

				Debug.LogError("Please, select one gameobject to pack and unpack");

			}

		}

	}

}
