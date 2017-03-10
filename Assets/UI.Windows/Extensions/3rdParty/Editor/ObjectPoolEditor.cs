
using UnityEngine;
using UnityEngine.Extensions;
using System.Collections.Generic;
using UnityEngine.UI.Windows;

namespace UnityEditor.Extensions {
	
	[CustomEditor(typeof(ObjectPool))]
	public class ObjectPoolEditor : Editor {

	    private class ComponentData {

	        public string comment;
	        public Component component;

	        public ComponentData(string comment, Component component) {

	            this.comment = comment;
	            this.component = component;

	        }


        }

		private bool nonPoolComponents;
		private bool objectLookup;
		private bool prefabLookup;
		private bool sceneLookup;
#if UNITY_EDITOR && POOL_TRACE
	    private bool referenceInfos;
#endif

        private Dictionary<string, List<ComponentData>> cache = new Dictionary<string, List<ComponentData>>();

        private void AddToCache(string name, Component obj, string comment) {

			List<ComponentData> list;
			if (this.cache.TryGetValue(name, out list) == true) {

				list.Add(new ComponentData(comment, obj));

			} else {

				this.cache.Add(name, new List<ComponentData> { new ComponentData(comment, obj) });

			}

		}

		public void OnEnable() {

			EditorApplication.update += this.Repaint;

		}

		public void OnDisable() {

			EditorApplication.update -= this.Repaint;

		}

		public override void OnInspectorGUI() {

			var target = this.target as ObjectPool;

#if UNITY_EDITOR && POOL_TRACE
		    if (GUILayout.Button("Cleanup references") == true) {

		        target.CleanupReferenceInfos();

		    }

		    this.referenceInfos = EditorGUILayout.Foldout(this.referenceInfos, string.Format("Reference infos ({0})", target.referenceInfos.Count));
		    if (this.referenceInfos == true) {

		        var infos = target.referenceInfos;
		        for (var i = 0; i < infos.Count; ++i) {

		            var info = infos[i];
                    EditorGUILayout.LabelField(string.Format("{0} ({1})", info.name, info.typeName));
		            if (GUILayout.Button("Copy stack trace to clipboard") == true) {

		                EditorGUIUtility.systemCopyBuffer = info.stackTrace;

                    }
		            
                    EditorGUILayout.Space();

                }

            }
#endif

            this.nonPoolComponents = EditorGUILayout.Foldout(this.nonPoolComponents, string.Format("Non Pool Components ({0})", target.GetNonPoolComponents().Count));
			if (this.nonPoolComponents == true) {

				this.cache.Clear();
				var list = target.GetNonPoolComponents();

			    for (var i = 0; i < list.Count; ++i) {

			        var item = list.GetKeyAt(i);

			        var stackTrace = list.GetValueAt(i).ToString();

                    if (item == null) {

                        this.AddToCache("Null", item, stackTrace);
                        continue;

                    }

                    if (item is WindowObject) {

                        var win = (item as WindowObject).GetWindow();
                        if (win != null) {

                            this.AddToCache(win.name, item, stackTrace);

                        } else {

                            this.AddToCache("Null Window", item, stackTrace);

                        }

                    } else {

                        this.AddToCache("Other Objects", item, stackTrace);

                    }

                }

                ++EditorGUI.indentLevel;
				foreach (var item in this.cache) {

					EditorGUILayout.LabelField(string.Format("{0} ({1})", item.Key, item.Value.Count));
					++EditorGUI.indentLevel;
					foreach (var componentData in item.Value) {

					    var subItem = componentData.component;

                        if (subItem == null) {

							EditorGUILayout.LabelField(subItem.GetType().ToString());

                            ++EditorGUI.indentLevel;
                            EditorGUILayout.TextField(componentData.comment);
                            --EditorGUI.indentLevel;

                        } else {

							EditorGUILayout.ObjectField(subItem, typeof(Component), allowSceneObjects: true);

						}

                    }
					--EditorGUI.indentLevel;

				}
				--EditorGUI.indentLevel;

			}

			this.objectLookup = EditorGUILayout.Foldout(this.objectLookup, string.Format("Object Lookup ({0})", target.GetObjectLookup().Count));
            
			if (this.objectLookup == true) {

				++EditorGUI.indentLevel;
				var list = target.GetObjectLookup();
				foreach (var item in list) {

					if (item.Key == null) {

						EditorGUILayout.LabelField(item.Key.GetType().ToString());

					} else {
						
						EditorGUILayout.ObjectField(item.Key, typeof(Component), allowSceneObjects: true);

					}

					++EditorGUI.indentLevel;
					foreach (var subItem in item.Value) {

						if (subItem == null) {

							EditorGUILayout.LabelField(subItem.GetType().ToString());

						} else {

							EditorGUILayout.ObjectField(subItem, typeof(Component), allowSceneObjects: true);

						}

					}
					--EditorGUI.indentLevel;

				}
				--EditorGUI.indentLevel;

			}

			this.prefabLookup = EditorGUILayout.Foldout(this.prefabLookup, string.Format("Prefab Lookup ({0})", target.GetPrefabLookup().Count));
			if (this.prefabLookup == true) {

				++EditorGUI.indentLevel;
				var list = target.GetPrefabLookup();
				foreach (var item in list) {

					if (item.Key == null) {

						EditorGUILayout.LabelField(item.Key.GetType().ToString());

					} else {

						EditorGUILayout.ObjectField(item.Key, typeof(Component), allowSceneObjects: true);

					}

					if (item.Value == null) {

						EditorGUILayout.LabelField(item.Value.GetType().ToString());

					} else {
						
						EditorGUILayout.ObjectField(item.Value, typeof(Component), allowSceneObjects: true);

					}

					EditorGUILayout.Space();

				}
				--EditorGUI.indentLevel;

			}

			this.sceneLookup = EditorGUILayout.Foldout(this.sceneLookup, string.Format("Scene Lookup ({0})", target.GetSceneLookup().Count));
			if (this.sceneLookup == true) {

				++EditorGUI.indentLevel;
				var list = target.GetSceneLookup();
				foreach (var item in list) {

					EditorGUILayout.ObjectField(item.Key, typeof(Component), allowSceneObjects: true);
					EditorGUILayout.ObjectField(item.Value, typeof(Component), allowSceneObjects: true);
					EditorGUILayout.Space();

				}
				--EditorGUI.indentLevel;

			}

		}

	}

}