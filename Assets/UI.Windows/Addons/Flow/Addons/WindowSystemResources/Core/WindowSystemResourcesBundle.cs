using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.IO.Compression;

namespace UnityEngine.UI.Windows.Plugins.Resources {

	public class BundleIgnoreAttribute : System.Attribute {
	};

	[System.Serializable]
	public class BundleStructureComponent {

		[System.Serializable]
		public class Field {

			public enum Type {
				ValueType,
				Reference,
				Array,
				Nested,
				DefaultValue,
				Vector2,
				Vector3,
				Vector4,
				Quaternion,
				Color,
				RectOffset,
				Binary,
			};

			[System.Serializable]
			public class Data {
				
			}

			[System.Serializable]
			public class DataVector2 : Data {

				public float x;
				public float y;

			}

			[System.Serializable]
			public class DataVector3 : Data {

				public float x;
				public float y;
				public float z;

			}

			[System.Serializable]
			public class DataVector4 : Data {

				public float x;
				public float y;
				public float z;
				public float w;

			}

			[System.Serializable]
			public class DataQuaternion : Data {

				public float x;
				public float y;
				public float z;
				public float w;

			}

			[System.Serializable]
			public class DataColor : Data {

				public byte r;
				public byte g;
				public byte b;
				public byte a;

			}

			[System.Serializable]
			public class DataRectOffset : Data {

				public int top;
				public int bottom;
				public int left;
				public int right;

			}

			[System.Serializable]
			public class DataValueType : Data {

				public string data;	// data

			}

			[System.Serializable]
			public class DataReference : Data {

				public string instanceId;	// instanceId
				public bool local;			// isLocal
				public bool isGameObject;

			}

			[System.Serializable]
			public class DataBinary : Data {

				public string instanceId;	// instanceId

			}

			[System.Serializable]
			public class DataArray : Data {

				public Field[] fields;	// fields

			}

			[System.Serializable]
			public class DataNested : Data {

				public BundleStructureComponent data;

			}

			public Type type;		// type
			public Data data {

				get {

					Data result = null;
					switch (this.type) {

						case Type.Array:
							result = this.array;
							break;

						case Type.Nested:
							result = this.nested;
							break;

						case Type.Reference:
							result = this.reference;
							break;

						case Type.ValueType:
							result = this.valueType;
							break;

						case Type.Vector2:
							result = this.vector2;
							break;

						case Type.Vector3:
							result = this.vector3;
							break;

						case Type.Vector4:
							result = this.vector4;
							break;

						case Type.Quaternion:
							result = this.quaternion;
							break;

						case Type.Color:
							result = this.color;
							break;

						case Type.RectOffset:
							result = this.rectOffset;
							break;

						case Type.Binary:
							result = this.binary;
							break;

						default:
							break;

					}

					return result;

				}

				set {

					this.array = null;
					this.reference = null;
					this.nested = null;
					this.valueType = null;
					this.color = null;
					this.vector2 = null;
					this.vector3 = null;
					this.vector4 = null;
					this.quaternion = null;
					this.rectOffset = null;
					this.binary = null;
					switch (this.type) {

						case Type.Array:
							this.array = value as DataArray;
							break;

						case Type.Nested:
							this.nested = value as DataNested;
							break;

						case Type.Reference:
							this.reference = value as DataReference;
							break;

						case Type.ValueType:
							this.valueType = value as DataValueType;
							break;

						case Type.Vector2:
							this.vector2 = value as DataVector2;
							break;

						case Type.Vector3:
							this.vector3 = value as DataVector3;
							break;

						case Type.Vector4:
							this.vector4 = value as DataVector4;
							break;

						case Type.Quaternion:
							this.quaternion = value as DataQuaternion;
							break;

						case Type.Color:
							this.color = value as DataColor;
							break;

						case Type.RectOffset:
							this.rectOffset = value as DataRectOffset;
							break;

						case Type.Binary:
							this.binary = value as DataBinary;
							break;

						default:
							break;

					}

				}

			}

			public string name;
			public DataArray array;
			public DataReference reference;
			public DataNested nested;
			public DataValueType valueType;
			public DataColor color;
			public DataVector2 vector2;
			public DataVector3 vector3;
			public DataVector4 vector4;
			public DataQuaternion quaternion;
			public DataRectOffset rectOffset;
			public DataBinary binary;

		}

		public int instanceId;		// instanceId
		public string type;			// type
		public Field[] fields;		// fields

	}

	[System.Serializable]
	public class BundleBinaryData {

		public enum Type {
			Texture,
			Sprite,
			Mesh,
		};

		public string id;
		public Type type;
		public string data;

		public Rect spriteRect;
		public Vector2 spritePivot;
		public float spritePixelsPerUnit;
		public uint spriteExtrude;
		public SpriteMeshType spriteMeshType;
		public Vector4 spriteBorder;

	}

	[System.Serializable]
	public class BundleStructureGameObject {

		public BundleStructureComponent[] components;	// components
		public BundleStructureGameObject[] childs;		// childs
		public BundleBinaryData[] binaries;				// binary data

		public string name;	// name
		public string tag;	// tag
		public int layer;	// layer
		public bool active;	// active

	}

	[System.Serializable]
	public class BundleStructureGameObjectRoot : BundleStructureGameObject {
		
	}

	public class WindowSystemResourcesBundle {

		public static Package Load(string data) {

			return WindowSystemResourcesBundle.Load(System.Text.Encoding.UTF8.GetBytes(data));

		}

		public static Package Load(byte[] data) {

			var package = new Package();
			package.Deserialize(Zip.Decompress(data));
			return package;

		}

		public static T Instantiate<T>(T source) where T : Component {

			return WindowSystemResourcesBundle.Instantiate(source.gameObject).GetComponent<T>();

		}

		public static GameObject Instantiate(GameObject source) {

			var package = new Package();
			var packed = package.Pack(source);
			package.Free();
			var go = package.Unpack(packed);
			go.hideFlags = HideFlags.None;
			return go;

		}

		#if UNITY_EDITOR
		public static byte[] Build(GameObject root) {

			var package = new Package();
			var packed = package.Pack(root);
			var serialized = package.Serialize(packed);
			package.Free();

			Debug.Log(serialized);
			return Zip.Compress(serialized);

		}

