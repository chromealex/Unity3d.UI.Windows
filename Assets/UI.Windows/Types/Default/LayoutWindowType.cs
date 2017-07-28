using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Extensions;
using ME.UAB;

namespace UnityEngine.UI.Windows.Types {

	[ExecuteInEditMode()]
	public class LayoutWindowType : WindowBase {
		
		public Layouts layouts;

		// Current Layout
		[HideInInspector][SerializeField]
		private Layout layout;

		public override void ApplyOrientationIndexDirect(int index) {

			base.ApplyOrientationIndexDirect(index);

			if (this.layouts.IsValid(index) == true) {

				this.layouts.ApplyOrientationIndex(index);

			}

		}

		public override void ApplyOrientationIndex(int index) {

			base.ApplyOrientationIndex(index);

			// Is new orientation supported?
			if (this.layouts.IsSupported() == true && this.layouts.IsValid(index) == true) {

				// Reload window
				if (this.GetState() == WindowObjectState.Shown) {

					System.Action onHideEnd = () => {

						ME.Coroutines.Run(this.ApplyOrientationIndex_YIELD(index));

					};

					// Hide
					this.skipRecycle = true;
					if (this.Hide(onHideEnd) == false) {

						UnityEngine.Events.UnityAction action = null;
						action = () => {

							this.events.onEveryInstance.Unregister(WindowEventType.OnHideEndLate, action);
							onHideEnd.Invoke();

						};
						this.events.onEveryInstance.Register(WindowEventType.OnHideEndLate, action);

					}

				} else {

					Debug.LogWarning(string.Format("Window `{0}` is not in `Shown` state, this behaviour currently unsupported.", this.name));
					
				}

			}

		}

		private IEnumerator<byte> ApplyOrientationIndex_YIELD(int index) {

			yield return 0;

			this.skipRecycle = false;
			this.layouts.ApplyOrientationIndex(index);
			// Reinit
			this.setup = false;
			this.Init(this.initialParameters, () => {

				this.Show();

			}, async: false);

		}

		public Layout GetCurrentLayout() {

			return this.layouts.GetCurrentLayout();

		}

		new public LayoutWindowType GetWindow() {
			
			return base.GetWindow() as LayoutWindowType;
			
		}
		
		public override Canvas GetCanvas() {

			var layoutInstance = this.GetCurrentLayout().GetLayoutInstance();
			if (layoutInstance == null) return null;

			return layoutInstance.canvas;
			
		}

		public override CanvasScaler GetCanvasScaler() {

			var layoutInstance = this.GetCurrentLayout().GetLayoutInstance();
			if (layoutInstance == null) return null;

			return layoutInstance.canvasScaler;

		}

		public void GetLayoutComponent<T>(out T component, LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			component = this.GetLayoutComponent<T>(tag);
			
		}

		public T GetLayoutComponent<T>(LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			return this.GetCurrentLayout().Get<T>(tag);
			
		}
		
		public override Vector2 GetSize() {

			var layoutInstance = this.GetCurrentLayout().GetLayoutInstance();
			if (layoutInstance == null) return Vector2.zero;

			return layoutInstance.GetSize();
			
		}

		public WindowLayoutElement GetLayoutContainer(LayoutTag tag) {

			return this.GetCurrentLayout().GetContainer(tag);

		}

		public override int GetSortingOrder() {
			
			return this.GetCurrentLayout().GetSortingOrder();
			
		}
		
		public override float GetLayoutAnimationDuration(bool forward) {
			
			return this.GetCurrentLayout().GetAnimationDuration(forward);
			
		}
		
		public override Rect GetRect() {

			var bounds = base.GetRect();
			//var root = this.layout.GetLayoutInstance().root;
			var baseSubElements = this.GetCurrentLayout().GetLayoutInstance().GetSubComponents();
			if (baseSubElements.Count == 1) {

				var baseLayoutElement = baseSubElements[0];
				var subElements = baseLayoutElement.GetSubComponents();
				if (subElements.Count == 1) {

					var layoutElement = subElements[0];

					Vector3[] corners = new Vector3[4];
					(layoutElement.transform as RectTransform).GetWorldCorners(corners);

					var leftBottom = this.workCamera.WorldToScreenPoint(corners[0]);
					var leftTop = this.workCamera.WorldToScreenPoint(corners[1]);
					var rightTop = this.workCamera.WorldToScreenPoint(corners[2]);
					var rightBottom = this.workCamera.WorldToScreenPoint(corners[3]);

					bounds = new Rect(new Vector2(leftTop.x, leftTop.y), new Vector2(rightBottom.x - leftBottom.x, rightTop.y - rightBottom.y));

				}

			}

			return bounds;

		}

