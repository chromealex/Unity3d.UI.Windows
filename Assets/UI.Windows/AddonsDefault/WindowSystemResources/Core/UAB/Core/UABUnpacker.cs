using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ME.UAB.Extensions;

namespace ME.UAB {
	
	public class UABUnpacker {

		private Dictionary<int, Object> tempReferencesUnpack = null;
		private UABPackage package = null;
		private GameObject[] unpackgedGameObjects = null;

		public GameObject[] Run(UABPackage package, UABConfig config = null, List<ISerializer> serializers = null) {

			if (this.unpackgedGameObjects != null) return this.unpackgedGameObjects;

			if (config == null) config = Builder.GetDefaultConfig(required: true);
			if (serializers == null) serializers = Builder.GetAllSerializers(config);

			this.Free();

			this.package = package;
			this.tempReferencesUnpack = new Dictionary<int, Object>();

			var gos = new GameObject[package.objects.Length];
			for (int i = 0; i < package.objects.Length; ++i) {

				var go = this.Unpack(package.objects[i], serializers);
				go.hideFlags = HideFlags.HideAndDontSave;
				if (Application.isPlaying == true) GameObject.DontDestroyOnLoad(go);
				gos[i] = go;

			}

			this.unpackgedGameObjects = gos;

			this.Free();
			return gos;

		}

		public void Free() {

			this.package = null;

			if (this.tempReferencesUnpack != null) this.tempReferencesUnpack.Clear();
			this.tempReferencesUnpack = null;

		}

		#region Unpack
		public GameObject Unpack(UABGameObject root, List<ISerializer> serializers) {

			return this.Unpack(root, null, serializers);

		}

		public GameObject Unpack(UABGameObject root, Transform parent, List<ISerializer> serializers) {
			
			var go = new GameObject(root.name);
			go.tag = root.tag;
			go.layer = root.layer;
			go.SetActive(root.active);
			go.transform.SetParent(parent);

			var tempList = ListPool<Component>.Get();
			for (int i = 0; i < root.components.Length; ++i) {

				var type = System.Type.GetType(root.components[i].type);
				var isTransform = false;
				if (
					type == typeof(Transform) ||
					type == typeof(RectTransform)) {

					isTransform = true;

				}

				Component c = null;
				if (isTransform == true) {

					c = go.GetComponent<Transform>();
					if (type == typeof(RectTransform)) c = go.AddComponent<RectTransform>();

				} else {

					c = go.AddComponent(type);

				}

				if (c == null) {

					throw new UnityException("Package malformed. Type was not found: " + root.components[i].type);

				} else {

					this.RegisterReferenceUnpack(root.components[i], c);
					tempList.Add(c);

				}

			}

			for (int i = 0; i < root.childs.Length; ++i) {

				this.Unpack(root.childs[i], go.transform, serializers);

			}

			for (int i = 0; i < root.components.Length; ++i) {

				this.Unpack(tempList[i], root.components[i], serializers);

			}

			ListPool<Component>.Release(tempList);

			return go;

		}

		public void Unpack(Component component, UABComponent uComponent, List<ISerializer> serializers) {

			var found = false;
			for (int j = 0; j < serializers.Count; ++j) {

				if (serializers[j].IsValid(component) == true) {

					var field = new UABField();
					field.fields = uComponent.fields;
					var componentObj = (object)component;
					serializers[j].DeserializeBeforeRef(this, field, ref componentObj, serializers);
					found = true;
					break;

				}

			}

			if (found == false) {

				this.Deserialize(component, uComponent.fields, serializers);

			}

		}

		public object Deserialize(object component, UABField[] data, List<ISerializer> serializers) {

			if (UABSerializer.HasProperties(component) == true) {

				var fields = component.GetType().GetAllProperties()
					.Where(x => x.GetCustomAttributes(true).Any(a => a is System.ObsoleteAttribute) == false && x.CanWrite == true && UABSerializer.FilterProperties(x) == true).ToArray();
				//var fields = component.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance);
				//Debug.Log(component + " :: " + UABSerializer.HasProperties(component) + " :: " + fields.Length);
				for (int i = 0; i < data.Length; ++i) {

					var fieldInfo = fields.FirstOrDefault(x => x.Name == data[i].name);
					if (fieldInfo != null) {

						fieldInfo.SetValue(component, this.Unpack(component, fieldInfo.PropertyType, data[i], serializers), null);

					} else {

						Debug.LogWarningFormat("Property with the name `{0}` was not found on property deserialization stage. Be sure you have no per-platform #ifdef directives in your scripts. Skipped.", data[i].name);

					}

				}

			} else {
				
				var fields = component.GetType().GetAllFields();
				for (int i = 0; i < data.Length; ++i) {
					
					var fieldInfo = fields.FirstOrDefault(x => x.Name == data[i].name);
					if (fieldInfo != null) {

						fieldInfo.SetValue(component, this.Unpack(component, fieldInfo.FieldType, data[i], serializers));

					} else {

						Debug.LogWarningFormat("Field with the name `{0}` was not found on field deserialization stage. Be sure you have no per-platform #ifdef directives in your scripts. Skipped.", data[i].name);

					}

				}

			}

			return component;

		}

		public object Unpack(object container, System.Type fieldType, UABField fieldData, List<ISerializer> serializers) {

			for (int i = 0; i < serializers.Count; ++i) {

				var s = serializers[i];
				if ((s is IBinarySerializer) == false && s.IsValid(fieldData.serializatorId) == true) {

					object obj = container;
					s.DeserializeBeforeRef(this, fieldData, ref obj, serializers);
					return obj;

				}

			}