		public static void Build(string path, int version = 0) {

			var initializer = GameObject.FindObjectOfType<UnityEngine.UI.Windows.WindowSystemFlow>();
			if (initializer != null) {

				var flowData = initializer.GetBaseFlow();
				if (flowData == null) {

					Debug.LogError("`FlowData` was not found. Open the scene with WindowSystemFlow initializer.");
					return;

				}

				if (System.IO.Directory.Exists(path) == false) {

					System.IO.Directory.CreateDirectory(path);

				}

				var bundles = UnityEditor.AssetDatabase.GetAllAssetBundleNames();
				for (int i = 0; i < bundles.Length; ++i) {

					var bundle = bundles[i];
					var assetsPath = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
					for (int j = 0; j < assetsPath.Length; ++j) {

						var assetPath = assetsPath[j];
						var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
						if (asset != null) {

							var assetBuildPath = string.Format("{0}/{1}.uiwb", path, bundle);

							var dir = System.IO.Path.GetDirectoryName(assetBuildPath);
							if (System.IO.Directory.Exists(dir) == false) {

								System.IO.Directory.CreateDirectory(dir);

							}

							var data = WindowSystemResourcesBundle.Build(asset);
							System.IO.File.WriteAllBytes(assetBuildPath, data);

							if (version > 0) {

								var name = System.IO.Path.GetFileName(assetBuildPath);
								var builtinPath = string.Format("{0}/BundlesCache/{1}", Application.streamingAssetsPath, version, name);
								var builtinDir = System.IO.Path.GetDirectoryName(builtinPath);
								if (System.IO.Directory.Exists(builtinDir) == false) {

									System.IO.Directory.CreateDirectory(builtinDir);

								}

								System.IO.File.WriteAllBytes(builtinPath, data);
								
							}

						}

					}

				}

			}

		}

		[UnityEditor.MenuItem("Window/UI.Windows: Tests/Resource Bundles/One Selection->Pack->Unpack")]
		public static void TestOneSelectionPackUnpack() {

			var obj = UnityEditor.Selection.activeGameObject;
			if (obj != null) {

				var data = WindowSystemResourcesBundle.Build(obj);

				var package = WindowSystemResourcesBundle.Load(data);
				GameObject.Instantiate(package.Unpack());
				package.Free();
				/*
				var package = new Package();
				var packed = package.Pack(obj);
				var serialized = package.Serialize(packed);
				Debug.Log(serialized);
				package.Free();

				var newPackage = new Package();
				var unpacked = newPackage.Deserialize(serialized);
				var go = newPackage.Unpack(unpacked);
				GameObject.Instantiate(go);
				newPackage.Free();*/

			} else {

				Debug.LogError("Please, select one gameobject to pack and unpack");

			}

		}
		#endif

		public class Package {

			private string currentBundleName;

			private Dictionary<int, Object> localReferences = new Dictionary<int, Object>();
			private Dictionary<string, Object> projectReferences = new Dictionary<string, Object>();
			private Dictionary<string, Object> localBinaries = new Dictionary<string, Object>();
			private GameObject unpackedRoot;

			private BundleStructureGameObject deserialized;

			public string SerializeObject(object value) {
				
				return UnityEngine.UI.Windows.Extensions.Tiny.Json.Encode(value);

			}

			public object DeserializeObject(string data, string type) {

				return UnityEngine.UI.Windows.Extensions.Tiny.Json.Decode(data, System.Type.GetType(type));

			}

			public object DeserializeObject(string data, System.Type type) {

				return UnityEngine.UI.Windows.Extensions.Tiny.Json.Decode(data, type);

			}

			public T DeserializeObject<T>(string data) {

				return UnityEngine.UI.Windows.Extensions.Tiny.Json.Decode<T>(data);

			}

			public string Serialize(BundleStructureGameObject root) {

				return UnityEngine.UI.Windows.Extensions.Tiny.Json.Encode(root);

			}

			public BundleStructureGameObject Deserialize(string data) {

				this.deserialized = UnityEngine.UI.Windows.Extensions.Tiny.Json.Decode<BundleStructureGameObject>(data);
				return this.deserialized;

			}

			#if UNITY_EDITOR
			private bool PrepareForEncode(Texture tex) {

				var importer = UnityEditor.TextureImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(tex)) as UnityEditor.TextureImporter;
				if (importer != null) {

					var oldValue = importer.isReadable;
					importer.isReadable = true;
					importer.SaveAndReimport();
					/*UnityEditor.TextureImporterSettings settings = new UnityEditor.TextureImporterSettings();
					importer.ReadTextureSettings(settings);
					var oldValue = settings.readable;
					if (oldValue == false) {
						
						settings.readable = true;
						importer.SetTextureSettings(settings);
						importer.SaveAndReimport();

					}*/

					return oldValue;

				}

				return false;

			}

			private void CompleteEncoding(Texture tex, bool readable) {

				var importer = UnityEditor.TextureImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(tex)) as UnityEditor.TextureImporter;
				if (importer != null) {

					importer.isReadable = readable;
					importer.SaveAndReimport();

					/*UnityEditor.TextureImporterSettings settings = new UnityEditor.TextureImporterSettings();
					importer.ReadTextureSettings(settings);
					if (readable != settings.readable) {
						
						settings.readable = readable;
						importer.SetTextureSettings(settings);
						importer.SaveAndReimport();

					}*/

				}

			}
			#endif

			public void BuildProjectReferencesMap(BundleStructureGameObject data) {

				var flowData = Flow.FlowSystem.GetData();
				if (flowData == null) {

					Debug.LogError("Project References can't be created because of FlowSystem.GetData() returns null.");
					return;

				}

				#if UNITY_EDITOR
				var path = System.IO.Path.GetDirectoryName(UnityEditor.AssetDatabase.GetAssetPath(flowData));
				path += "/Bundles/Resources/";
				if (System.IO.Directory.Exists(path) == false) {

					System.IO.Directory.CreateDirectory(path);
					UnityEditor.AssetDatabase.ImportAsset(path);

				}
				#endif

				var binaries = new List<BundleBinaryData>();
				foreach (var item in this.projectReferences) {

					if (this.IsBinary(item.Value) == true) {

						var binData = new BundleBinaryData();
						binData.id = item.Key;
						if (item.Value is Mesh) {

							binData.type = BundleBinaryData.Type.Mesh;
							var bytes = MeshSerializer.WriteMesh(item.Value as Mesh, saveTangents: true);
							binData.data = Convert.ToBase64String(bytes);

						} else if (item.Value is Texture) {

							binData.type = BundleBinaryData.Type.Texture;
							#if UNITY_EDITOR
							var oldReadable = this.PrepareForEncode(item.Value as Texture);
							var texture = (item.Value as Texture2D);
							var bytes = texture.EncodeToPNG();
							this.CompleteEncoding(texture, oldReadable);
							binData.data = Convert.ToBase64String(bytes);
							#endif

						} else if (item.Value is Sprite) {

							binData.type = BundleBinaryData.Type.Sprite;
							#if UNITY_EDITOR
							var sprite = (item.Value as Sprite);
							var oldReadable = this.PrepareForEncode(sprite.texture);
							var bytes = sprite.texture.EncodeToPNG();
							this.CompleteEncoding(sprite.texture, oldReadable);
							binData.data = Convert.ToBase64String(bytes);
							binData.spriteRect = sprite.rect;
							binData.spritePivot = sprite.pivot;
							binData.spritePixelsPerUnit = sprite.pixelsPerUnit;
							binData.spriteExtrude = 0u;
							binData.spriteMeshType = SpriteMeshType.FullRect;
							binData.spriteBorder = sprite.border;
							#endif

						}
						binaries.Add(binData);
						continue;

					}

					#if UNITY_EDITOR
					var guid = item.Key;
					var obj = item.Value;
					var name = guid + ".asset";

					ObjectReference reference = null;
					var fullPath = path + name;
					if (System.IO.File.Exists(fullPath) == false) {

						reference = ME.EditorUtilities.CreateAsset<ObjectReference>(name, path);

					} else {

						reference = UnityEditor.AssetDatabase.LoadAssetAtPath<ObjectReference>(fullPath);

					}

					reference.reference = obj;
					UnityEditor.EditorUtility.SetDirty(reference);
					#endif

				}

				data.binaries = binaries.ToArray();

			}