		public override Transform GetLayoutRoot() {
			
			return this.GetCurrentLayout().GetRoot();
			
		}

		protected override void MoveLayout(Vector2 delta) {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.GetLayoutInstance().root.Move(delta);

		}
		
		public override void OnLocalizationChanged() {
			
			base.OnLocalizationChanged();
			
			var layout = this.GetCurrentLayout();
			if (layout != null) layout.OnLocalizationChanged();
			
		}

		public override void OnManualEvent<T>(T data) {

			base.OnManualEvent<T>(data);

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.OnManualEvent<T>(data);

		}

		protected override void DoLayoutInit(float depth, int raycastPriority, int orderInLayer, System.Action callback, bool async) {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.Create(this, this.transform, depth, raycastPriority, orderInLayer, () => {

				this.GetCurrentLayout().DoInit();
				if (callback != null) callback.Invoke();

			}, async);

		}

		protected override void DoLayoutDeinit(System.Action callback) {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoDeinit(callback);

		}

		protected override void DoLayoutShowBegin(AppearanceParameters parameters) {

			//this.layout.DoWindowOpen();
			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoShowBegin(parameters);

		}

		protected override void DoLayoutShowEnd(AppearanceParameters parameters) {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoShowEnd(parameters);

		}

		protected override void DoLayoutHideBegin(AppearanceParameters parameters) {
			
			//this.layout.DoWindowClose();
			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoHideBegin(parameters);

		}

		protected override void DoLayoutHideEnd(AppearanceParameters parameters) {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoHideEnd(parameters);

		}

		protected override void DoLayoutWindowLayoutComplete() {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoWindowLayoutComplete();

		}

		protected override void DoLayoutWindowOpen() {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoWindowOpen();

		}

		protected override void DoLayoutWindowClose() {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoWindowClose();

		}

		protected override void DoLayoutWindowActive() {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoWindowActive();

		}

		protected override void DoLayoutWindowInactive() {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoWindowInactive();

		}

		protected override void DoLayoutUnload() {

			var layout = this.GetCurrentLayout();
			if (layout != null) layout.DoWindowUnload();

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {
			
			base.OnValidateEditor();

			this.layouts.OnValidateEditor(this, this.layout);

			if (this.GetCurrentLayout() == null) return;

			this.GetCurrentLayout().Update_EDITOR(this);
			
		}
		#endif

	}

	[System.Serializable]
	public class Layouts {

		public string[] types;		// horizontal, vertical
		public Layout[] layouts;	// 
		//[System.NonSerialized]
		public int currentLayoutIndex;

		public Layout GetCurrentLayout() {

			if (this.layouts == null || this.layouts.Length == 0) return null;

			if (this.currentLayoutIndex < 0 || this.currentLayoutIndex >= this.layouts.Length) {

				return this.layouts[0];

			}

			return this.layouts[this.currentLayoutIndex];

		}

		public bool IsSupported() {

			return this.IsValid(0) == true || this.IsValid(1) == true;

		}

		public bool IsValid(int index) {

			if (index < 0 || index >= this.layouts.Length) {

				return false;

			}

			return this.layouts[index].enabled;

		}

		public void ApplyOrientationIndex(int index) {

			this.currentLayoutIndex = index;

		}

		#if UNITY_EDITOR
		public void OnValidateEditor(LayoutWindowType root, Layout layout) {

			if (this.layouts == null || this.layouts.Length == 0 || this.layouts[0] == null) {

				// Setup as default
				this.types = new string[2];
				this.types[0] = "Horizontal";
				this.types[1] = "Vertical";

				this.layouts = new Layout[2];
				this.layouts[0] = layout;
				layout.enabled = true;
				this.layouts[1] = new Layout();

				UnityEditor.EditorUtility.SetDirty(root);

			}

		}
		#endif

	}

	[System.Serializable]
	public class Layout : IWindowEventsController, ILoadableResource, ILoadableReference {

		public bool enabled;

		private class ComponentComparer : IEqualityComparer<Component> {
			
			public bool Equals(Component x, Component y) {
				
				if (Object.ReferenceEquals(x, y)) return true;
				
				if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;
				
				return x.tag == y.tag;
				
			}
			
