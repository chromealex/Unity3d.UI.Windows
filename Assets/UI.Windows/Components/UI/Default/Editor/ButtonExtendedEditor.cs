using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI {

	[CustomEditor(typeof(ButtonExtended))]
	public class ButtonExtendedEditor : ButtonEditor {

		private SerializedProperty transition;
		private SerializedProperty transitionExtended;

		SerializedProperty extScaleBlockProperty;
		SerializedProperty extAlphaBlockProperty;
		SerializedProperty extColorBlockProperty;
		SerializedProperty extSpriteStateProperty;
		SerializedProperty extAnimTriggerProperty;
		SerializedProperty extTargetGraphicProperty;
		SerializedProperty extTargetGraphicsProperty;
		SerializedProperty extTargetGraphicsItemsProperty;

		AnimBool extShowScale = new AnimBool();
		AnimBool extShowAlpha = new AnimBool();
		AnimBool extShowColorTint = new AnimBool();
		AnimBool extShowSpriteTrasition = new AnimBool();
		AnimBool extShowAnimTransition = new AnimBool();
		AnimBool extShowTargetGraphicsItems = new AnimBool();
		private bool lastInteractive;

		protected override void OnEnable() {

			base.OnEnable();

			this.transition = this.serializedObject.FindProperty("m_Transition");
			this.transitionExtended = serializedObject.FindProperty("transitionExtended");
			
			extScaleBlockProperty = serializedObject.FindProperty("m_Scale");
			extAlphaBlockProperty = serializedObject.FindProperty("m_Alpha");
			extColorBlockProperty = serializedObject.FindProperty("m_Colors");
			extSpriteStateProperty = serializedObject.FindProperty("m_SpriteState");
			extAnimTriggerProperty = serializedObject.FindProperty("m_AnimationTriggers");
			extTargetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");
			extTargetGraphicsProperty = serializedObject.FindProperty("m_TargetGraphics");
			extTargetGraphicsItemsProperty = serializedObject.FindProperty("graphicItems");

			var trans = GetTransition(this.transitionExtended);
			extShowScale.value = (trans & ButtonExtended.Transition.Scale) != 0;
			extShowAlpha.value = (trans & ButtonExtended.Transition.CanvasGroupAlpha) != 0;
			extShowColorTint.value = (trans & ButtonExtended.Transition.ColorTint) != 0;
			extShowSpriteTrasition.value = (trans & ButtonExtended.Transition.SpriteSwap) != 0;
			extShowAnimTransition.value = (trans & ButtonExtended.Transition.Animation) != 0;
			extShowTargetGraphicsItems.value = (trans & ButtonExtended.Transition.TargetGraphics) != 0;
			
			extShowScale.valueChanged.AddListener(Repaint);
			extShowAlpha.valueChanged.AddListener(Repaint);
			extShowColorTint.valueChanged.AddListener(Repaint);
			extShowSpriteTrasition.valueChanged.AddListener(Repaint);

		}

		protected override void OnDisable() {

			base.OnDisable();
			
			extShowScale.valueChanged.RemoveListener(Repaint);
			extShowAlpha.valueChanged.RemoveListener(Repaint);
			extShowColorTint.valueChanged.RemoveListener(Repaint);
			extShowSpriteTrasition.valueChanged.RemoveListener(Repaint);

		}

		static ButtonExtended.Transition GetTransition(SerializedProperty transition) {

			return (ButtonExtended.Transition)transition.intValue;

		}

		public override void OnInspectorGUI() {
			
			base.OnInspectorGUI();

			serializedObject.Update();

			this.transition.enumValueIndex = 0;
			
			var trans = GetTransition(this.transitionExtended);
			var animator = (target as Selectable).GetComponent<Animator>();
			extShowScale.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.Scale) != 0);
			extShowAlpha.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.CanvasGroupAlpha) != 0);
			extShowColorTint.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.ColorTint) != 0);
			extShowSpriteTrasition.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.SpriteSwap) != 0);
			extShowAnimTransition.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.Animation) != 0);
			extShowTargetGraphicsItems.target = (!transitionExtended.hasMultipleDifferentValues && (trans & ButtonExtended.Transition.TargetGraphics) != 0);

			EditorGUILayout.PropertyField(this.transitionExtended);

			if ((trans & ButtonExtended.Transition.ColorTint) != 0 ||
			    (trans & ButtonExtended.Transition.SpriteSwap) != 0) {
				EditorGUILayout.PropertyField(extTargetGraphicProperty);
				EditorGUILayout.PropertyField(extTargetGraphicsProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			
			if (EditorGUILayout.BeginFadeGroup(extShowColorTint.faded)) {
				EditorGUILayout.PropertyField(extColorBlockProperty);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(extShowAlpha.faded)) {
				EditorGUILayout.PropertyField(extAlphaBlockProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();
			
			if (EditorGUILayout.BeginFadeGroup(extShowScale.faded)) {
				EditorGUILayout.PropertyField(extScaleBlockProperty, includeChildren: true);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(extShowSpriteTrasition.faded)) {
				EditorGUILayout.PropertyField(extSpriteStateProperty);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(extShowAnimTransition.faded)) {
				EditorGUILayout.PropertyField(extAnimTriggerProperty);

				if (animator == null || animator.runtimeAnimatorController == null) {
					Rect buttonRect = EditorGUILayout.GetControlRect();
					buttonRect.xMin += EditorGUIUtility.labelWidth;
				}
			}

			if (EditorGUILayout.BeginFadeGroup(extShowTargetGraphicsItems.faded)) {
				if (extTargetGraphicsItemsProperty != null) EditorGUILayout.PropertyField(extTargetGraphicsItemsProperty, includeChildren: true);
				EditorGUILayout.Space();
			}

			serializedObject.ApplyModifiedProperties();

		}

	}

}