			public void Free() {

				this.localReferences.Clear();
				this.projectReferences.Clear();
				this.localBinaries.Clear();

				this.deserialized = null;

				if (this.unpackedRoot != null) {

					if (Application.isPlaying == true) {

						GameObject.Destroy(this.unpackedRoot);

					} else {
						
						GameObject.DestroyImmediate(this.unpackedRoot, allowDestroyingAssets: false);

					}

					this.unpackedRoot = null;

				}

			}

			public GameObject Unpack() {

				return this.Unpack(this.deserialized);

			}

			public GameObject Unpack(BundleStructureGameObject data) {

				if (data != null) {

					this.localBinaries.Clear();
					this.localReferences.Clear();

					for (int i = 0; i < data.binaries.Length; ++i) {

						Object obj = null;

						var binData = data.binaries[i];
						if (binData.type == BundleBinaryData.Type.Mesh) {

							obj = MeshSerializer.ReadMesh(Convert.FromBase64String(binData.data));

						} else if (binData.type == BundleBinaryData.Type.Sprite) {

							var textureData = Convert.FromBase64String(binData.data);
							var tex = new Texture2D(1, 1);
							tex.LoadImage(textureData);
							tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
							var sprite = Sprite.Create(tex, binData.spriteRect, binData.spritePivot, binData.spritePixelsPerUnit, binData.spriteExtrude, binData.spriteMeshType, binData.spriteBorder);
							obj = sprite;

						} else if (binData.type == BundleBinaryData.Type.Texture) {
							
							var textureData = Convert.FromBase64String(binData.data);
							var tex = new Texture2D(1, 1);
							tex.LoadImage(textureData);
							tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
							obj = tex;

						}

						binData.data = null;

						this.localBinaries.Add(binData.id, obj);

					}

					this.unpackedRoot = this.Unpack(data, null);
					this.unpackedRoot.hideFlags = HideFlags.HideAndDontSave;
					this.FillGameObject(data, this.unpackedRoot.transform);
					return this.unpackedRoot;

				}

				return null;

			}

			public void FillGameObject(BundleStructureGameObject data, Transform root) {

				var results = ME.ListPool<Component>.Get();
				root.GetComponents(results);
				for (int i = 0; i < data.components.Length; ++i) {

					var component = data.components[i];
					//var type = System.Type.GetType(component.t);
					this.FillComponent(results[i], component.fields);

				}
				ME.ListPool<Component>.Release(results);

				for (int i = 0; i < data.childs.Length; ++i) {

					var child = data.childs[i];
					this.FillGameObject(child, root.GetChild(i));

				}

			}

			public GameObject Unpack(BundleStructureGameObject data, Transform root) {

				var go = new GameObject(data.name);
				go.tag = data.tag;
				go.layer = data.layer;
				go.SetActive(data.active);

				var tr = go.transform;
				tr.SetParent(root);

				for (int i = 0; i < data.components.Length; ++i) {

					var component = data.components[i];
					var type = System.Type.GetType(component.type);
					//Debug.LogWarning("Add: " + type + " :: " + component.type);

					Component componentItem = null;

					if (type == typeof(Transform)) {

						componentItem = go.transform;

					} else {
						
						componentItem = go.AddComponent(type);

					}

					if (
						componentItem is Transform ||
						componentItem is RectTransform) {

						tr = componentItem as Transform;

					}

					this.localReferences.Add(component.instanceId, componentItem);

				}

				for (int i = 0; i < data.childs.Length; ++i) {

					var child = data.childs[i];
					this.Unpack(child, tr);

				}

				return go;

			}

			private object DeserializeValue_INTERNAL(BundleStructureComponent.Field field, BundleStructureComponent.Field.Type fieldType, System.Reflection.FieldInfo fieldInfo = null) {
				
				object value = null;

				if (fieldType == BundleStructureComponent.Field.Type.ValueType) {

					var data = field.data as BundleStructureComponent.Field.DataValueType;

					try {

						if (fieldInfo.FieldType.IsEnum == true) {

							value = Enum.ToObject(fieldInfo.FieldType, int.Parse(data.data));

						} else {

							value = Convert.ChangeType(data.data, fieldInfo.FieldType);

						}

					} catch (Exception) {

						//Debug.LogException(ex);
						value = this.DeserializeObject(data.data, fieldInfo.FieldType);

					}

				} else if (fieldType == BundleStructureComponent.Field.Type.Array) {

					var data = field.data as BundleStructureComponent.Field.DataArray;
					if (data != null) {
					
						if (data.fields != null) {

							var elementType = fieldInfo.FieldType.GetEnumerableType();

							if (fieldInfo.FieldType.IsGenericType == true) {
							
								IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

								//object obj = Activator.CreateInstance(elementType);
								for (int i = 0; i < data.fields.Length; ++i) {

									var item = data.fields[i];
									if (item != null) {

										list.Add(this.DeserializeValue_INTERNAL(item, item.type, fieldInfo));

									} else {

										list.Add(null);

									}

								}

								value = list;

							} else if (fieldInfo.FieldType.IsArray == true) {

								Array filledArray = Array.CreateInstance(elementType, data.fields.Length);
								for (int i = 0; i < data.fields.Length; ++i) {

									var item = data.fields[i];
									if (item != null) {

										filledArray.SetValue(this.DeserializeValue_INTERNAL(item, item.type, fieldInfo), i);

									} else {

										filledArray.SetValue(null, i);

									}

								}

								value = filledArray;

							}

						} else {
						
							value = null;

						}

					}

				} else if (fieldType == BundleStructureComponent.Field.Type.Nested) {

					var data = field.data as BundleStructureComponent.Field.DataNested;
					object instance = null;
					try {

						instance = System.Activator.CreateInstance(System.Type.GetType(data.data.type));

					} catch (Exception) {

						instance = FormatterServices.GetUninitializedObject(System.Type.GetType(data.data.type));

					}

					this.FillComponent(instance, data.data.fields);

					value = instance;

				} else if (fieldType == BundleStructureComponent.Field.Type.Binary) {

					var data = field.data as BundleStructureComponent.Field.DataBinary;

					Object obj;
					if (this.localBinaries.TryGetValue(data.instanceId, out obj) == true) {

						return obj;

					} else {

						throw new Exception("Package Malformed");

					}

				} else if (fieldType == BundleStructureComponent.Field.Type.Reference) {

					var data = field.data as BundleStructureComponent.Field.DataReference;
					if (data.local == true) {

						if (string.IsNullOrEmpty(data.instanceId) == false) {
							
							var instanceId = int.Parse(data.instanceId);
							Object obj;
							if (this.localReferences.TryGetValue(instanceId, out obj) == true) {
								
								if (data.isGameObject == true) {

									obj = (Object)(obj as Transform).gameObject;

								}

								return obj;

							} else {

								throw new Exception(string.Format("Package Malformed: Local reference `{0}` was not found for field `{1}`", instanceId, field.name));

							}

						} else {

							return null;

						}

					} else {

						var rf = UnityEngine.Resources.Load<ObjectReference>(data.instanceId);
						if (rf != null) {

							return rf.reference;

						} else {

							Debug.LogWarning(string.Format("Resource Reference was not found: {0}", data.instanceId));

						}

					}

				} else if (fieldType == BundleStructureComponent.Field.Type.Color) {

					var data = field.data as BundleStructureComponent.Field.DataColor;
					value = new Color32(data.r, data.g, data.b, data.a);

					if (fieldInfo.FieldType == typeof(Color)) {

						value = (Color)(Color32)value;

					}

				} else if (fieldType == BundleStructureComponent.Field.Type.RectOffset) {

					var data = field.data as BundleStructureComponent.Field.DataRectOffset;
					value = new RectOffset(data.left, data.right, data.top, data.bottom);

				} else if (fieldType == BundleStructureComponent.Field.Type.Vector2) {

					var data = field.data as BundleStructureComponent.Field.DataVector2;
					value = new Vector2(data.x, data.y);

				} else if (fieldType == BundleStructureComponent.Field.Type.Vector3) {

					var data = field.data as BundleStructureComponent.Field.DataVector3;
					value = new Vector3(data.x, data.y, data.z);

				} else if (fieldType == BundleStructureComponent.Field.Type.Vector4) {

					var data = field.data as BundleStructureComponent.Field.DataVector4;
					value = new Vector4(data.x, data.y, data.z, data.w);

				} else if (fieldType == BundleStructureComponent.Field.Type.Quaternion) {

					var data = field.data as BundleStructureComponent.Field.DataQuaternion;
					value = new Quaternion(data.x, data.y, data.z, data.w);

				}