			public int GetHashCode(Component component) {
				
				if (Object.ReferenceEquals(component, null)) return 0;
				
				return component.tag.GetHashCode();
				
			}
			
		}

		[System.Serializable]
		public class Component : ILoadableResource, ILoadableReference {
			
			#if UNITY_EDITOR
			public string description;
			public void UpdateDescription(LayoutWindowType layoutWindow) {

				if (layoutWindow != null &&
					layoutWindow.GetCurrentLayout().layout != null) {

					var element = layoutWindow.GetCurrentLayout().layout.GetRootByTag(this.tag);
					if (element != null) {

						this.description = element.comment;

					}

				}

			}
			#endif

			[ReadOnly]
			public LayoutTag tag;
			#if UNITY_EDITOR
			public WindowComponent component;
			#endif
			public ResourceMono componentResource;
			public WindowComponent componentNoResource;
			public int sortingOrder = 0;
			public WindowComponentParametersBase componentParameters;

			#if UNITY_EDITOR
			public bool editorParametersFoldout;
			public IParametersEditor componentParametersEditor;
			#endif

			public Component(LayoutTag tag) {

				this.tag = tag;
				
			}

			public IResourceReference GetReference() {

				return this.window;

			}
			
			public ResourceBase GetResource() {

				return this.componentResource;

			}

			#if UNITY_EDITOR
			public WindowComponentParametersBase OnComponentChanged(WindowBase window, WindowComponent newComponent) {

				//var hasChanged = (newComponent != this.component);
				this.component = newComponent;

				WindowComponentParametersBase instance = null;
				//if (hasChanged == true) {

					if (newComponent == null) {

						this.componentResource.ResetToDefault();

					}

					if (this.componentParameters != null) {
						
						var link = this.componentParameters;
						//UnityEditor.EditorApplication.delayCall += () => {
						
						Object.DestroyImmediate(link, allowDestroyingAssets: true);
						
						//};
						
					}

					instance = Layout.AddParametersFor(window, this.component);
					this.componentParameters = instance;

					this.OnValidate();

				//} else {

				//	instance = this.componentParameters;

				//}

				return instance;

			}

			private IPreviewEditor editor;
			public void OnPreviewGUI(Rect rect, GUIStyle background) {

				if (this.component != null) {

					if (this.editor == null) this.editor = UnityEditor.Editor.CreateEditor(this.component) as IPreviewEditor;
					if (this.editor != null) {

						this.editor.OnPreviewGUI(rect, background);

					}

				}
				
			}

			public void OnValidate() {

				#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isUpdating == true) return;
				#endif

				/*
				if (this.componentParameters != null) {

					if (this.componentParameters.GetFlags() == 0x0) {

						// no flags = no parameters
						var link = this.componentParameters;
						UnityEditor.EditorApplication.delayCall += () => {

							Object.DestroyImmediate(link, allowDestroyingAssets: true);

						};

					}

				}*/

				if (this.componentResource == null) this.componentResource = new ResourceMono();
				this.componentResource.Validate(this.component);

				if (this.componentResource.IsLoadable() == false) {

					this.componentNoResource = this.component;

				} else {

					this.componentNoResource = null;

				}

			}
			#endif

			[HideInInspector][System.NonSerialized]
			private IWindowEventsAsync instance;
			
			[HideInInspector][System.NonSerialized]
			private IWindowComponentLayout root;
			
			[HideInInspector][System.NonSerialized]
			private WindowBase window;

			private bool stopped = false;

			public void SetComponent(WindowComponent component) {

				this.componentNoResource = component;

			}

			public void SetComponent(ResourceMono resource) {

				this.componentResource = resource;

			}

