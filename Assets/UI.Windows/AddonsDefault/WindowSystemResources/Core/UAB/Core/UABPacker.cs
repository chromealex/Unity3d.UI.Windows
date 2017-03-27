using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ME.UAB.Extensions;

namespace ME.UAB {
	
	public class UABPacker {
		
		private struct TempReferencePack {

			public UABField uField;
			public object value;

		};

		private struct TempBinaryPack {

			public UABField uField;
			public object value;

		};

		private string currentBundleName = null;
		private List<TempReferencePack> tempReferencesPack = null;
		private List<TempBinaryPack> tempBinariesPack = null;
		#pragma warning disable
		private UABConfig config = null;
		#pragma warning restore

		public UABPackage Run(GameObject[] objects, UABConfig config = null, List<ISerializer> serializers = null) {

			#if UNITY_EDITOR
			if (config == null) config = Builder.GetDefaultConfig(required: true);
			#endif
			if (serializers == null) serializers = Builder.GetAllSerializers();

			this.Free();

			this.tempReferencesPack = new List<TempReferencePack>();
			this.tempBinariesPack = new List<TempBinaryPack>();
			this.config = config;

			var result = new UABPackage();
			// Pack GameObjects
			{
				result.objects = new UABGameObject[objects.Length];
				for (int i = 0; i < objects.Length; ++i) {

					this.tempReferencesPack.Clear();

					var obj = objects[i];
					this.currentBundleName = BundleImporter.GetBundleName(obj);
					var uGo = this.Pack(obj, serializers);
					result.objects[i] = uGo;

					this.BuildReferences(obj.transform);

				}
			}

			this.BuildBinaries(ref result.binaryHeaders, ref result.binaryData, serializers);

			this.Free();
			return result;

		}

		public void Free() {

			this.currentBundleName = null;

			if (this.tempReferencesPack != null) this.tempReferencesPack.Clear();
			this.tempReferencesPack = null;

			if (this.tempBinariesPack != null) this.tempBinariesPack.Clear();
			this.tempBinariesPack = null;

			this.config = null;

		}

		#region Pack
		private UABGameObject Pack(GameObject root, List<ISerializer> serializers) {
			
			var uGo = new UABGameObject();
			uGo.name = root.name;
			uGo.tag = root.tag;
			uGo.layer = root.layer;
			uGo.active = root.activeSelf;

			var childCount = root.transform.childCount;
			uGo.childs = new UABGameObject[childCount];
			for (int i = 0; i < childCount; ++i) {

				var uChild = this.Pack(root.transform.GetChild(i).gameObject, serializers);
				uGo.childs[i] = uChild;

			}

			var components = root.GetComponents<Component>();
			uGo.components = new UABComponent[components.Length];
			for (int i = 0; i < components.Length; ++i) {

				uGo.components[i] = this.Pack(components[i], serializers);

			}

			return uGo;

		}

		private UABComponent Pack(Component component, List<ISerializer> serializers) {

			var uComponent = new UABComponent();
			uComponent.instanceId = component.GetInstanceID();
			uComponent.type = Builder.GetTypeString(component.GetType());

			var found = false;
			for (int j = 0; j < serializers.Count; ++j) {

				if (serializers[j].IsValid(component) == true) {

					var field = new UABField();
					var componentObj = (object)component;
					serializers[j].Serialize(this, field, ref componentObj, serializers);
					uComponent.fields = field.fields;
					found = true;
					break;

				}

			}

			if (found == false) {

				var uFields = this.Serialize(component, serializers);
				uComponent.fields = uFields;

			}

			return uComponent;

		}
		#endregion

		public void BuildBinaries(ref UABBinaryHeader[] headers, ref UABBinaryData[] data, List<ISerializer> serializers) {

			var binData = ListPool<UABBinaryData>.Get();
			var binHeaders = ListPool<UABBinaryHeader>.Get();

			var list = this.tempBinariesPack;
			for (int i = 0; i < list.Count; ++i) {

				var uField = list[i].uField;
				var value = list[i].value;

				var headerId = string.Empty;
				var found = false;

				#if UNITY_EDITOR
				var instanceId = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(value as Object));

				for (int j = 0; j < serializers.Count; ++j) {

					if (serializers[j].IsValid(value) == true) {

						if (binData.Any(x => x.instanceId == instanceId) == false) {

							var f = new UABField();
							serializers[j].Serialize(null, f, ref value, serializers);
							binData.Add(new UABBinaryData() { instanceId = instanceId, data = f.data });

						}

						var field = new UABField();
						serializers[j].Serialize(this, field, ref value, serializers);

						var header = new UABBinaryHeader() { binDataInstanceId = instanceId };
						header.instanceId = header.GetHashCode().ToString();
						header.field = field;

						headerId = header.instanceId;

						if (binHeaders.Any(x => x.instanceId == header.instanceId) == false) binHeaders.Add(header);

						found = true;

						break;

					}

				}
				#endif

				uField.serializatorId = -3;
				uField.data = UABSerializer.SerializeValueType(new UABBinary() { instanceId = headerId });

				if (found == false) {

					throw new UnityException(string.Format("Binary Serializer was not found for type `{0}`. Package cannot be processed.", value.GetType().ToString()));

				}

			}
			list.Clear();

