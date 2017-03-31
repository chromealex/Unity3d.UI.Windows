using UnityEngine;
using System.Collections.Generic;

namespace ME.UAB {

	public interface IBinarySerializer {}

	public interface ISerializer {

		void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers);
		void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers);
		bool IsValid(object value);
		bool IsValid(string id);

	}

	public static class UABSerializer {

		public static string SerializeValueType(object data) {

			return ME.UAB.Tiny.Json.Encode(data);

		}

		public static bool FilterProperties(System.Reflection.PropertyInfo property) {

			return
				property.Name != "material" &&
				property.Name != "mesh";

		}

		public static bool HasProperties(object component) {

			return
				((component is MonoBehaviour) == false/* || (component is UnityEngine.EventSystems.UIBehaviour) == true*/) &&
				(component is Component) == true;

		}

		public static object DeserializeValueType(string data, System.Type type) {

			return ME.UAB.Tiny.Json.Decode(data, type);

		}

		public static T DeserializeValueType<T>(string data) {

			return ME.UAB.Tiny.Json.Decode<T>(data);

		}

	}

}