			public void Create(WindowBase window, WindowLayoutElement root, System.Action<WindowComponent> callback = null, bool async = false, System.Action<WindowObjectElement> onItem = null) {

				if (this.stopped == true) return;

				this.window = window;
				this.root = root;

				System.Action<WindowComponent> onLoaded = (component) => {

					if (this.stopped == true) return;

					if (component == null && this.root == null) {

						if (callback != null) callback.Invoke(null);
						return;

					}

					if (component == null) {

						this.root.Setup(null, this);
						if (callback != null) callback.Invoke(null);
						return;

					}

					//Debug.Log("Unpack component: " + component.name);
					var instance = component.Spawn(activeByDefault: false);
					//instance.SetComponentState(WindowObjectState.NotInitialized);
					instance.SetParent(root, setTransformAsSource: false, worldPositionStays: false);
					instance.SetTransformAs();

					if (this.componentParameters != null) {

						instance.Setup(this.componentParameters);

					}

					var rect = instance.transform as RectTransform;
					if (rect != null) {

						rect.sizeDelta = (component.transform as RectTransform).sizeDelta;
						rect.anchoredPosition = (component.transform as RectTransform).anchoredPosition;
						
					}

					this.root.Setup(instance, this);
					instance.Setup(window);
					
					if (instance.autoRegisterInRoot == true && root.autoRegisterSubComponents == true) {

						root.RegisterSubComponent(instance);

					}

					instance.transform.SetSiblingIndex(this.sortingOrder);

					this.instance = instance;
					instance.DoLoad(async, onItem, () => {

						if (this.stopped == true) return;

						//if (instance != null) instance.gameObject.SetActive(true);
						if (callback != null) callback.Invoke(this.instance as WindowComponent);

					});

				};

				WindowComponent loadedComponent = null;
				if (this.componentResource.IsLoadable() == true) {

					WindowSystemResources.LoadRefCounter<WindowComponent>(this, (component) => {
						
						loadedComponent = component;
						onLoaded.Invoke(loadedComponent);

						WindowSystemResources.Unload(this, this.GetResource(), resetController: false);

					}, () => {
						
						#if UNITY_EDITOR
						Debug.LogWarningFormat("[ Layout ] Resource request failed {0} [{1}].", UnityEditor.AssetDatabase.GetAssetPath(this.component.GetInstanceID()), window.name);
						#endif

					}, async);
					return;

				} else {

					loadedComponent = this.componentNoResource;
					#if UNITY_EDITOR
					if (loadedComponent != null) Debug.LogWarningFormat("[ Layout ] Resource `{0}` [{1}] should be placed in `Resources` folder to be loaded/unloaded automaticaly. Window `{2}` requested this resource. This warning shown in editor only.", loadedComponent.name, UnityEditor.AssetDatabase.GetAssetPath(loadedComponent.GetInstanceID()), window.name);
					#endif

				}

				onLoaded.Invoke(loadedComponent);

			}

			public void StopCreate() {

				this.stopped = true;

			}

			public void Unload() {

				//Debug.Log("Unload: " + this.componentResource.GetId() + " :: " + this.componentResource.assetPath);

				WindowSystemResources.UnloadResource_INTERNAL(this.GetReference(), this.componentResource);

				if (this.root != null) this.root.OnWindowUnload();

				this.instance = null;
				this.root = null;
				this.window = null;

				/*if (this.componentResource.loadableResource == true) {

					this.componentResource.Unload();

				} else {

					// Resource was not load via Resources.Load(), so it can't be unload properly

				}*/

			}

			public WindowLayoutElement GetContainer() {

				return this.root as WindowLayoutElement;

			}

			public T Get<T>(LayoutTag tag = LayoutTag.None) where T : WindowComponent {
				
				if (tag != LayoutTag.None) {
					
					if (this.tag != tag) return null;
					
				}

				if (this.instance is LinkerComponent) {

					return (this.instance as LinkerComponent).Get<T>();

				}

				return this.instance as T;
				
			}
			
			public float GetDuration(bool forward) {
				
				var root = this.root as IWindowAnimation;
				if (root == null) return 0f;

				return root.GetAnimationDuration(forward);
				
			}

		}

		#region Scale
		public WindowLayout.ScaleMode scaleMode;
		public Vector2 fixedScaleResolution = new Vector2(1024f, 768f);
		public float matchWidthOrHeight = 0f;
		public WindowLayoutPreferences layoutPreferences;
		public bool allowCustomLayoutPreferences = true;
		#endregion

		#region Layout
		#if UNITY_EDITOR
		[BundleIgnore]
		public WindowLayout layout;
		#endif
		[SerializeField] private WindowLayout layoutNoResource;
		[SerializeField] private ResourceAuto layoutResource;
		#endregion

		#region Components
		public Component[] components;
		#endregion

		#region Runtime
		private WindowBase window;
		[System.NonSerialized]
		private IWindowEventsController instance;
		private bool stopped = false;
		#endregion

		public IResourceReference GetReference() {

			return this.window;

		}

		public ResourceBase GetResource() {

			return this.layoutResource;

		}

