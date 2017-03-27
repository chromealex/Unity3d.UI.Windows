using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ME.UAB.Serializers {

	public class KeyframeSerializer : ISerializer {
		
		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Keyframe);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var keyFrame = new Keyframe();
			value = unpacker.Deserialize(keyFrame, field.fields, serializers);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			field.fields = packer.SerializeForced(value, serializers);

		}

	}

	public class AnimationCurveSerializer : ISerializer {

		public class Data {
			
			public Keyframe[] keys;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(AnimationCurve);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new AnimationCurve(data.keys);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			var data = new Data();
			var tr = value as AnimationCurve;
			data.keys = tr.keys;
			field.fields = packer.Serialize(data, serializers);

		}

	}

	public class Color32Serializer : ISerializer {

		public class Data {

			public byte r;
			public byte g;
			public byte b;
			public byte a;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Color32);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new Color32(data.r, data.g, data.b, data.a);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class ColorSerializer : ISerializer {

		public class Data {

			public float r;
			public float g;
			public float b;
			public float a;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Color);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new Color(data.r, data.g, data.b, data.a);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class QuaternionSerializer : ISerializer {

		public class Data {

			public float x;
			public float y;
			public float z;
			public float w;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Quaternion);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new Quaternion(data.x, data.y, data.z, data.w);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class Vector4Serializer : ISerializer {

		public class Data {

			public float x;
			public float y;
			public float z;
			public float w;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Vector4);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new Vector4(data.x, data.y, data.z, data.w);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class Vector3Serializer : ISerializer {

		public class Data {

			public float x;
			public float y;
			public float z;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Vector3);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new Vector3(data.x, data.y, data.z);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class Vector2Serializer : ISerializer {

		public class Data {

			public float x;
			public float y;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Vector2);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new Vector2(data.x, data.y);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class TransformSerializer : ISerializer {

		public class Data {

			public Vector3 position;
			public Vector3 rotation;
			public Vector3 scale;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Transform);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			var tr = value as Transform;
			tr.localPosition = data.position;
			tr.localEulerAngles = data.rotation;
			tr.localScale = data.scale;

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			var tr = value as Transform;
			var data = new Data();
			data.position = tr.localPosition;
			data.rotation = tr.localEulerAngles;
			data.scale = tr.localScale;
			field.fields = packer.Serialize(data, serializers);

		}

	}

	public class RectTransformSerializer : ISerializer {

		public class Data {

			public Vector3 position;
			public Vector3 rotation;
			public Vector3 scale;
			public Vector2 anchoredPosition;
			public Vector2 pivot;
			public Vector2 anchorMin;
			public Vector2 anchorMax;
			public Vector2 sizeDelta;

		}

		public bool IsValid(int id) {

			return this.GetHashCode() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(RectTransform);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			var tr = value as RectTransform;
			tr.localPosition = data.position;
			tr.localEulerAngles = data.rotation;
			tr.localScale = data.scale;
			tr.anchoredPosition = data.anchoredPosition;
			tr.anchorMax = data.anchorMin;
			tr.anchorMin = data.anchorMax;
			tr.sizeDelta = data.sizeDelta;
			tr.pivot = data.pivot;

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetHashCode();
			var tr = value as RectTransform;
			var data = new Data();
			data.position = tr.localPosition;
			data.rotation = tr.localEulerAngles;
			data.scale = tr.localScale;
			data.anchoredPosition = tr.anchoredPosition;
			data.anchorMax = tr.anchorMin;
			data.anchorMin = tr.anchorMax;
			data.sizeDelta = tr.sizeDelta;
			data.pivot = tr.pivot;
			field.fields = packer.Serialize(data, serializers);

		}

	}

}