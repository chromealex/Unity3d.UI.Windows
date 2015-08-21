using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI {

	[CustomEditor(typeof(ButtonExtended))]
	public class ButtonExtendedEditor : ButtonEditor {

		private SerializedProperty transition;
		private SerializedProperty transitionExtended;
		SerializedProperty extAlphaBlockProperty;
		SerializedProperty extColorBlockProperty;
		SerializedProperty extSpriteStateProperty;
		SerializedProperty extAnimTriggerProperty;
		SerializedProperty extTargetGraphicProperty;
		AnimBool extShowAlphaTint = new AnimBool();
		AnimBool extShowColorTint = new AnimBool();
		AnimBool extShowSpriteTrasition = new AnimBool();
		AnimBool extShowAnimTransition = new AnimBool();
		private bool lastInteractive;

		protected override void OnEnable() {

			base.OnEnable();

			this.transition = this.serializedObject.FindProperty("m_Transition");
			this.transitionExtended = serializedObject.FindProperty("transitionExtended");

			extAlphaBlockProperty = serializedObject.FindProperty("m_Alpha");
			extColorBlockProperty = serializedObject.FindProperty("m_Colors");
			extSpriteStateProperty = serializedObject.FindProperty("m_SpriteState");
			extAnimTriggerProperty = serializedObject.FindProperty("m_AnimationTriggers");
			extTargetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");

			var trans = GetTransition(this.transitionExtended);
			extShowAlphaTint.value = (trans == ButtonExtended.Transition.CanvasGroupAlpha);
			extShowColorTint.value = (trans == ButtonExtended.Transition.ColorTint || trans == ButtonExtended.Transition.SpriteSwapAndColorTint);
			extShowSpriteTrasition.value = (trans == ButtonExtended.Transition.SpriteSwap || trans == ButtonExtended.Transition.SpriteSwapAndColorTint);
			extShowAnimTransition.value = (trans == ButtonExtended.Transition.Animation);
			
			extShowColorTint.valueChanged.AddListener(Repaint);
			extShowSpriteTrasition.valueChanged.AddListener(Repaint);

		}

		protected override void OnDisable() {
			base.OnDisable();

			extShowAlphaTint.valueChanged.RemoveListener(Repaint);
			extShowColorTint.valueChanged.RemoveListener(Repaint);
			extShowSpriteTrasition.valueChanged.RemoveListener(Repaint);
		}

		static ButtonExtended.Transition GetTransition(SerializedProperty transition) {

			return (ButtonExtended.Transition)transition.enumValueIndex;

		}

		public override void OnInspectorGUI() {
			
			base.OnInspectorGUI();

			serializedObject.Update();

			this.transition.enumValueIndex = 0;
			
			var trans = GetTransition(this.transitionExtended);
			var animator = (target as Selectable).GetComponent<Animator>();
			extShowAlphaTint.target = (!transitionExtended.hasMultipleDifferentValues && (trans == ButtonExtended.Transition.CanvasGroupAlpha));
			extShowColorTint.target = (!transitionExtended.hasMultipleDifferentValues && (trans == ButtonExtended.Transition.ColorTint || trans == ButtonExtended.Transition.SpriteSwapAndColorTint));
			extShowSpriteTrasition.target = (!transitionExtended.hasMultipleDifferentValues && (trans == ButtonExtended.Transition.SpriteSwap || trans == ButtonExtended.Transition.SpriteSwapAndColorTint));
			extShowAnimTransition.target = (!transitionExtended.hasMultipleDifferentValues && (trans == ButtonExtended.Transition.Animation));

			EditorGUILayout.PropertyField(this.transitionExtended);

			if (trans == ButtonExtended.Transition.ColorTint || trans == ButtonExtended.Transition.SpriteSwap || trans == ButtonExtended.Transition.SpriteSwapAndColorTint) {
				EditorGUILayout.PropertyField(extTargetGraphicProperty);
				EditorGUILayout.Space();
			}
			
			if (EditorGUILayout.BeginFadeGroup(extShowColorTint.faded)) {
				EditorGUILayout.PropertyField(extColorBlockProperty);
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFadeGroup();
			
			if (EditorGUILayout.BeginFadeGroup(extShowAlphaTint.faded)) {
				EditorGUILayout.PropertyField(extAlphaBlockProperty, includeChildren: true);
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

			serializedObject.ApplyModifiedProperties();

		}

	}

}