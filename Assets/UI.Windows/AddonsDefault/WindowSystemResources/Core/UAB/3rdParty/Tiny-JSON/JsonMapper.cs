using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ME.UAB.Tiny {

	using Encoder = System.Action<object, JsonBuilder>;
	using Decoder = Func<Type, object, object>;

	public static class JsonMapper {

		internal static Encoder genericEncoder;
		internal static Decoder genericDecoder;
		internal static Dictionary<Type, Encoder> encoders = new Dictionary<Type, Encoder>();
		internal static Dictionary<Type, Decoder> decoders = new Dictionary<Type, Decoder>();

		static JsonMapper() {
			RegisterDefaultEncoder();
			RegisterDefaultDecoder();
		}

		static void RegisterDefaultEncoder() {

			// register generic encoder
			RegisterEncoder<object>((obj, builder) => {
				builder.AppendBeginObject();
				Type type = obj.GetType();
				bool first = true;
				while (type != null) {
					foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
						if (field.GetCustomAttributes(typeof(System.NonSerializedAttribute), true).Length == 0) {
							var val = field.GetValue(obj);
							if (first) first = false; else if (val != null) builder.AppendSeperator();
							JsonMapper.EncodeNameValue(field.Name, val, builder);
						}
					}
					type = type.BaseType;
				}
				builder.AppendEndObject();
			});

			// register IEnumerable support for all list and array types
			RegisterEncoder<IEnumerable>((obj, builder) => {
				builder.AppendBeginArray();
				bool first = true;
				foreach (var item in (IEnumerable)obj) {
					if (first) first = false; else builder.AppendSeperator();
					JsonMapper.EncodeValue(item, builder);
				}
				builder.AppendEndArray();
			});

			// register enum support
			RegisterEncoder<Enum>((obj, builder) => {

//TODO Alex Chrome said : Enum IS BYTE for sure
//				int value = (byte)obj;
				//int value = (int)obj;
				var value = Convert.ChangeType(obj, typeof(int));
				builder.AppendNumber(value);
			});

			// register zulu date support
			RegisterEncoder<DateTime>((obj, builder) => {
				DateTime date = (DateTime)obj;
				String zulu = date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
				builder.AppendString(zulu);
			});
		}

		static void RegisterDefaultDecoder() {

			// register generic decoder
			RegisterDecoder<object>((type, jsonObj) => {
				object instance = Activator.CreateInstance(type, true);

				if (jsonObj is IDictionary) {
					foreach (DictionaryEntry item in (IDictionary)jsonObj) {
						string name = (string)item.Key;
						if (!JsonMapper.DecodeValue(instance, name, item.Value)) {
							Console.WriteLine("couldn't decode field \"" + name + "\" of " + type);
						}
					}
				} else {
					Console.WriteLine("unsupported json type: " + (jsonObj != null ? jsonObj.GetType().ToString() : "null"));
				}

				return instance;
			});

			// register IList support 
			RegisterDecoder<System.Collections.IEnumerable>((type, jsonObj) => {
				if (typeof(IEnumerable).IsAssignableFrom(type)) {
					if (jsonObj is IList) {
						IList jsonList = (IList)jsonObj;
						if (type.IsArray) {															// Arrays
							Type elementType = type.GetElementType();
							bool nullable = elementType.IsNullable();
							var array = Array.CreateInstance(elementType, jsonList.Count);
							for (int i = 0; i < jsonList.Count; i++) {
								object value = JsonMapper.DecodeValue(jsonList[i], elementType);
								if (value != null || nullable) array.SetValue(value, i);
							}
							return array;
						} else if (type.GetGenericArguments().Length == 1) {						// IList
							IList instance = null;
							Type genericType = type.GetGenericArguments()[0];
							bool nullable = genericType.IsNullable();
							if (type != typeof(IList) && typeof(IList).IsAssignableFrom(type)) {
								instance = Activator.CreateInstance(type, true) as IList;
							} else {
								Type genericListType = typeof(List<>).MakeGenericType(genericType);
								instance = Activator.CreateInstance(genericListType) as IList;
							}
							foreach (var item in jsonList) {
								object value = JsonMapper.DecodeValue(item, genericType);
								if (value != null || nullable) instance.Add(value);
							}
							return instance;
						}
					} 
					if (jsonObj is Dictionary<string, object>) {									// Dictionary
						Dictionary<string, object> jsonDict = (Dictionary<string, object>)jsonObj;
						if (type.GetGenericArguments().Length == 2) {
							IDictionary instance = null;
							Type keyType = type.GetGenericArguments()[0];
							Type genericType = type.GetGenericArguments()[1];
							bool nullable = genericType.IsNullable();
							if (type != typeof(IDictionary) && typeof(IDictionary).IsAssignableFrom(type)) {
								instance = Activator.CreateInstance(type, true) as IDictionary;
							} else {
								Type genericDictType = typeof(Dictionary<,>).MakeGenericType(keyType, genericType);
								instance = Activator.CreateInstance(genericDictType) as IDictionary;
							}
							foreach (KeyValuePair<string, object> item in jsonDict) {
								Console.WriteLine(item.Key + " = " + JsonMapper.DecodeValue(item.Value, genericType));
								object value = JsonMapper.DecodeValue(item.Value, genericType);
								if (value != null || nullable) instance.Add(item.Key, value);
							}
							return instance;
						} else {
							Console.WriteLine("unexpected type arguemtns");
						}
					} 
				}
				Console.WriteLine("couldn't decode: " + type);
				return null;
			});

			// register enum support
			RegisterDecoder<Enum>((type, jsonObj) => {
				if (jsonObj is string) {
					return Enum.Parse(type, (string)jsonObj);
				} else {
					return Enum.ToObject(type, jsonObj);
				}
			});
		}


		public static void RegisterDecoder<T>(Decoder decoder) {
			if (typeof(T) == typeof(object)) {
				genericDecoder = decoder;
			} else {
				JsonMapper.decoders[typeof(T)] = decoder;
			}
		}

		public static void RegisterEncoder<T>(Encoder encoder) {
			if (typeof(T) == typeof(object)) {
				genericEncoder = encoder;
			} else {
				JsonMapper.encoders[typeof(T)] = encoder;
			}
		}

		public static Decoder GetDecoder(Type type) {
			if (decoders.ContainsKey(type)) {
				return decoders[type];
			} 
			foreach (var entry in decoders) {
				Type baseType = entry.Key;
				if (baseType.IsAssignableFrom(type)) {
					return entry.Value;
				}
			}
			return genericDecoder;
		}

		public static Encoder GetEncoder(Type type) {
			if (encoders.ContainsKey(type)) {
				return encoders[type];
			} 
			foreach (var entry in encoders) {
				Type baseType = entry.Key;
				if (baseType.IsAssignableFrom(type)) {
					return entry.Value;
				}
			}
			return genericEncoder;
		}

		public static T DecodeJsonObject<T>(object jsonObj) {
			Decoder decoder = GetDecoder(typeof(T));
			return (T)decoder(typeof(T), jsonObj);
		}

		public static object DecodeJsonObject(object jsonObj, Type type) {
			Decoder decoder = GetDecoder(type);
			return decoder(type, jsonObj);
		}

		public static void EncodeValue(object value, JsonBuilder builder) {
			if (JsonBuilder.IsSupported(value)) {
				builder.AppendValue(value);
			} else {
				Encoder encoder = GetEncoder(value.GetType()); 
				if (encoder != null) {
					encoder(value, builder);
				} else {
					Console.WriteLine("encoder for " + value.GetType() + " not found");
				}
			}
		}

		public static bool EncodeNameValue(string name, object value, JsonBuilder builder) {

			if (value != null) {
				
				builder.AppendName(UnwrapName(name));
				EncodeValue(value, builder);

				return true;

			}

			return false;

		}

		public static string UnwrapName(string name) {
			if (name.StartsWith("<") && name.Contains(">")) {
				return name.Substring(name.IndexOf("<") + 1, name.IndexOf(">") - 1);
			}
			return name;
		}

		static object ConvertValue(object value, Type type) {
			if (value != null) {
				Type safeType = Nullable.GetUnderlyingType(type) ?? type;
				if (!type.IsEnum) {
					return Convert.ChangeType(value, safeType);
				}
			}
			return value;
		}

		static object DecodeValue(object value, Type targetType) {
			if (value == null) return null;

			if (JsonBuilder.IsSupported(value)) {
				value = ConvertValue(value, targetType);
			}

			// use a registered decoder
			if (value != null && !targetType.IsAssignableFrom(value.GetType())) {
				Decoder decoder = GetDecoder(targetType);
				value = decoder(targetType, value);
			}

			if (value != null && targetType.IsAssignableFrom(value.GetType())) {
				return value;
			} else {
				Console.WriteLine("couldn't decode: " + targetType);
				return null;
			}
		}

		public static bool DecodeValue(object target, string name, object value) {
			Type type = target.GetType();
			while (type != null) {
				foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
					if (field.GetCustomAttributes(typeof(System.NonSerializedAttribute), true).Length == 0) {
						if (name == UnwrapName(field.Name)) {
							if (value != null) {
								Type targetType = field.FieldType;
								object decodedValue = DecodeValue(value, targetType);

								if (decodedValue != null && targetType.IsAssignableFrom(decodedValue.GetType())) {
									field.SetValue(target, decodedValue);
									return true;
								} else {
									return false;
								}
							} else {
								field.SetValue(target, null);
								return true;
							}
						}
					}
				}
				type = type.BaseType;
			}
			return false;
		}
	}
}

