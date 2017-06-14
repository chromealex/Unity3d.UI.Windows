using UnityEngine;
using System.Collections;

namespace UAB.Tests {

	public class CustomTest : MonoBehaviour {

		[System.Serializable]
		public class Data {

			public string a = "test";
			public int i = 0;

			public GameObject[] goArr;

		}

		public System.IntPtr ptr;
		public Object scene;

		public int i = 1;
		public int j = 2;
		public Data nested;

		public Transform tr;
		public GameObject go;

		public Object[] trArr;

		public AnimationCurve curve;

		[ContextMenu("Setup")]
		public void Setup() {

			this.trArr = new Object[2];
			this.trArr[0] = this.tr;
			this.trArr[1] = this.go;

		}

	}

}