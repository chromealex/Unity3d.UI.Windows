using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace ME.UAB.Tests {

	public static class UABTests {
		
		public static void BuildUnityAssetBundles(string dir) {

			#if UNITY_5_5_OR_NEWER
			UnityEditor.BuildPipeline.BuildAssetBundles(dir, UnityEditor.BuildAssetBundleOptions.StrictMode | UnityEditor.BuildAssetBundleOptions.ChunkBasedCompression | UnityEditor.BuildAssetBundleOptions.DisableWriteTypeTree, UnityEditor.EditorUserBuildSettings.activeBuildTarget);
			#else
			UnityEditor.BuildPipeline.BuildAssetBundles(dir, UnityEditor.BuildAssetBundleOptions.ChunkBasedCompression | UnityEditor.BuildAssetBundleOptions.DisableWriteTypeTree, UnityEditor.EditorUserBuildSettings.activeBuildTarget);
			#endif

		}

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

		[NUnit.Framework.Test]
		public static void JsonEncodeDecode() {

			var package = new UABPackage();
			var serialized = UABSerializer.SerializeValueType(package);
			var deserialized = UABSerializer.DeserializeValueType<UABPackage>(serialized);
			var serialized2 = UABSerializer.SerializeValueType(deserialized);

			NUnit.Framework.Assert.IsTrue(serialized == serialized2);

		}

		[NUnit.Framework.Test]
		public static void PackUnpackGameObject() {

			var objs = UABTests.GetTestObjects();

			var serializers = Builder.GetAllSerializers();
			var config = Builder.GetDefaultConfig(required: true);

			var data = ME.UAB.Builder.Pack(objs, config, serializers);
			var dataSerialized = UABSerializer.SerializeValueType(data);
			var zipped = Zipper.ZipString(dataSerialized);

			var unzipped = Zipper.UnzipString(zipped);
			var dataDeserialized = UABSerializer.DeserializeValueType<UABPackage>(unzipped);
			var gos = ME.UAB.Builder.Unpack(dataDeserialized, config, serializers);

			NUnit.Framework.Assert.IsTrue(UABTests.CompareTestObjects(objs, gos));

		}

		public static bool CompareType<T>(bool system, params System.Type[] requiredTypes) where T : Object {

			var types = new List<System.Type>();
			var reqs = typeof(T).GetCustomAttributes(typeof(RequireComponent), inherit: true);
			if (reqs != null) {

				for (int i = 0; i < reqs.Length; ++i) {

					var req = reqs[i] as RequireComponent;
					if (req != null) {

						if (req.m_Type0 != null) types.Add(req.m_Type0);
						if (req.m_Type1 != null) types.Add(req.m_Type1);
						if (req.m_Type2 != null) types.Add(req.m_Type2);

					}

				}

			}

			if (requiredTypes != null) types.AddRange(requiredTypes);
			types.Add(typeof(T));

			var go = new GameObject("UTest", types.ToArray());
			var objs = new GameObject[1] { go };

			if (typeof(T) == typeof(Animator)) {

				go.GetComponent<Animator>().bodyRotation = Quaternion.identity;

			}

			#if UNITY_5_5_OR_NEWER
			if (typeof(T) == typeof(ParticleSystem)) {

				go.GetComponent<ParticleSystem>().useAutoRandomSeed = false;

			}
			#endif

			//go.SendMessage("OnValidate");

			var serializers = Builder.GetAllSerializers();
			var config = Builder.GetDefaultConfig(required: true);

			var data = ME.UAB.Builder.Pack(objs, config, serializers);
			var dataSerialized = UABSerializer.SerializeValueType(data);
			var zipped = Zipper.ZipString(dataSerialized);

			var unzipped = Zipper.UnzipString(zipped);
			var dataDeserialized = UABSerializer.DeserializeValueType<UABPackage>(unzipped);
			var gos = ME.UAB.Builder.Unpack(dataDeserialized, config, serializers);

			for (int i = 0; i < gos.Length; ++i) {

				var t1 = gos[i].GetComponent<T>();
				var fields1 = t1.GetType().GetAllProperties().Where(x => x.GetCustomAttributes(true).Any(a => a is System.ObsoleteAttribute) == false && x.CanWrite == true && UABSerializer.FilterProperties(x) == true).ToArray();

				var t2 = objs[i].GetComponent<T>();
				var fields2 = t1.GetType().GetAllProperties().Where(x => x.GetCustomAttributes(true).Any(a => a is System.ObsoleteAttribute) == false && x.CanWrite == true && UABSerializer.FilterProperties(x) == true).ToArray();

				var result = UABTests.CompareFields(fields1, fields2, t1, t2, system);
				if (result == false) return false;

			}

			return true;

		}

		private static bool CompareFields(PropertyInfo[] fields1, PropertyInfo[] fields2, object t1, object t2, bool system) {

			if (fields1.Length != fields2.Length) return false;

			System.Func<string, bool, string> formatAssume = (string value, bool serious) => {

				return string.Format("`{0}` is Unity serialization bug, {1}", value, (serious == true ? "it will be normaly serialize/deserialize by UAB, but this parameter will be ignored" : "it will be normaly serialize/deserialize by UAB"));

			};

			for (int i = 0; i < fields1.Length; ++i) {

				if (system == true) {

					if (
						fields1[i].Name == "m_CachedPtr" ||
						fields1[i].Name == "m_InstanceID" ||
						fields1[i].Name == "material") continue;

				}

				if (fields1[i].Name == "hideFlags") continue;

				/*if (t1 is Animator) {

					if (fields1[i].Name == "bodyRotation") {
						NUnit.Framework.Assume.That(false, formatAssume("bodyRotation", false));
						continue;
					}

				}*/

				if (t1 is Joint2D || t1 is Joint) {

					if (fields1[i].Name == "breakForce") {
						NUnit.Framework.Assume.That(false, formatAssume("breakForce", false));
						continue;
					}
					if (fields1[i].Name == "breakTorque") {
						NUnit.Framework.Assume.That(false, formatAssume("breakTorque", false));
						continue;
					}

				}

				if (t1 is HingeJoint2D || t1 is SliderJoint2D || t1 is WheelJoint2D) {

					if (fields1[i].Name == "useMotor") {
						NUnit.Framework.Assume.That(false, formatAssume("useMotor", true));
						continue;
					}
					if (fields1[i].Name == "useLimits") {
						NUnit.Framework.Assume.That(false, formatAssume("useLimits", true));
						continue;
					}

				}

				var v1 = fields1[i].GetValue(t1, null);
				var v2 = fields2[i].GetValue(t2, null);

				if (v1 != null && v2 != null && v1.ToString() != v2.ToString()) {

					NUnit.Framework.Assume.That(false, string.Format("[ CompareFields ] {0} != {1}, Field Name: {2}", v1, v2, fields1[i].Name));
					return false;

				}

			}

			return true;

		}

		private static bool CompareTestObjects(GameObject[] gos1, GameObject[] gos2) {

			if (gos1.Length != gos2.Length) return false;

			for (int i = 0; i < gos1.Length; ++i) {

				var go1 = gos1[i];
				var go2 = gos2[i];
				var result = (go1.name == go2.name && go1.layer == go2.layer && go1.tag == go2.tag);
				if (result == false) return false;

			}

			return true;

		}

		private static GameObject[] GetTestObjects() {

			var name = "UTest";
			var layer = 12;
			var tag = "EditorOnly";

			var obj = new GameObject();
			obj.name = name;
			obj.layer = layer;
			obj.tag = tag;

			return new GameObject[1] { obj };

		}

	}

}
