using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ME.UAB.Serializers {

	public static class TextureSerializerHelper {

		#if UNITY_EDITOR
		public static bool PrepareForEncode(Texture tex) {

			var importer = UnityEditor.TextureImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(tex)) as UnityEditor.TextureImporter;
			if (importer != null) {

				var oldValue = importer.isReadable;
				importer.isReadable = true;
				importer.SaveAndReimport();

				return oldValue;

			}

			return false;

		}

		public static void CompleteEncoding(Texture tex, bool readable) {

			var importer = UnityEditor.TextureImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(tex)) as UnityEditor.TextureImporter;
			if (importer != null) {

				importer.isReadable = readable;
				importer.SaveAndReimport();

			}

		}
		#endif

	}

	public class TextureSerializer : IBinarySerializer, ISerializer {
		
		public class Data {
			
		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			var t = value.GetType();
			return
				t == typeof(Texture) ||
				t == typeof(Texture2D);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			var textureData = System.Convert.FromBase64String(field.data);
			var tex = new Texture2D(1, 1);
			tex.LoadImage(textureData);
			tex.Apply(updateMipmaps: true, makeNoLongerReadable: true);
			value = tex;

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			field.fieldType = FieldType.BinaryType;
			if (packer == null) {
				
				#if UNITY_EDITOR
				var oldReadable = TextureSerializerHelper.PrepareForEncode(value as Texture);
				var texture = (value as Texture2D);
				var bytes = texture.EncodeToPNG();
				TextureSerializerHelper.CompleteEncoding(texture, oldReadable);
				field.data = System.Convert.ToBase64String(bytes);
				#endif

			} else {

				var data = new Data();
				field.fields = packer.Serialize(data, serializers);

			}

		}

	}

	public class SpriteSerializer : IBinarySerializer, ISerializer {

		public class Data {

			public Rect spriteRect;
			public Vector2 spritePivot;
			public float spritePixelsPerUnit;
			public uint spriteExtrude;
			public SpriteMeshType spriteMeshType;
			public Vector4 spriteBorder;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			var t = value.GetType();
			return t == typeof(Sprite);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {
			
			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			var textureData = System.Convert.FromBase64String(field.data);
			var tex = new Texture2D(1, 1);
			tex.LoadImage(textureData);
			tex.Apply(updateMipmaps: true, makeNoLongerReadable: true);
			var sprite = Sprite.Create(tex, data.spriteRect, data.spritePivot, data.spritePixelsPerUnit, data.spriteExtrude, data.spriteMeshType, data.spriteBorder);
			value = sprite;

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			field.fieldType = FieldType.BinaryType;
			var sprite = (value as Sprite);
			if (packer == null) {

				#if UNITY_EDITOR
				var oldReadable = TextureSerializerHelper.PrepareForEncode(sprite.texture);
				var bytes = sprite.texture.EncodeToPNG();
				TextureSerializerHelper.CompleteEncoding(sprite.texture, oldReadable);
				field.data = System.Convert.ToBase64String(bytes);
				#endif

			} else {

				var data = new Data();
				data.spriteRect = sprite.rect;
				data.spritePivot = sprite.pivot;
				data.spritePixelsPerUnit = sprite.pixelsPerUnit;
				data.spriteExtrude = 0u;
				data.spriteMeshType = SpriteMeshType.FullRect;
				data.spriteBorder = sprite.border;
				field.fields = packer.Serialize(data, serializers);

			}

		}

	}

}