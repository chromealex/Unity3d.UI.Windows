using UnityEngine;
using System.Collections;

namespace ME.UAB.Tests {

	public static class UABTests {

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
				var unzipped = Zipper.UnzipString(zipped);
				var dataDeserialized = UABSerializer.DeserializeValueType<UABPackage>(unzipped);
				var gos = ME.UAB.Builder.Unpack(dataDeserialized, serializers);
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

	}

}