			data = binData.ToArray();
			ListPool<UABBinaryData>.Release(binData);

			headers = binHeaders.ToArray();
			ListPool<UABBinaryHeader>.Release(binHeaders);

		}

		public void BuildReferences(Transform root) {

			var list = this.tempReferencesPack;
			for (int i = 0; i < list.Count; ++i) {

				var uField = list[i].uField;
				var value = list[i].value;

				var isObjectNull = (value as UnityEngine.Object) == null;
				var instanceId = Builder.GetInstanceID(value).ToString();
				var isLocal = (isObjectNull == false ? Builder.GetRoot(value) == root : true);
				var isGameObject = (value is GameObject);
				var postInstanceId = string.Empty;

				//Debug.Log(Builder.GetRoot(value) + " :: " + root);

				var resultInstanceId = string.Empty;

				if (isLocal == false && isObjectNull == false) {

					// Add resource link
					#if UNITY_EDITOR
					var source = value as Object;
					var assetPath = UnityEditor.AssetDatabase.GetAssetPath(source);

					instanceId = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);

					if (assetPath.StartsWith("Assets/") == false) {

						// Unity default resource
						instanceId = "_" + value.GetType().FullName + "." + (value as Object).name;

					} else {

						var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetPath);
						if (obj != source) {
							
							if (obj is GameObject) {

								// Object is storing inside
								// Finding local asset path
								postInstanceId = ME.UAB.Extensions.EditorUtilities.GetLocalIdentfierInFile(source).ToString();

							} else {

								var allObjects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
								var objectInside = allObjects.FirstOrDefault(x => x.name == source.name);
								if (objectInside != null) {
									
									var index = System.Array.IndexOf(allObjects, objectInside);
									if (index >= 0) {

										instanceId = string.Format("{0}_{1}", UnityEditor.AssetDatabase.AssetPathToGUID(assetPath), index);
										postInstanceId = "inner";

									} else {

										throw new UnityException(string.Format("Object with the name `{0}` was not found in `{1}`. Package cannot be processed.", objectInside.name, assetPath));

									}

								} else {

									throw new UnityException(string.Format("Object was not found in `{0}`. Package cannot be processed.", assetPath));

								}

							}

						} else {

							postInstanceId = "root";

						}

					}

					var localPath = this.config.UAB_RESOURCES_PATH;
					var projectPath = "Assets/" + localPath;

					{ // Creating `Resources` directory
						
						var path = Application.dataPath + "/" + localPath;
						if (System.IO.Directory.Exists(path) == false) {

							System.IO.Directory.CreateDirectory(path);
							UnityEditor.AssetDatabase.ImportAsset(projectPath);

						}

					}

					{ // Creating asset by GUID
						
						var guid = instanceId + "_" + postInstanceId;
						var name = guid + ".asset";

						ObjectReference reference = null;
						var fullPath = projectPath + "/" + name;
						if (System.IO.File.Exists(fullPath) == false) {

							reference = ME.UAB.Extensions.EditorUtilities.CreateAsset<ObjectReference>(name, projectPath);

						} else {

							reference = UnityEditor.AssetDatabase.LoadAssetAtPath<ObjectReference>(fullPath);

						}

						reference.reference = source;
						UnityEditor.EditorUtility.SetDirty(reference);

					}
					#endif

					resultInstanceId = string.Format("{0}_{1}", instanceId, postInstanceId);

				} else {

					if (isGameObject == true) {

						var go = (value as GameObject);
						instanceId = Builder.GetInstanceID(go == null ? null : go.transform).ToString();

					}

					resultInstanceId = instanceId;

				}