				return value;

			}

			private void SetValue_INTERNAL(object component, System.Reflection.FieldInfo fieldInfo, BundleStructureComponent.Field field) {
				
				fieldInfo.SetValue(component, this.DeserializeValue_INTERNAL(field, field.type, fieldInfo));

			}

			private T SetValue_INTERNAL<T>(string name, BundleStructureComponent.Field[] fields, T defaultValue) {

				if (fields != null) {
					
					var field = fields.FirstOrDefault(x => x != null && x.name == name);
					if (field != null && field.data != null) {

						return (T)this.DeserializeValue_INTERNAL(field, field.type);//this.DeserializeObject<T>(field.data.raw);

					}

				}

				return defaultValue;

			}

			private void FillComponent(object component, BundleStructureComponent.Field[] fields) {
				
				var type = component.GetType();

				if (component is RectTransform || component is Transform) {

					if (component is RectTransform) {

						var tr = component as RectTransform;

						tr.localPosition = this.SetValue_INTERNAL("localPosition", fields, Vector3.zero);
						tr.localRotation = this.SetValue_INTERNAL("localRotation", fields, Quaternion.identity);
						tr.localScale = this.SetValue_INTERNAL("localScale", fields, Vector3.one);

						tr.anchoredPosition = this.SetValue_INTERNAL("anchoredPosition", fields, Vector2.zero);
						tr.anchorMin = this.SetValue_INTERNAL("anchorMin", fields, Vector2.zero);
						tr.anchorMax = this.SetValue_INTERNAL("anchorMax", fields, Vector2.one);
						tr.pivot = this.SetValue_INTERNAL("pivot", fields, Vector2.one * 0.5f);
						tr.sizeDelta = this.SetValue_INTERNAL("sizeDelta", fields, Vector2.zero);

					} else if (component is Transform) {

						var tr = component as Transform;

						tr.localPosition = this.SetValue_INTERNAL("localPosition", fields, Vector3.zero);
						tr.localRotation = this.SetValue_INTERNAL("localRotation", fields, Quaternion.identity);
						tr.localScale = this.SetValue_INTERNAL("localScale", fields, Vector3.one);

					}

				} else {

					if (fields == null) return;

					var fieldsInfo = type.GetAllFields();
					for (int i = 0; i < fields.Length; ++i) {

						var field = fields[i];
						if (field == null) continue;

						/*var fieldInfo = fieldsInfo[i];
						//if (fieldInfo.IsNotSerialized == true) continue;

						var attr = fieldInfo.GetCustomAttributes(typeof(SerializeField), inherit: true);
						if ((attr == null || attr.Length == 0) && fieldInfo.IsPublic == false) continue;

						var ignoreAttr = fieldInfo.GetCustomAttributes(typeof(BundleIgnoreAttribute), inherit: true);
						if ((ignoreAttr != null && ignoreAttr.Length != 0)) continue;*/

						//Debug.Log(i + " :: " + field.type + " :: " + (field.data != null ? field.data.GetType().ToString() : null) + " :: " + fieldInfo.Name);

						var fieldInfo = fieldsInfo.FirstOrDefault(x => x.Name == field.name);
						if (fieldInfo != null) {
							
							this.SetValue_INTERNAL(component, fieldInfo, field);

						} else {

							Debug.LogWarningFormat("Field with name `{0}` was not found in {1}", field.name, component);

						}

					}

				}

			}

			public BundleStructureGameObject Pack(GameObject root) {

				#if UNITY_EDITOR
				this.currentBundleName = BundleImporter.FindBundleName(root);
				/*var path = UnityEditor.AssetDatabase.GetAssetPath(root);
				if (string.IsNullOrEmpty(path) == false) {

					var importer = UnityEditor.AssetImporter.GetAtPath(path);
					if (importer != null) this.currentBundleName = importer.assetBundleName;

				}*/
				#endif

				this.projectReferences.Clear();

				#region Local References Cache
				this.localReferences.Clear();
				var componentsRef = root.GetComponentsInChildren<Component>(includeInactive: true);
				for (int i = 0; i < componentsRef.Length; ++i) {

					var objectItem = componentsRef[i];
					//Debug.Log(objectItem.name + " :: " + objectItem + " :: " + objectItem.GetInstanceID());
					this.localReferences.Add(objectItem.GetInstanceID(), objectItem);

				}
				var trsRef = root.GetComponentsInChildren<Transform>(includeInactive: true);
				for (int i = 0; i < trsRef.Length; ++i) {

					var trRef = trsRef[i].gameObject;
					this.localReferences.Add(trRef.GetInstanceID(), trRef);

				}
				#endregion

				var data = this.Pack_INTERNAL(root, isRoot: true);
				this.BuildProjectReferencesMap(data);

				return data;

			}

			private BundleStructureGameObject Pack_INTERNAL(GameObject root, bool isRoot) {

				var obj = (isRoot == true ? new BundleStructureGameObjectRoot() : new BundleStructureGameObject());

				obj.name = root.name;
				obj.tag = root.tag;
				obj.layer = root.layer;
				obj.active = root.activeSelf;

				var components = ME.ListPool<Component>.Get();
				root.GetComponents(components);

				obj.components = new BundleStructureComponent[components.Count];
				for (int i = 0; i < components.Count; ++i) {

					obj.components[i] = this.PackComponent(components[i]);

				}

				ME.ListPool<Component>.Release(components);

				var tr = root.transform;
				var count = tr.childCount;
				obj.childs = new BundleStructureGameObject[count];
				for (int i = 0; i < count; ++i) {

					var child = tr.GetChild(i).gameObject;
					obj.childs[i] = this.Pack_INTERNAL(child, isRoot: false);

				}

				return obj;

			}

