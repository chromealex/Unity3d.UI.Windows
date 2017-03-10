using UnityEngine;
using UnityEditor;

namespace ME {

	[CustomEditor(typeof(Tweener))]
	public class TweenerEditor : Editor {

		public void OnEnable() {

			EditorApplication.update += this.Repaint;

		}

		public void OnDisable() {

			EditorApplication.update -= this.Repaint;

		}

		private void DrawObject(object obj) {

			if (obj == null) {

				EditorGUILayout.LabelField("Null");
				return;

			}

			if (obj is Component) {

				EditorGUILayout.ObjectField(obj as Object, typeof(Component), allowSceneObjects: true);

			} else {

				if (obj is Object) {

					EditorGUILayout.LabelField((obj as Object).name);

				} else {

					EditorGUILayout.LabelField(obj.ToString());

				}

			}

		}

		public override void OnInspectorGUI() {

			var target = this.target as Tweener;
			var tweens = target.GetTweens();

		    /*if (GUILayout.Button("Copy stacks to clipboard") == true) {
		        
                var textBuilder = new System.Text.StringBuilder();
		        for (var i = 0; i < tweens.Count; ++i) {

		            textBuilder.Append(tweens[i].stackTrace);
		            textBuilder.AppendLine();
                    textBuilder.AppendLine("-----------------------------------------------");

                }

		        var text = textBuilder.ToString();
		        EditorGUIUtility.systemCopyBuffer = text;

		    }*/

			EditorGUILayout.LabelField(string.Format("Count: {0}", tweens.Count));

			for (int i = 0; i < tweens.Count; ++i) {

				var tween = tweens[i];

				EditorGUILayout.LabelField(string.Format("Tween {0} (Targets: {1})", i, tween.Count));

				++EditorGUI.indentLevel;
				{

					EditorGUILayout.LabelField(string.Format("Loops: {0}", tween.GetLoops()));

					if (tween.getTag() != null) {

						EditorGUILayout.LabelField("Tag");

						this.DrawObject(tween.getTag());

					}

					var multiTag = tween.getMultiTag();
					EditorGUILayout.LabelField("MultiTag: ");
					this.DrawObject(multiTag.tag1);
					this.DrawObject(multiTag.tag2);
					this.DrawObject(multiTag.tag3);

				}
				--EditorGUI.indentLevel;

			}

		}

	}

}
