using UnityEngine;
using UnityEngine.UI.Windows;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEditor.UI.Windows {
	
	[CustomEditor(typeof(WindowLayoutElement))]
	[CanEditMultipleObjects()]
	public class WindowLayoutElementEditor : Editor {

		public override void OnInspectorGUI() {
			
			this.DrawDefaultInspector();

		}

	}

}