			public BundleStructureComponent PackComponent(Component component) {

				var item = new BundleStructureComponent();
				var type = component.GetType();
				item.type = this.GetTypeString(type);
				item.instanceId = component.GetInstanceID();

				//Debug.Log("Pack: " + component.GetType());
				if (component is RectTransform || component is Transform) {

					if (component is RectTransform) {

						var tr = component as RectTransform;
						item.fields = new BundleStructureComponent.Field[8];

						this.SetFieldValue_INTERNAL("localPosition", item.fields, 0, tr.localPosition, Vector3.zero);
						this.SetFieldValue_INTERNAL("localRotation", item.fields, 1, tr.localRotation, Quaternion.identity);
						this.SetFieldValue_INTERNAL("localScale", item.fields, 2, tr.localScale, Vector3.one);

						this.SetFieldValue_INTERNAL("anchoredPosition", item.fields, 3, tr.anchoredPosition, Vector2.zero);
						this.SetFieldValue_INTERNAL("anchorMin", item.fields, 4, tr.anchorMin, Vector2.zero);
						this.SetFieldValue_INTERNAL("anchorMax", item.fields, 5, tr.anchorMax, Vector2.one);
						this.SetFieldValue_INTERNAL("pivot", item.fields, 6, tr.pivot, Vector2.one * 0.5f);
						this.SetFieldValue_INTERNAL("sizeDelta", item.fields, 7, tr.sizeDelta, Vector2.zero);

						this.StripFields(ref item.fields);

					} else if (component is Transform) {

						var tr = component as Transform;
						item.fields = new BundleStructureComponent.Field[3];

						this.SetFieldValue_INTERNAL("localPosition", item.fields, 0, tr.localPosition, Vector3.zero);
						this.SetFieldValue_INTERNAL("localRotation", item.fields, 1, tr.localRotation, Quaternion.identity);
						this.SetFieldValue_INTERNAL("localScale", item.fields, 2, tr.localScale, Vector3.one);

						this.StripFields(ref item.fields);

					}

				} else {

					this.FillNested(component, item);

				}

				return item;

			}

			private void StripFields(ref BundleStructureComponent.Field[] fields) {

				var lastNotNullIndex = -1;
				var allIsNull = true;
				for (int i = 0; i < fields.Length; ++i) {

					var field = fields[i];
					if (field != null && field.type != BundleStructureComponent.Field.Type.DefaultValue) {

						allIsNull = false;
						lastNotNullIndex = i;

					}

				}

				if (allIsNull == true) {

					fields = null;

				} else {

					System.Array.Resize(ref fields, lastNotNullIndex + 1);

				}

			}

			private void StripField(BundleStructureComponent.Field[] fields, int index) {
				
				var field = fields[index];
				if (field == null || field.type == BundleStructureComponent.Field.Type.DefaultValue) {

					fields[index] = null;

				} else if (field != null && field.data == null) {

					fields[index] = null;

				}

			}

			private void FillNested(object component, BundleStructureComponent item) {

				var type = component.GetType();
				item.type = this.GetTypeString(type);

				object defaultInstance = null;
				if (component is Component) {

					var go = new GameObject();
					defaultInstance = go.AddComponent(type);
					if (Application.isPlaying == false) {

						GameObject.DestroyImmediate(go, allowDestroyingAssets: false);

					} else {
						
						GameObject.Destroy(go);

					}

				} else {
					
					defaultInstance = FormatterServices.GetUninitializedObject(type);

				}

				var fields = type.GetAllFields();
				item.fields = new BundleStructureComponent.Field[fields.Length];
				for (int i = 0; i < fields.Length; ++i) {

					var field = fields[i];
					//if (field.IsNotSerialized == true) continue;
					var attr = field.GetCustomAttributes(typeof(SerializeField), inherit: true);
					if ((attr == null || attr.Length == 0) && field.IsPublic == false) continue;

					var ignoreAttr = field.GetCustomAttributes(typeof(BundleIgnoreAttribute), inherit: true);
					if ((ignoreAttr != null && ignoreAttr.Length != 0)) continue;

					this.SetFieldValue_INTERNAL(field.Name, item.fields, i, field.GetValue(component), field.GetValue(defaultInstance));

				}

				this.StripFields(ref item.fields);

			}

			private bool CheckDefault(object value, object defaultValue, BundleStructureComponent.Field[] fields, int index) {

				var serializedValue = value.ToString();
				var serializedDefaultValue = defaultValue.ToString();
				if (serializedValue == serializedDefaultValue) {

					fields[index].type = BundleStructureComponent.Field.Type.DefaultValue;
					this.StripField(fields, index);
					return true;

				}

				return false;

			}