				uField.serializatorId = -2;
				uField.data = UABSerializer.SerializeValueType(new UABReference() { instanceId = resultInstanceId, isLocal = isLocal, isGameObject = isGameObject });

			}

			list.Clear();

		}

		public void RegisterReferencePack(UABField uField, object value) {

			this.tempReferencesPack.Add(new TempReferencePack() { uField = uField, value = value });

		}

		public void RegisterBinaryPack(UABField uField, object value) {

			this.tempBinariesPack.Add(new TempBinaryPack() { uField = uField, value = value });

		}

		public bool IsSkipped(System.Type type) {

			return
				type == typeof(System.IntPtr);

		}

		#region Serializer
		public UABField[] SerializeForced(object component, List<ISerializer> serializers) {

			return this.Serialize(component, forced: true, serializers: serializers);

		}

		public UABField[] Serialize(object component, List<ISerializer> serializers) {

			return this.Serialize(component, forced: false, serializers: serializers);

		}

		public UABField[] Serialize(object component, bool forced, List<ISerializer> serializers) {

			var list = ListPool<UABField>.Get();
			if (UABSerializer.HasProperties(component) == true) {
				
				var fields = component.GetType().GetAllProperties()
					.Where(x => x.GetCustomAttributes(true).Any(a => a is System.ObsoleteAttribute) == false && x.CanWrite == true && UABSerializer.FilterProperties(x) == true).ToArray();
				for (int i = 0; i < fields.Length; ++i) {

					var field = fields[i];

					var ignoreAttr = field.GetCustomAttributes(typeof(BundleIgnoreAttribute), inherit: true);
					if ((ignoreAttr != null && ignoreAttr.Length != 0)) continue;

					if (this.IsSkipped(fields[i].PropertyType) == true) continue;

					list.Add(this.Pack(fields[i].Name, fields[i].PropertyType, fields[i].GetValue(component, null), serializers));

				}

			} else {

				var fields = component.GetType().GetAllFields();
				for (int i = 0; i < fields.Length; ++i) {

					var field = fields[i];

					if (forced == false) {

						var attr = field.GetCustomAttributes(typeof(SerializeField), inherit: true);
						if ((attr == null || attr.Length == 0) && field.IsPublic == false) continue;

					}

					var ignoreAttr = field.GetCustomAttributes(typeof(BundleIgnoreAttribute), inherit: true);
					if ((ignoreAttr != null && ignoreAttr.Length != 0)) continue;

					if (this.IsSkipped(fields[i].FieldType) == true) continue;

					list.Add(this.Pack(fields[i].Name, fields[i].FieldType, fields[i].GetValue(component), serializers));

				}

			}

			var uFields = list.ToArray();
			ListPool<UABField>.Release(list);

			return uFields;

		}

		public UABField Pack(string fieldName, System.Type fieldType, object value, List<ISerializer> serializers) {
			
			var uField = new UABField();
			uField.name = fieldName;
			uField.fieldType = FieldType.ValueType;

			if (value == null || (value is Object && (value as Object) == null)) {
				
				uField.data = null;
				return uField;

			}

			if ((value is Object) == false) {

				for (int i = 0; i < serializers.Count; ++i) {

					var s = serializers[i];
					if ((s is IBinarySerializer) == false && s.IsValid(value) == true) {
						
						s.Serialize(this, uField, ref value, serializers);
						return uField;

					}

				}

			}

			var type = fieldType;
			if (value.IsArray() == true) {

				// array
				uField.fieldType = FieldType.ArrayType;

			} else if (
				(type.IsClass == true || type.IsValueType == true) &&
				type.IsSerializable == true &&
				type.IsSimpleType() == false &&
				type.IsEnum == false) {

				// nested

				uField.fieldType = FieldType.NestedType;
				uField.fields = this.Serialize(value, serializers);

			} else if (
				type.IsSimpleType() == true ||
				type.IsValueType == true ||
				type.IsEnum == true) {

				// value type
				uField.fieldType = FieldType.ValueType;

			} else {

				// ref type
				uField.fieldType = FieldType.ReferenceType;

				if (UAB.Builder.IsBinary(this.currentBundleName, value as Object) == true) {

					// binary type
					uField.fieldType = FieldType.BinaryType;

				}

			}

			if (uField.fieldType == FieldType.ValueType) {

				var found = false;
				for (int i = 0; i < serializers.Count; ++i) {

					if (serializers[i].IsValid(value) == true) {

						serializers[i].Serialize(this, uField, ref value, serializers);
						found = true;
						break;

					}

				}

				if (found == false) {

					// no custom serializator was found - use default
					uField.serializatorId = -1;
					if (value is string) {

						uField.data = (string)value;

					} else {

						uField.data = UABSerializer.SerializeValueType(value);

					}

				}

			} else if (uField.fieldType == FieldType.ReferenceType) {

				//Debug.Log("Pack ref type: " + value);

				this.RegisterReferencePack(uField, value);

			} else if (uField.fieldType == FieldType.BinaryType) {

				this.RegisterBinaryPack(uField, value);

			} else if (uField.fieldType == FieldType.ArrayType) {

				var count = 0;
				var enumerator = ((IEnumerable)value).GetEnumerator();
				while (enumerator.MoveNext() == true) {

					++count;

				}

				//Debug.Log("Pack arr type: " + count);
				if (count > 0) {

					enumerator.Reset();
					uField.fields = new UABField[count];
					count = 0;
					while (enumerator.MoveNext() == true) {

						var element = enumerator.Current;
						uField.fields[count] = (element == null ? null : this.Pack(null, element.GetType(), element, serializers));
						++count;

					}

				}

			}

			return uField;

		}
		#endregion

	}

}