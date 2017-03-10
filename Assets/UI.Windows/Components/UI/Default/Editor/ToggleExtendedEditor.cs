using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI {

	[CustomEditor(typeof(ToggleExtended))]
	public class ToggleExtendedEditor : ToggleEditor {

		private SerializedProperty transition;
		private SerializedProperty transitionExtended;

		SerializedProperty anchorsTransitionOnProperty;
		SerializedProperty anchorsTransitionOffProperty;

		AnimBool anchorsTransitionOnShow = new AnimBool();
		AnimBool anchorsTransitionOffShow = new AnimBool();

		SerializedProperty pivotTransitionOnProperty;
		SerializedProperty pivotTransitionOffProperty;

		AnimBool pivotTransitionOnShow = new AnimBool();
		AnimBool pivotTransitionOffShow = new AnimBool();

		SerializedProperty colorTransitionOnProperty;
		SerializedProperty colorTransitionOffProperty;

		AnimBool colorTransitionOnShow = new AnimBool();
		AnimBool colorTransitionOffShow = new AnimBool();

		SerializedProperty alphaTransitionOnProperty;
		SerializedProperty alphaTransitionOffProperty;

		AnimBool alphaTransitionOnShow = new AnimBool();
		AnimBool alphaTransitionOffShow = new AnimBool();

		protected override void OnEnable() {

			base.OnEnable();

			this.transition = serializedObject.FindProperty("toggleTransition");
			this.transitionExtended = serializedObject.FindProperty("toggleTransitionExtended");

			anchorsTransitionOnProperty = serializedObject.FindProperty("anchorsTransitionOn");
			anchorsTransitionOffProperty = serializedObject.FindProperty("anchorsTransitionOff");

			pivotTransitionOnProperty = serializedObject.FindProperty("pivotTransitionOn");
			pivotTransitionOffProperty = serializedObject.FindProperty("pivotTransitionOff");

			colorTransitionOnProperty = serializedObject.FindProperty("colorTransitionOn");
			colorTransitionOffProperty = serializedObject.FindProperty("colorTransitionOff");

			alphaTransitionOnProperty = serializedObject.FindProperty("m_AlphaOn");
			alphaTransitionOffProperty = serializedObject.FindProperty("m_AlphaOff");

			var trans = GetTransition(this.transitionExtended);
			anchorsTransitionOnShow.value = (trans & ToggleExtended.ToggleTransitionExtended.Anchors) != 0;
			anchorsTransitionOffShow.value = (trans & ToggleExtended.ToggleTransitionExtended.Anchors) != 0;
			pivotTransitionOnShow.value = (trans & ToggleExtended.ToggleTransitionExtended.Pivot) != 0;
			pivotTransitionOffShow.value = (trans & ToggleExtended.ToggleTransitionExtended.Pivot) != 0;
			colorTransitionOnShow.value = (trans & ToggleExtended.ToggleTransitionExtended.Color) != 0;
			colorTransitionOffShow.value = (trans & ToggleExtended.ToggleTransitionExtended.Color) != 0;
			alphaTransitionOnShow.value = (trans & ToggleExtended.ToggleTransitionExtended.Alpha) != 0;
			alphaTransitionOffShow.value = (trans & ToggleExtended.ToggleTransitionExtended.Alpha) != 0;

			anchorsTransitionOnShow.valueChanged.AddListener(Repaint);
			anchorsTransitionOffShow.valueChanged.AddListener(Repaint);

			pivotTransitionOnShow.valueChanged.AddListener(Repaint);
			pivotTransitionOffShow.valueChanged.AddListener(Repaint);

			colorTransitionOnShow.valueChanged.AddListener(Repaint);
			colorTransitionOffShow.valueChanged.AddListener(Repaint);

			alphaTransitionOnShow.valueChanged.AddListener(Repaint);
			alphaTransitionOffShow.valueChanged.AddListener(Repaint);

		}

		protected override void OnDisable() {

			base.OnDisable();

			anchorsTransitionOnShow.valueChanged.RemoveListener(Repaint);
			anchorsTransitionOffShow.valueChanged.RemoveListener(Repaint);

			pivotTransitionOnShow.valueChanged.RemoveListener(Repaint);
			pivotTransitionOffShow.valueChanged.RemoveListener(Repaint);

			colorTransitionOnShow.valueChanged.RemoveListener(Repaint);
			colorTransitionOffShow.valueChanged.RemoveListener(Repaint);

			alphaTransitionOnShow.valueChanged.RemoveListener(Repaint);
			alphaTransitionOffShow.valueChanged.RemoveListener(Repaint);

		}

		static ToggleExtended.ToggleTransitionExtended GetTransition(SerializedProperty transition) {

			return (ToggleExtended.ToggleTransitionExtended)transition.intValue;

		}

		public override void OnInspectorGUI() {
			
			base.OnInspectorGUI();

			serializedObject.Update();

			var trans = GetTransition(this.transitionExtended);

			if (this.transitionExtended.intValue > this.transition.intValue) {
				
				this.transition.intValue = 0;
			
			} else {

				this.transition.intValue = this.transitionExtended.intValue;

			}

			anchorsTransitionOnShow.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ToggleExtended.ToggleTransitionExtended.Anchors) != 0);
			anchorsTransitionOffShow.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ToggleExtended.ToggleTransitionExtended.Anchors) != 0);

			pivotTransitionOnShow.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ToggleExtended.ToggleTransitionExtended.Pivot) != 0);
			pivotTransitionOffShow.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ToggleExtended.ToggleTransitionExtended.Pivot) != 0);

			colorTransitionOnShow.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ToggleExtended.ToggleTransitionExtended.Color) != 0);
			colorTransitionOffShow.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ToggleExtended.ToggleTransitionExtended.Color) != 0);

			alphaTransitionOnShow.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ToggleExtended.ToggleTransitionExtended.Alpha) != 0);
			alphaTransitionOffShow.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ToggleExtended.ToggleTransitionExtended.Alpha) != 0);

			EditorGUILayout.PropertyField(this.transitionExtended);

			if (EditorGUILayout.BeginFadeGroup(anchorsTransitionOnShow.faded)) {
				EditorGUILayout.PropertyField(anchorsTransitionOnProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(anchorsTransitionOffShow.faded)) {
				EditorGUILayout.PropertyField(anchorsTransitionOffProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(pivotTransitionOnShow.faded)) {
				EditorGUILayout.PropertyField(pivotTransitionOnProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(pivotTransitionOffShow.faded)) {
				EditorGUILayout.PropertyField(pivotTransitionOffProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(colorTransitionOnShow.faded)) {
				EditorGUILayout.PropertyField(colorTransitionOnProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(colorTransitionOffShow.faded)) {
				EditorGUILayout.PropertyField(colorTransitionOffProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(alphaTransitionOnShow.faded)) {
				EditorGUILayout.PropertyField(alphaTransitionOnProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(alphaTransitionOffShow.faded)) {
				EditorGUILayout.PropertyField(alphaTransitionOffProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			serializedObject.ApplyModifiedProperties();

		}

	}

}