		public void Create(WindowBase window, Transform root, float depth, int raycastPriority, int orderInLayer, System.Action callback, bool async) {

			if (this.stopped == true) return;

			this.window = window;

			System.Action<WindowLayout> onLoaded = (layout) => {

				if (this.stopped == true) return;

				var instance = layout.Spawn(activeByDefault: false);
				instance.transform.SetParent(root);
				instance.transform.localPosition = Vector3.zero;
				instance.transform.localRotation = Quaternion.identity;
				instance.transform.localScale = Vector3.one;
				
				var rect = instance.transform as RectTransform;
				rect.sizeDelta = (layout.transform as RectTransform).sizeDelta;
				rect.anchoredPosition = (layout.transform as RectTransform).anchoredPosition;

				var layoutPreferences = this.layoutPreferences;
				if (this.allowCustomLayoutPreferences == true) {

					layoutPreferences = WindowSystem.GetCustomLayoutPreferences() ?? this.layoutPreferences;

				}

				instance.Setup(window);
				instance.Init(depth, raycastPriority, orderInLayer);
				instance.SetLayoutPreferences(this.scaleMode, this.fixedScaleResolution, layoutPreferences);

				this.instance = instance;

				ME.Utilities.CallInSequence(() => {

					if (this.stopped == true) return;

					instance.gameObject.SetActive(true);
					callback.Invoke();

				}, this.components, (component, c) => {
					
					component.Create(window, instance.GetRootByTag(component.tag), (comp) => c.Invoke(), async);

				}, waitPrevious: true);

			};

			WindowLayout loadedComponent = null;
			if (this.layoutResource.IsLoadable() == true) {
				
				WindowSystemResources.LoadRefCounter<WindowLayout>(this, (layout) => {
					
					loadedComponent = layout;
					onLoaded.Invoke(loadedComponent);

					WindowSystemResources.Unload(this, this.GetResource(), resetController: false);

				}, () => {

					#if UNITY_EDITOR
					Debug.LogWarningFormat("[ Layout ] Resource request failed {0} [{1}].", UnityEditor.AssetDatabase.GetAssetPath(this.layout.GetInstanceID()), window.name);
					#endif

				}, async);
				return;

			} else {

				loadedComponent = this.layoutNoResource;
				#if UNITY_EDITOR
				if (loadedComponent != null) Debug.LogWarningFormat("[ Layout ] Resource `{0}` [{1}] should be placed in `Resources` folder to be loaded/unloaded automaticaly. Window `{2}` requested this resource. This warning shown in editor only.", loadedComponent.name, UnityEditor.AssetDatabase.GetAssetPath(loadedComponent.GetInstanceID()), window.name);
				#endif

			}

			onLoaded.Invoke(loadedComponent);

		}

		public void StopCreate() {

			//Debug.Log("StopCreate: " + this.window + " :: " + this.window.GetLastState() + " :: " + this.window.preferences.createPool);
			if (this.window != null && (this.window.GetLastState() == WindowObjectState.NotInitialized || this.window.GetLastState() == WindowObjectState.Initializing) && this.window.preferences.createPool == false) {

				this.stopped = true;

				for (int i = 0; i < this.components.Length; ++i) {

					this.components[i].StopCreate();

				}

			}

		}

		public void Unload() {

			for (int i = 0; i < this.components.Length; ++i) {

				this.components[i].Unload();

			}

			WindowSystemResources.UnloadResource_INTERNAL(this.GetReference(), this.layoutResource);

			this.window = null;

		}

		public void SetCustomLayoutPreferences(WindowLayoutPreferences layoutPreferences) {

			if (this.allowCustomLayoutPreferences == true) {

				this.GetLayoutInstance().SetLayoutPreferences(this.scaleMode, this.fixedScaleResolution, layoutPreferences);

			}

		}

		public WindowLayout GetLayoutInstance() {

			return this.instance as WindowLayout;

		}

		public float GetAnimationDuration(bool forward) {
			
			var maxDuration = 0f;
			foreach (var component in this.components) {
				
				var d = component.GetDuration(forward);
				if (d >= maxDuration) {
					
					maxDuration = d;
					
				}
				
			}
			
			return maxDuration;
			
		}
		
		public Transform GetRoot() {

			var instance = this.GetLayoutInstance();
			return (instance != null ? instance.transform : null);
			
		}

		public WindowLayoutElement GetContainer(LayoutTag tag) {

			var component = this.components.FirstOrDefault((c) => c.tag == tag);
			if (component != null) return component.GetContainer();

			return null;

		}

