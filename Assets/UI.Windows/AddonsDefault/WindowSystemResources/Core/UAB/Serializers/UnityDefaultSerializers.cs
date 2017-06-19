using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ME.UAB.Serializers {

	public class KeyframeSerializer : ISerializer {

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Keyframe);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var keyFrame = new Keyframe();
			value = unpacker.Deserialize(keyFrame, field.fields, serializers);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			field.fields = packer.SerializeForced(value, serializers);

		}

	}

	public class AnimationCurveSerializer : ISerializer {

		public class Data {
			
			public Keyframe[] keys;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
			var data = new Data();
			var tr = value as AnimationCurve;
			data.keys = tr.keys;
			field.fields = packer.Serialize(data, serializers);

		}

	}

	public class NavMeshPathSerializer : ISerializer {

		public class Data {
			
		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			#if UNITY_5_5_OR_NEWER
			return value.GetType() == typeof(UnityEngine.AI.NavMeshPath);
			#else
			return value.GetType() == typeof(UnityEngine.NavMeshPath);
			#endif

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			#if UNITY_5_5_OR_NEWER
			value = new UnityEngine.AI.NavMeshPath();
			#else
			value = new UnityEngine.NavMeshPath();
			#endif

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class GradientAlphaKeySerializer : ISerializer {

		public class Data {

			public float alpha;
			public float time;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(GradientAlphaKey);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new GradientAlphaKey() { alpha = data.alpha, time = data.time };

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class GradientColorKeySerializer : ISerializer {

		public class Data {

			public Color color;
			public float time;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(GradientColorKey);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new GradientColorKey() { color = data.color, time = data.time };

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class GradientSerializer : ISerializer {

		public class Data {

			public GradientColorKey[] colorKeys;
			public GradientAlphaKey[] alphaKeys;
			#if UNITY_5_5_OR_NEWER
			public GradientMode mode;
			#endif

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Gradient);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			if (data.colorKeys == null) data.colorKeys = new GradientColorKey[0];
			if (data.alphaKeys == null) data.alphaKeys = new GradientAlphaKey[0];
			#if UNITY_5_5_OR_NEWER
			value = new Gradient() { mode = data.mode, colorKeys = data.colorKeys, alphaKeys = data.alphaKeys };
			#else
			value = new Gradient() { colorKeys = data.colorKeys, alphaKeys = data.alphaKeys };
			#endif

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			var v = (Gradient)value;
			var data = new Data();
			#if UNITY_5_5_OR_NEWER
			data.mode = v.mode;
			#endif
			data.colorKeys = v.colorKeys;
			data.alphaKeys = v.alphaKeys;
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

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
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

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
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

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
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

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class Vector3Serializer : ISerializer {

		public class Data {

			public float x;
			public float y;
			public float z;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class Vector2Serializer : ISerializer {

		public class Data {

			public float x;
			public float y;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
			field.fields = packer.Serialize(value, serializers);

		}

	}

	public class RectOffsetSerializer : ISerializer {

		public class Data {

			public int x;
			public int y;
			public int z;
			public int w;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(RectOffset);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new RectOffset(data.x, data.y, data.z, data.w);

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			var tr = value as RectOffset;
			var data = new Data();
			data.x = tr.left;
			data.y = tr.right;
			data.z = tr.top;
			data.w = tr.bottom;
			field.fields = packer.Serialize(data, serializers);

		}

	}

	public class TransformSerializer : ISerializer {

		public class Data {

			public Vector3 position;
			public Vector3 rotation;
			public Vector3 scale;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
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

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

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

			field.serializatorId = this.GetId();
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

	public class AnimatorSerializer : ISerializer {

		public class Data {

			public Avatar avatar;
			public RuntimeAnimatorController runtimeAnimatorController;
			public AnimatorCullingMode cullingMode;
			public AnimatorUpdateMode updateMode;
			public bool applyRootMotion;
			public bool linearVelocityBlending;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(Animator);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			var animator = value as Animator;
			animator.avatar = data.avatar;
			animator.runtimeAnimatorController = data.runtimeAnimatorController;
			animator.cullingMode = data.cullingMode;
			animator.updateMode = data.updateMode;
			animator.applyRootMotion = data.applyRootMotion;
			animator.linearVelocityBlending = data.linearVelocityBlending;

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			var animator = value as Animator;
			var data = new Data();
			data.avatar = animator.avatar;
			data.runtimeAnimatorController = animator.runtimeAnimatorController;
			data.cullingMode = animator.cullingMode;
			data.updateMode = animator.updateMode;
			data.applyRootMotion = animator.applyRootMotion;
			data.linearVelocityBlending = animator.linearVelocityBlending;
			field.fields = packer.Serialize(data, serializers);

		}

	}

}