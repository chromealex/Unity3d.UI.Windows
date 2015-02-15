using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI.Windows;
using System.Linq;

namespace UnitEditor.UI.Windows {

	[CanEditMultipleObjects()]
	[CustomEditor(typeof(WindowBase), true)]
	public class WindowBaseInspector : Editor {

		public override void OnInspectorGUI() {

			this.serializedObject.Update();

			this.OnEditLogic(this.targets.Cast<WindowBase>().ToArray());

			// Draw default
			this.DrawDefaultInspector();

			this.serializedObject.ApplyModifiedProperties();

		}

		public virtual void OnEditLogic(WindowBase[] targets) {
		}

	}

}