		public T Get<T>(LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			for (var i = 0; i < this.components.Length; ++i) {
				
				var item = this.components[i].Get<T>(tag);
				if (item != null) return item;
				
			}
			
			return default(T);
			
		}

		public int GetSortingOrder() {
			
			return this.GetLayoutInstance().canvas.sortingOrder;
			
		}
		
		// Events
		public void OnLocalizationChanged() {
			
			if (this.instance != null) this.GetLayoutInstance().OnLocalizationChanged();
			
		}
		
		public void OnManualEvent<T>(T data) where T : IManualEvent {
			
			if (this.instance != null) this.GetLayoutInstance().OnManualEvent<T>(data);
			
		}

		public void DoWindowLayoutComplete() {

			if (this.instance != null) this.instance.DoWindowLayoutComplete();

		}

		public void DoWindowOpen() {

			if (this.instance != null) this.instance.DoWindowOpen();

		}

		public void DoWindowClose() {

			if (this.instance != null) this.instance.DoWindowClose();

		}

		public void DoWindowActive() {

			if (this.instance != null) this.instance.DoWindowActive();

		}

		public void DoWindowInactive() {

			if (this.instance != null) this.instance.DoWindowInactive();

		}

		#region Base Events
		// Base Events
		public void DoInit() {

			if (this.instance != null) this.instance.DoInit();

		}

		public void DoDeinit(System.Action callback) {

			if (this.instance != null) this.instance.DoDeinit(callback);

			this.instance = null;
			this.components = null;

		}

		public void DoShowBegin(AppearanceParameters parameters) {

			if (this.instance != null) this.instance.DoShowBegin(parameters);

		}

		public void DoShowEnd(AppearanceParameters parameters) {

			if (this.instance != null) {

				this.instance.DoShowEnd(parameters);
				CanvasUpdater.ForceUpdate(this.GetLayoutInstance().canvas, this.GetLayoutInstance().canvasScaler);

			}

		}

		public void DoHideBegin(AppearanceParameters parameters) {

			this.StopCreate();
			if (this.instance != null) this.instance.DoHideBegin(parameters); 

		}

		public void DoHideEnd(AppearanceParameters parameters) {

			if (this.instance != null) this.instance.DoHideEnd(parameters);

		}

		public void DoWindowUnload() {

			if (this.instance != null) {

				this.instance.DoWindowUnload();
				this.Unload();

			}

		}
		#endregion
		
		public static WindowComponentParametersBase AddParametersFor(WindowBase window, WindowComponent component) {

			return Layout.AddParametersFor(window.gameObject, component);

		}

		public static WindowComponentParametersBase AddParametersFor(GameObject obj, WindowComponent component) {

			if (component == null) return null;

			// Find the type
			var type = System.Type.GetType(string.Format("{0}Parameters", component.GetType().FullName), throwOnError: false, ignoreCase: false);
			if (type == null) return null;

			// Add component
			var instance = obj.AddComponent(type);
			instance.hideFlags = HideFlags.HideInInspector;
			return instance as WindowComponentParametersBase;

		}

		#if UNITY_EDITOR
		private List<LayoutTag> tags = new List<LayoutTag>();
		internal void Update_EDITOR(LayoutWindowType layoutWindow) {
			
			if (this.layout == null) return;
			
			this.layout.GetTags(this.tags);

			foreach (var tag in this.tags) {

				this.AddComponentLink(tag);

			}

			this.components = this.components.Distinct(new ComponentComparer()).ToArray();

			// Used
			for (int i = 0; i < this.components.Length; ++i) {

				this.components[i].OnValidate();

				this.components[i].UpdateDescription(layoutWindow);

				var index = this.tags.IndexOf(this.components[i].tag);
				if (index == -1) {
					
					this.RemoveComponentLink(this.components[i].tag);
					continue;
					
				}
				this.tags.RemoveAt(index);

			}

			this.layoutResource.tempObject = this.layout;
			this.layoutResource.Validate();
			this.layoutNoResource = (this.layoutResource.IsLoadable() == true ? null : this.layout);

		}
		
		private void RemoveComponentLink(LayoutTag tag) {
			
			this.components = this.components.Where((c) => c.tag != tag).ToArray();
			
		}
		
		private void AddComponentLink(LayoutTag tag) {
			
			var list = this.components.ToList();
			list.Add(new Component(tag));
			this.components = list.ToArray();
			
		}
		#endif
		
	}

}