			private void SetFieldValue_INTERNAL(string name, BundleStructureComponent.Field[] fields, int index, object value, object defaultValue) {

				fields[index] = new BundleStructureComponent.Field();
				fields[index].name = name;

				var isNull = (value == null || (value is Object && ((value as Object) == null || (value as Object).GetInstanceID() == 0)));
				if ((value is bool) == false && isNull == true) {
					
					if (defaultValue == null) {

						fields[index].type = BundleStructureComponent.Field.Type.DefaultValue;
						this.StripField(fields, index);
						return;

					}

					fields[index].type = BundleStructureComponent.Field.Type.Reference;
					var d = new BundleStructureComponent.Field.DataReference();
					d.instanceId = "0";
					fields[index].data = d;

				} else {

					var type = value.GetType();
					if (type == typeof(Color) ||
						type == typeof(Color32)) {

						if (this.CheckDefault(value, defaultValue, fields, index) == true) return;

						Color32 color = new Color32();
						if (type == typeof(Color)) {

							color = (Color32)(Color)value;

						} else {

							color = (Color32)value;

						}

						fields[index].type = BundleStructureComponent.Field.Type.Color;
						var d = new BundleStructureComponent.Field.DataColor();
						d.r = color.r;
						d.g = color.g;
						d.b = color.b;
						d.a = color.a;
						fields[index].data = d;

					} else if (type == typeof(RectOffset)) {

						if (this.CheckDefault(value, defaultValue, fields, index) == true) return;

						var val = (RectOffset)value;

						fields[index].type = BundleStructureComponent.Field.Type.RectOffset;
						var d = new BundleStructureComponent.Field.DataRectOffset();
						d.top = val.top;
						d.bottom = val.bottom;
						d.left = val.left;
						d.right = val.right;
						fields[index].data = d;

					} else if (type == typeof(Vector2)) {

						if (this.CheckDefault(value, defaultValue, fields, index) == true) return;

						var val = (Vector2)value;

						fields[index].type = BundleStructureComponent.Field.Type.Vector2;
						var d = new BundleStructureComponent.Field.DataVector2();
						d.x = val.x;
						d.y = val.y;
						fields[index].data = d;

					} else if (type == typeof(Vector3)) {

						if (this.CheckDefault(value, defaultValue, fields, index) == true) return;

						var val = (Vector3)value;

						fields[index].type = BundleStructureComponent.Field.Type.Vector3;
						var d = new BundleStructureComponent.Field.DataVector3();
						d.x = val.x;
						d.y = val.y;
						d.z = val.z;
						fields[index].data = d;

					} else if (type == typeof(Vector4)) {

						if (this.CheckDefault(value, defaultValue, fields, index) == true) return;

						var val = (Vector4)value;

						fields[index].type = BundleStructureComponent.Field.Type.Vector4;
						var d = new BundleStructureComponent.Field.DataVector4();
						d.x = val.x;
						d.y = val.y;
						d.z = val.z;
						d.w = val.w;
						fields[index].data = d;

					} else if (type == typeof(Quaternion)) {

						if (this.CheckDefault(value, defaultValue, fields, index) == true) return;

						var val = (Quaternion)value;

						fields[index].type = BundleStructureComponent.Field.Type.Quaternion;
						var d = new BundleStructureComponent.Field.DataQuaternion();
						d.x = val.x;
						d.y = val.y;
						d.z = val.z;
						d.w = val.w;
						fields[index].data = d;

					} else if (value is ME.Events.ISimpleEvent) {

						fields[index] = null;

					} else if (value.IsArray() == true) {
						
						fields[index].type = BundleStructureComponent.Field.Type.Array;
						var d = new BundleStructureComponent.Field.DataArray();

						var count = 0;
						var enumerator = ((IEnumerable)value).GetEnumerator();
						while (enumerator.MoveNext() == true) {

							++count;

						}

						if (count > 0) {

							enumerator.Reset();
							d.fields = new BundleStructureComponent.Field[count];
							count = 0;
							while (enumerator.MoveNext() == true) {
								
								this.SetFieldValue_INTERNAL(null, d.fields, count, enumerator.Current, null);
								++count;

							}

							//d.raw = this.SerializeObject(d.f);

						}

						/*
						var arr = (isList == true) ? ((IList)value).GetEnumerator() : (System.Array)value;
						if (arr.Length > 0) {

							d.items = new BundleStructureComponent.Field[arr.Length];
							for (int i = 0; i < arr.Length; ++i) {

								this.SetFieldValue_INTERNAL(d.items, i, arr.GetValue(i), null);

							}

							d.raw = this.SerializeObject(d.items);

						}*/

						fields[index].data = d;

						#region Strip
						if (d.fields == null || d.fields.Length == 0) fields[index].data = null;
						this.StripField(fields, index);
						#endregion

					} else if (
						(type.IsClass == true || type.IsValueType == true) &&
						type.IsSerializable == true &&
						type.IsSimpleType() == false &&
						type.IsEnum == false) {

						fields[index].type = BundleStructureComponent.Field.Type.Nested;
						var d = new BundleStructureComponent.Field.DataNested();

						//Debug.LogError("Nested: " + type.ToString());
						d.data = new BundleStructureComponent();
						this.FillNested(value, d.data);
						fields[index].data = d;

						#region Strip
						if (d.data == null || d.data.fields == null) fields[index].data = null;
						this.StripField(fields, index);
						#endregion

					} else if (
						type.IsSimpleType() == true ||
						type.IsValueType == true ||
						type.IsEnum == true) {

						if (type.IsEnum == true) {

							value = Convert.ChangeType(value, typeof(int));

						}

						if (defaultValue == null && type == typeof(string)) {

							defaultValue = string.Empty;

						}

						var serializedValue = value.ToString();//this.SerializeObject(value);
						var serializedDefaultValue = defaultValue.ToString();//this.SerializeObject(defaultValue);
						if ((value is bool) == false && serializedValue == serializedDefaultValue) {
							
							fields[index].type = BundleStructureComponent.Field.Type.DefaultValue;
							this.StripField(fields, index);
							return;

						} else {

							//Debug.LogWarning("Values: " + serializedValue + " :: " + serializedDefaultValue);

						}

						fields[index].type = BundleStructureComponent.Field.Type.ValueType;
						var d = new BundleStructureComponent.Field.DataValueType();
						d.data = serializedValue;
						fields[index].data = d;

					} else {

						//Debug.LogWarning(name + ", Ref: " + value + " == " + defaultValue + " :: " + this.IsLocal(value as Object) + " :: " + type);

						fields[index].type = BundleStructureComponent.Field.Type.Reference;
						var d = new BundleStructureComponent.Field.DataReference();
						d.local = this.IsLocal(value as Object);
						if (d.local == true) {

							if (value is GameObject) {
								
								value = (value as GameObject).transform;
								d.isGameObject = true;

							}

							d.instanceId = (value as Object).GetInstanceID().ToString();

						} else {

							#if UNITY_EDITOR
							d.instanceId = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(value as Object));
							this.RegisterProjectReference(d.instanceId, value as Object);
							#endif

						}

						if (this.IsBinary(value as Object) == true) {

							fields[index].type = BundleStructureComponent.Field.Type.Binary;
							var bin = new BundleStructureComponent.Field.DataBinary();
							bin.instanceId = d.instanceId;
							fields[index].data = bin;

						} else {

							fields[index].data = d;

						}

					}

				}

			}

			public void RegisterProjectReference(string guid, Object obj) {

				Object val;
				if (this.projectReferences.TryGetValue(guid, out val) == false) {

					this.projectReferences.Add(guid, obj);

				} else {

					this.projectReferences[guid] = obj;

				}

			}

			public bool IsBinary(Object obj) {

				var dataInBundle = false;

				#if UNITY_EDITOR
				var assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
				var importer = UnityEditor.AssetImporter.GetAtPath(assetPath);
				if (importer != null && string.IsNullOrEmpty(importer.assetBundleName) == false) {

					//Debug.LogWarning(assetPath + " :: " + importer.assetBundleName + " == " + this.currentBundleName);
					dataInBundle = (importer.assetBundleName == this.currentBundleName);

				}
				//Debug.Log(assetPath + " :: " + (obj is Mesh) + " :: " + (obj is Texture) + " :: " + obj + " :: " + dataInBundle);
				#endif

				return
					dataInBundle == true &&
					(obj is Mesh || obj is Texture || obj is Sprite);

			}

			public bool IsLocal(Object obj) {

				if (obj == null) return true;

				return this.localReferences.ContainsKey(obj.GetInstanceID());

			}

			private string GetTypeString(System.Type type) {

				return type.FullName + ", " + type.Assembly.FullName;

			}

		}

	}

	public static class TypeExtensions {
		/// <summary>
		/// Determine whether a type is simple (String, Decimal, DateTime, etc) 
		/// or complex (i.e. custom class with public properties and methods).
		/// </summary>
		/// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
		public static bool IsSimpleType(this Type type) {
			
			return
				type.IsPrimitive ||
				new Type[] { 
					typeof(String),
					typeof(Decimal),
					typeof(DateTime),
					typeof(DateTimeOffset),
					typeof(TimeSpan),
					typeof(Guid)
				}.Contains(type) ||
				Convert.GetTypeCode(type) != TypeCode.Object;

		}

		public static bool IsArray(this object o) {

			bool isGenericList = false;
			var oType = o.GetType();
			if (oType.IsArray == true) return true;
			if (oType is IEnumerable == true) return true;
			if (oType is IList == true) return true;
			if (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>))) isGenericList = true;

			return isGenericList;

		}

		public static bool IsGenericList(this object o) {
			
			bool isGenericList = false;
			var oType = o.GetType();
			if (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>))) isGenericList = true;

			return isGenericList;

		}

		public static bool IsEnumerableType(this Type type) {
			
			return (type.GetInterface("IEnumerable") != null);

		}

		public static Type GetEnumerableType(this Type type) {
			
			if (type == null)
				throw new ArgumentNullException("type");

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				return type.GetGenericArguments()[0];

			var iface = (from i in type.GetInterfaces()
				where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
				select i).FirstOrDefault();

			if (iface == null)
				throw new ArgumentException("Does not represent an enumerable type.", "type");

			return GetEnumerableType(iface);

		}

		public static FieldInfo[] GetAllFields(this Type t) {
			
			if (t == null) return new FieldInfo[0];

			BindingFlags flags = 
				BindingFlags.Public |
				BindingFlags.NonPublic | 
				BindingFlags.Instance | 
				BindingFlags.DeclaredOnly;

			var fields = t.GetFields(flags);
			var baseFields = t.BaseType.GetAllFields();
			return fields.Concat(baseFields).ToArray();

		}

	}

	public static class Zip {

		/*public static byte[] Compress(byte[] data) {
			using (var compressedStream = new MemoryStream()) {
				using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress)) {
					zipStream.Write(data, 0, data.Length);
					zipStream.Close();
					return compressedStream.ToArray();
				}
			}

		}

		public static byte[] Decompress(byte[] data) {
			
			using (var compressedStream = new MemoryStream(data)) {
				using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress)) {
					compressedStream.Read(data, 0, data.Length);
					return data;
				}
			}

		}*/

		public static void CopyTo(Stream src, Stream dest) {
			byte[] bytes = new byte[4096];

			int cnt;

			while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
				dest.Write(bytes, 0, cnt);
			}
		}

		public static byte[] Compress(string str) {
			var bytes = System.Text.Encoding.UTF8.GetBytes(str);

			using (var msi = new MemoryStream(bytes))
				using (var mso = new MemoryStream()) {
					using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
						//msi.CopyTo(gs);
						CopyTo(msi, gs);
					}

					return mso.ToArray();
				}
		}

		public static string Decompress(byte[] bytes) {
			using (var msi = new MemoryStream(bytes))
				using (var mso = new MemoryStream()) {
					using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
						//gs.CopyTo(mso);
						CopyTo(gs, mso);
					}

					return System.Text.Encoding.UTF8.GetString(mso.ToArray());
				}
		}

	}

	public class BundleImporter {
		
		public static string FindBundleName(Object obj) {

			#if UNITY_EDITOR
			var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
			var sPath = path.Split(System.IO.Path.DirectorySeparatorChar);
			if (sPath != null && sPath.Length > 0) {
				
				var rootPath = sPath[0];
				for (int i = 1; i < sPath.Length; ++i) {

					var importer = UnityEditor.AssetImporter.GetAtPath(rootPath);
					if (importer != null) {

						if (string.IsNullOrEmpty(importer.assetBundleName) == false) {

							return importer.assetBundleName;

						}

					}

					rootPath += System.IO.Path.DirectorySeparatorChar + sPath[i];

				}

			}
			#endif

			return string.Empty;

		}

	}

	public class MeshSerializer {
		// A simple mesh saving/loading functionality.
		// This is a utility script, you don't need to add it to any objects.
		// See SaveMeshForWeb and LoadMeshFromWeb for example of use.
		//
		// Uses a custom binary format:
		//
		//    2 bytes vertex count
		//    2 bytes triangle count
		//    1 bytes vertex format (bits: 0=vertices, 1=normals, 2=tangents, 3=uvs)
		//
		//    After that come vertex component arrays, each optional except for positions.
		//    Which ones are present depends on vertex format:
		//        Positions
		//            Bounding box is before the array (xmin,xmax,ymin,ymax,zmin,zmax)
		//            Then each vertex component is 2 byte unsigned short, interpolated between the bound axis
		//        Normals
		//            One byte per component
		//        Tangents
		//            One byte per component
		//        UVs (8 bytes/vertex - 2 floats)
		//            Bounding box is before the array (xmin,xmax,ymin,ymax)
		//            Then each UV component is 2 byte unsigned short, interpolated between the bound axis
		//
		//    Finally the triangle indices array: 6 bytes per triangle (3 unsigned short indices)
		// Reads mesh from an array of bytes. [old: Can return null if the bytes seem invalid.]
		public static Mesh ReadMesh(byte[] bytes)
		{
			if (bytes == null || bytes.Length < 5)
				throw new Exception("Invalid mesh file!");

			var buf = new BinaryReader(new MemoryStream(bytes));

			// read header
			var vertCount = buf.ReadUInt16();
			var triCount = buf.ReadUInt16();
			var format = buf.ReadByte();

			// sanity check
			if (vertCount < 0 || vertCount > 64000)
				throw new Exception("Invalid vertex count in the mesh data!");
			if (triCount < 0 || triCount > 64000)
				throw new Exception("Invalid triangle count in the mesh data!");
			if (format < 1 || (format & 1) == 0 || format > 15)
				throw new Exception("Invalid vertex format in the mesh data!");

			var mesh = new Mesh();
			int i;

			// positions
			var verts = new Vector3[vertCount];
			ReadVector3Array16Bit(verts, buf);
			mesh.vertices = verts;

			if ((format & 2) != 0) // have normals
			{
				var normals = new Vector3[vertCount];
				ReadVector3ArrayBytes(normals, buf);
				mesh.normals = normals;
			}

			if ((format & 4) != 0) // have tangents
			{
				var tangents = new Vector4[vertCount];
				ReadVector4ArrayBytes(tangents, buf);
				mesh.tangents = tangents;
			}

			if ((format & 8) != 0) // have UVs
			{
				var uvs = new Vector2[vertCount];
				ReadVector2Array16Bit(uvs, buf);
				mesh.uv = uvs;
			}

			// triangle indices
			var tris = new int[triCount * 3];
			for (i = 0; i < triCount; ++i)
			{
				tris[i * 3 + 0] = buf.ReadUInt16();
				tris[i * 3 + 1] = buf.ReadUInt16();
				tris[i * 3 + 2] = buf.ReadUInt16();
			}
			mesh.triangles = tris;

			buf.Close();

			return mesh;
		}

		static void ReadVector3Array16Bit(Vector3[] arr, BinaryReader buf)
		{
			var n = arr.Length;
			if (n == 0)
				return;

			// read bounding box
			Vector3 bmin;
			Vector3 bmax;
			bmin.x = buf.ReadSingle();
			bmax.x = buf.ReadSingle();
			bmin.y = buf.ReadSingle();
			bmax.y = buf.ReadSingle();
			bmin.z = buf.ReadSingle();
			bmax.z = buf.ReadSingle();

			// decode vectors as 16 bit integer components between the bounds
			for (var i = 0; i < n; ++i)
			{
				ushort ix = buf.ReadUInt16();
				ushort iy = buf.ReadUInt16();
				ushort iz = buf.ReadUInt16();
				float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
				float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
				float zz = iz / 65535.0f * (bmax.z - bmin.z) + bmin.z;
				arr[i] = new Vector3(xx, yy, zz);
			}
		}
		static void WriteVector3Array16Bit(Vector3[] arr, BinaryWriter buf)
		{
			if (arr.Length == 0)
				return;

			// calculate bounding box of the array
			var bounds = new Bounds(arr[0], new Vector3(0.001f, 0.001f, 0.001f));
			foreach (var v in arr)
				bounds.Encapsulate(v);

			// write bounds to stream
			var bmin = bounds.min;
			var bmax = bounds.max;
			buf.Write(bmin.x);
			buf.Write(bmax.x);
			buf.Write(bmin.y);
			buf.Write(bmax.y);
			buf.Write(bmin.z);
			buf.Write(bmax.z);

			// encode vectors as 16 bit integer components between the bounds
			foreach (var v in arr)
			{
				var xx = Mathf.Clamp((v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f, 0.0f, 65535.0f);
				var yy = Mathf.Clamp((v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f, 0.0f, 65535.0f);
				var zz = Mathf.Clamp((v.z - bmin.z) / (bmax.z - bmin.z) * 65535.0f, 0.0f, 65535.0f);
				var ix = (ushort)xx;
				var iy = (ushort)yy;
				var iz = (ushort)zz;
				buf.Write(ix);
				buf.Write(iy);
				buf.Write(iz);
			}
		}
		static void ReadVector2Array16Bit(Vector2[] arr, BinaryReader buf)
		{
			var n = arr.Length;
			if (n == 0)
				return;

			// Read bounding box
			Vector2 bmin;
			Vector2 bmax;
			bmin.x = buf.ReadSingle();
			bmax.x = buf.ReadSingle();
			bmin.y = buf.ReadSingle();
			bmax.y = buf.ReadSingle();

			// Decode vectors as 16 bit integer components between the bounds
			for (var i = 0; i < n; ++i)
			{
				ushort ix = buf.ReadUInt16();
				ushort iy = buf.ReadUInt16();
				float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
				float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
				arr[i] = new Vector2(xx, yy);
			}
		}
		static void WriteVector2Array16Bit(Vector2[] arr, BinaryWriter buf)
		{
			if (arr.Length == 0)
				return;

			// Calculate bounding box of the array
			Vector2 bmin = arr[0] - new Vector2(0.001f, 0.001f);
			Vector2 bmax = arr[0] + new Vector2(0.001f, 0.001f);
			foreach (var v in arr)
			{
				bmin.x = Mathf.Min(bmin.x, v.x);
				bmin.y = Mathf.Min(bmin.y, v.y);
				bmax.x = Mathf.Max(bmax.x, v.x);
				bmax.y = Mathf.Max(bmax.y, v.y);
			}

			// Write bounds to stream
			buf.Write(bmin.x);
			buf.Write(bmax.x);
			buf.Write(bmin.y);
			buf.Write(bmax.y);

			// Encode vectors as 16 bit integer components between the bounds
			foreach (var v in arr)
			{
				var xx = (v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f;
				var yy = (v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f;
				var ix = (ushort)xx;
				var iy = (ushort)yy;
				buf.Write(ix);
				buf.Write(iy);
			}
		}

		static void ReadVector3ArrayBytes(Vector3[] arr, BinaryReader buf)
		{
			// decode vectors as 8 bit integers components in -1.0f .. 1.0f range
			var n = arr.Length;
			for (var i = 0; i < n; ++i)
			{
				byte ix = buf.ReadByte();
				byte iy = buf.ReadByte();
				byte iz = buf.ReadByte();
				float xx = (ix - 128.0f) / 127.0f;
				float yy = (iy - 128.0f) / 127.0f;
				float zz = (iz - 128.0f) / 127.0f;
				arr[i] = new Vector3(xx, yy, zz);
			}
		}
		static void WriteVector3ArrayBytes(Vector3[] arr, BinaryWriter buf)
		{
			// encode vectors as 8 bit integers components in -1.0f .. 1.0f range
			foreach (var v in arr)
			{
				var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
				var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
				var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
				buf.Write(ix);
				buf.Write(iy);
				buf.Write(iz);
			}
		}

		static void ReadVector4ArrayBytes(Vector4[] arr, BinaryReader buf)
		{
			// Decode vectors as 8 bit integers components in -1.0f .. 1.0f range
			var n = arr.Length;
			for (var i = 0; i < n; ++i)
			{
				byte ix = buf.ReadByte();
				byte iy = buf.ReadByte();
				byte iz = buf.ReadByte();
				byte iw = buf.ReadByte();
				float xx = (ix - 128.0f) / 127.0f;
				float yy = (iy - 128.0f) / 127.0f;
				float zz = (iz - 128.0f) / 127.0f;
				float ww = (iw - 128.0f) / 127.0f;
				arr[i] = new Vector4(xx, yy, zz, ww);
			}
		}
		static void WriteVector4ArrayBytes(Vector4[] arr, BinaryWriter buf)
		{
			// Encode vectors as 8 bit integers components in -1.0f .. 1.0f range
			foreach (var v in arr)
			{
				var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
				var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
				var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
				var iw = (byte)Mathf.Clamp(v.w * 127.0f + 128.0f, 0.0f, 255.0f);
				buf.Write(ix);
				buf.Write(iy);
				buf.Write(iz);
				buf.Write(iw);
			}
		}

		// Writes mesh to an array of bytes.
		public static byte[] WriteMesh(Mesh mesh, bool saveTangents)
		{
			if (!mesh)
				throw new Exception("No mesh given!");

			var verts = mesh.vertices;
			var normals = mesh.normals;
			var tangents = mesh.tangents;
			var uvs = mesh.uv;
			var tris = mesh.triangles;

			// figure out vertex format
			byte format = 1;
			if (normals.Length > 0)
				format |= 2;
			if (saveTangents && tangents.Length > 0)
				format |= 4;
			if (uvs.Length > 0)
				format |= 8;

			var stream = new MemoryStream();
			var buf = new BinaryWriter(stream);

			// write header
			var vertCount = (ushort)verts.Length;
			var triCount = (ushort)(tris.Length / 3);
			buf.Write(vertCount);
			buf.Write(triCount);
			buf.Write(format);
			// vertex components
			WriteVector3Array16Bit(verts, buf);
			WriteVector3ArrayBytes(normals, buf);
			if (saveTangents)
				WriteVector4ArrayBytes(tangents, buf);
			WriteVector2Array16Bit(uvs, buf);
			// triangle indices
			foreach (var idx in tris)
			{
				var idx16 = (ushort)idx;
				buf.Write(idx16);
			}
			buf.Close();

			return stream.ToArray();
		}
	}

}