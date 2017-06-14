using UnityEngine;
using System.Collections;

namespace ME.UAB {

	public class BundleIgnoreAttribute : System.Attribute {
	};

	public enum FieldType : byte {

		ValueType,
		ReferenceType,
		NestedType,
		ArrayType,
		BinaryType,

	};

	[System.Serializable]
	public class UABReference {

		public string instanceId;
		public bool isLocal;
		public bool isGameObject;

	}

	[System.Serializable]
	public class UABBinary {

		public string instanceId;

	}

	[System.Serializable]
	public class UABField {

		public string name;
		public FieldType fieldType;
		public string serializatorId;
		public UABField[] fields;
		public string data;

	}

	[System.Serializable]
	public class UABComponent {

		public int instanceId;
		public string type;
		public UABField[] fields;

	}

	[System.Serializable]
	public class UABBinaryData {

		public string instanceId;
		public string data;

	}

	[System.Serializable]
	public class UABBinaryHeader {

		public string instanceId;
		public UABField field;
		public string binDataInstanceId;

	}

	[System.Serializable]
	public class UABGameObject {

		public UABComponent[] components;
		public UABGameObject[] childs;

		public string name;
		public string tag;
		public int layer;
		public bool active;

	}

	[System.Serializable]
	public class UABPackage {

		public UABGameObject[] objects;
		public UABBinaryHeader[] binaryHeaders;
		public UABBinaryData[] binaryData;

	}

}