			object result = null;
			if (fieldData.fieldType == FieldType.ArrayType) {

				if (fieldData.fields == null) {

					fieldData.fields = new UABField[0];

				}

				var elementType = fieldType.GetEnumerableType();
				if (fieldType.IsGenericType == true) {

					IList list = (IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
					for (int i = 0; i < fieldData.fields.Length; ++i) {

						var item = fieldData.fields[i];
						if (item != null) {

							list.Add(this.Unpack(null, elementType, item, serializers));

						} else {

							list.Add(null);

						}

					}

					result = list;

				} else if (fieldType.IsArray == true) {

					var filledArray = System.Array.CreateInstance(elementType, fieldData.fields.Length);
					for (int i = 0; i < fieldData.fields.Length; ++i) {

						var item = fieldData.fields[i];
						if (item != null) {

							filledArray.SetValue(this.Unpack(null, elementType, item, serializers), i);

						} else {

							filledArray.SetValue(null, i);

						}

					}

					result = filledArray;

				}

			} else if (fieldData.fieldType == FieldType.NestedType) {

				var type = fieldType;
				object instance = null;
				try {

					instance = System.Activator.CreateInstance(type);

				} catch (System.Exception) {

					instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);

				}

				//fieldInfo.SetValue(obj, instance);
				result = instance;
				this.Deserialize(instance, fieldData.fields, serializers);

			} else if (fieldData.fieldType == FieldType.ValueType) {

				//Debug.Log(obj + " :: " + fieldInfo.Name + " = " + fieldData.data + " << " + fieldData.serializatorId);

				object value = null;

				var found = false;
				for (int i = 0; i < serializers.Count; ++i) {

					if (serializers[i].IsValid(fieldData.serializatorId) == true) {

						object obj = container;
						serializers[i].DeserializeBeforeRef(this, fieldData, ref obj, serializers);
						value = obj;
						found = true;
						break;

					}

				}

				if (found == false) {

					// no custom serializator was found - use default
					//fieldInfo.SetValue(obj, UABSerializer.DeserializeValueType(fieldData.data));

					try {

						if (fieldType.IsEnum == true) {

							value = System.Enum.ToObject(fieldType, int.Parse(fieldData.data));

						} else {

							value = System.Convert.ChangeType(fieldData.data, fieldType);

						}

					} catch (System.Exception) {

						//Debug.LogException(ex);
						value = UABSerializer.DeserializeValueType(fieldData.data, fieldType);

					}

				}

				//fieldInfo.SetValue(obj, value);
				result = value;

			} else if (fieldData.fieldType == FieldType.ReferenceType) {

				// Unpack ref type
				//Debug.Log("Unpack ref type: " + fieldData.data);

				//UAB.Builder.RegisterReferenceUnpack(fieldData, fieldInfo, obj, contextCallback);

				var data = UABSerializer.DeserializeValueType(fieldData.data, typeof(UABReference)) as UABReference;
				if (data != null) {

					if (data.isLocal == false) {

						var rf = UnityEngine.Resources.Load<ObjectReference>(data.instanceId);
						if (rf != null) {

							//Debug.LogWarning("Resources.Load: " + rf + " :: " + data.instanceId + " :: " + container);
							//fieldInfo.SetValue(container, rf.reference);
							result = rf.reference;

						} else {

							Debug.LogWarning(string.Format("Resource Reference was not found `{0}`.", data.instanceId));

						}

					} else {

						if (data.instanceId != "0") {

							object obj = null;
							Object value;
							if (this.GetTempByInstanceID(data.instanceId, out value) == true) {

								if (data.isGameObject == true) {

									obj = (object)((value as Transform).gameObject);

								} else {

									obj = (object)value;

								}

								result = obj;

							} else {

								throw new UnityException(string.Format("Package malformed. Local reference `{0}` was not found.", data.instanceId));

							}

						}

					}

				} else {

					throw new UnityException("Package malformed. UABReference deserialization failed.");

				}

			} else if (fieldData.fieldType == FieldType.BinaryType) {
				
				var data = UABSerializer.DeserializeValueType(fieldData.data, typeof(UABBinary)) as UABBinary;
				if (data != null) {

					var binHeader = this.GetBinaryHeader(data.instanceId);
					if (binHeader != null) {

						var found = false;
						for (int i = 0; i < serializers.Count; ++i) {

							if (serializers[i].IsValid(binHeader.field.serializatorId) == true) {

								object obj = null;
								var binData = this.GetBinaryData(binHeader.binDataInstanceId);
								if (binData != null) {

									var field = new UABField();
									field.data = binData.data;
									field.fields = binHeader.field.fields;
									serializers[i].DeserializeBeforeRef(this, field, ref obj, serializers);
									result = obj;
									found = true;

								}
								break;

							}

						}

						if (found == false) {

							throw new UnityException(string.Format("Package malformed. Serializer was not found by id `{0}`.", binHeader.field.serializatorId));

						}

					} else {

						throw new UnityException(string.Format("Package malformed. Binary Header was not found by id `{0}`.", data.instanceId));

					}

				} else {

					throw new UnityException("Package malformed. UABBinary deserialization failed.");

				}

			}

			return result;

		}

		#endregion

		public UABBinaryData GetBinaryData(string instanceId) {

			return this.package.binaryData.FirstOrDefault(x => x.instanceId == instanceId);

		}

		public UABBinaryHeader GetBinaryHeader(string instanceId) {

			return this.package.binaryHeaders.FirstOrDefault(x => x.instanceId == instanceId);

		}

		public void RegisterReferenceUnpack(UABComponent component, Object obj) {

			this.tempReferencesUnpack.Add(component.instanceId, obj);

		}

		public bool GetTempByInstanceID(string instanceId, out Object value) {

			return this.tempReferencesUnpack.TryGetValue(int.Parse(instanceId), out value);

		}

	}

}

