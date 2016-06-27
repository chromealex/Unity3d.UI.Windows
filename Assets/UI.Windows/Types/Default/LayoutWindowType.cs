using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Extensions;

namespace UnityEngine.UI.Windows.Types {

	[ExecuteInEditMode()]
	public class LayoutWindowType : WindowBase {

		public Layout layout;

		new public LayoutWindowType GetWindow() {
			
			return base.GetWindow() as LayoutWindowType;
			
		}
		
		public override Canvas GetCanvas() {
			
			return this.layout.GetLayoutInstance().canvas;
			
		}

		public void GetLayoutComponent<T>(out T component, LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			component = this.GetLayoutComponent<T>(tag);
			
		}

		public T GetLayoutComponent<T>(LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			return this.layout.Get<T>(tag);
			
		}
		
		public override Vector2 GetSize() {

			return this.layout.GetLayoutInstance().GetSize();
			
		}

		public WindowLayoutElement GetLayoutContainer(LayoutTag tag) {

			return this.layout.GetContainer(tag);

		}

		public override int GetSortingOrder() {
			
			return this.layout.GetSortingOrder();
			
		}
		
		public override float GetLayoutAnimationDuration(bool forward) {
			
			return this.layout.GetAnimationDuration(forward);
			
		}
		
		public override Rect GetRect() {

			var bounds = new Rect();
			//var root = this.layout.GetLayoutInstance().root;
			var baseSubElements = this.layout.GetLayoutInstance().GetSubComponents();
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
			
			return this.layout.GetRoot();
			
		}

		protected override void MoveLayout(Vector2 delta) {

			this.layout.GetLayoutInstance().root.Move(delta);

		}
		
		public override void OnLocalizationChanged() {
			
			base.OnLocalizationChanged();
			
			this.layout.OnLocalizationChanged();
			
		}

		public override void OnManualEvent<T>(T data) {

			base.OnManualEvent<T>(data);

			this.layout.OnManualEvent<T>(data);

		}

		protected override void DoLayoutInit(float depth, int raycastPriority, int orderInLayer) {

			this.layout.Create(this, this.transform, depth, raycastPriority, orderInLayer);
			this.layout.DoInit();

		}

		protected override void DoLayoutDeinit() {

			this.layout.DoDeinit();

		}

		protected override void DoLayoutActive() {

			this.layout.DoWindowActive();

		}

		protected override void DoLayoutInactive() {

			this.layout.DoWindowInactive();

		}

		protected override void DoLayoutShowBegin(AppearanceParameters parameters) {

			this.layout.DoWindowOpen();
			this.layout.DoShowBegin(parameters);

		}

		protected override void DoLayoutShowEnd(AppearanceParameters parameters) {

			this.layout.DoShowEnd(parameters);

		}

		protected override void DoLayoutHideBegin(AppearanceParameters parameters) {
			
			this.layout.DoWindowClose();
			this.layout.DoHideBegin(parameters);

		}

		protected override void DoLayoutHideEnd(AppearanceParameters parameters) {

			this.layout.DoHideEnd(parameters);

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {
			
			base.OnValidateEditor();

			if (this.layout == null) return;

			this.layout.Update_EDITOR(this);
			
		}
		#endif

	}
	
	[System.Serializable]
	public class Layout : IWindowEventsController {
		
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
		public class Component {
			
			#if UNITY_EDITOR
			public string description;
			public string GetDescription(LayoutWindowType layoutWindow) {

				if (layoutWindow != null &&
				    layoutWindow.layout.layout != null) {

					var element = layoutWindow.layout.layout.GetRootByTag(this.tag);
					if (element != null) return element.comment;

				}

				return string.Empty;

			}
			#endif

			[ReadOnly]
			public LayoutTag tag;
			#if UNITY_EDITOR
			public WindowComponent component;
			#endif
			public MonoResource componentResource;
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
			
			#if UNITY_EDITOR
			public WindowComponentParametersBase OnComponentChanged(WindowBase window, WindowComponent newComponent) {

				var hasChanged = (newComponent != this.component);
				this.component = newComponent;

				WindowComponentParametersBase instance = null;
				if (hasChanged == true) {
					
					if (this.componentParameters != null) {
						
						var link = this.componentParameters;
						//UnityEditor.EditorApplication.delayCall += () => {
						
						Object.DestroyImmediate(link, allowDestroyingAssets: true);
						
						//};
						
					}

					instance = Layout.AddParametersFor(window, this.component);
					this.componentParameters = instance;

					this.OnValidate();

				} else {

					instance = this.componentParameters;

				}

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

				this.componentResource.Validate(this.component);

				if (this.componentResource.loadableResource == false) {

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

			public void SetComponent(WindowComponent component) {

				this.componentNoResource = component;

			}

			public IWindowEventsAsync Create(WindowBase window, WindowLayoutElement root) {
				
				this.root = root;

				WindowComponent component = null;
				if (this.componentResource.loadableResource == true) {

					component = this.componentResource.Load<WindowComponent>();
					if (component == null) {

						Debug.LogError("[ Layout ] Be sure your UI project placed in Resources folder (ex: Assets/Resources/MyProject/UIProject.asset).");

					}

				} else {

					component = this.componentNoResource;

				}

				if (component == null && this.root == null) {

					return null;

				}

				if (component == null) {

					this.root.Setup(null, this);
					return null;

				}

				var instance = component.Spawn();
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

				return instance;

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

		public WindowLayout.ScaleMode scaleMode;
		public Vector2 fixedScaleResolution = new Vector2(1024f, 768f);
		public float matchWidthOrHeight = 0f;
		public WindowLayoutPreferences layoutPreferences;

		public WindowLayout layout;
		public Component[] components;
		
		[System.NonSerialized]
		private WindowLayout instance;
		
		public void Create(WindowBase window, Transform root, float depth, int raycastPriority, int orderInLayer) {
			
			var instance = this.layout.Spawn();
			instance.transform.SetParent(root);
			instance.transform.localPosition = Vector3.zero;
			instance.transform.localRotation = Quaternion.identity;
			instance.transform.localScale = Vector3.one;
			
			var rect = instance.transform as RectTransform;
			rect.sizeDelta = (this.layout.transform as RectTransform).sizeDelta;
			rect.anchoredPosition = (this.layout.transform as RectTransform).anchoredPosition;
			
			instance.Setup(window);
			instance.Init(depth, raycastPriority, orderInLayer, this.scaleMode, this.fixedScaleResolution, this.layoutPreferences);
			
			this.instance = instance;
			
			foreach (var component in this.components) component.Create(window, instance.GetRootByTag(component.tag));

		}

		public WindowLayout GetLayoutInstance() {

			return this.instance;

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
			
			return this.instance.transform;
			
		}

		public WindowLayoutElement GetContainer(LayoutTag tag) {

			var component = this.components.FirstOrDefault((c) => c.tag == tag);
			if (component != null) return component.GetContainer();

			return null;

		}

		public T Get<T>(LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			foreach (var component in this.components) {
				
				var item = component.Get<T>(tag);
				if (item != null) return item;
				
			}
			
			return default(T);
			
		}

		public int GetSortingOrder() {
			
			return this.instance.canvas.sortingOrder;
			
		}
		
		// Events
		public void OnLocalizationChanged() {
			
			this.instance.OnLocalizationChanged();
			
		}
		
		public void OnManualEvent<T>(T data) where T : IManualEvent {
			
			this.instance.OnManualEvent<T>(data);
			
		}

		public void DoWindowOpen() {

			this.instance.DoWindowOpen();

		}

		public void DoWindowClose() {

			this.instance.DoWindowClose();

		}

		public void DoWindowActive() {

			this.instance.DoWindowActive();

		}

		public void DoWindowInactive() {

			this.instance.DoWindowInactive();

		}

		#region Base Events
		// Base Events
		public void DoInit() {

			this.instance.DoInit();

		}

		public void DoDeinit() {
			
			if (this.instance != null) this.instance.DoDeinit();

		}

		public void DoShowBegin(AppearanceParameters parameters) {

			this.instance.DoShowBegin(parameters);

		}

		public void DoShowEnd(AppearanceParameters parameters) {

			this.instance.DoShowEnd(parameters);
			
			CanvasUpdater.ForceUpdate(this.instance.canvas, this.instance.canvasScaler);

		}

		public void DoHideBegin(AppearanceParameters parameters) {

			this.instance.DoHideBegin(parameters); 

		}

		public void DoHideEnd(AppearanceParameters parameters) {

			this.instance.DoHideEnd(parameters);

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
			
			// Used
			for (int i = 0; i < this.components.Length; ++i) {

				this.components[i].OnValidate();

				this.components[i].description = this.components[i].GetDescription(layoutWindow);

				var index = this.tags.IndexOf(this.components[i].tag);
				if (index == -1) {
					
					this.RemoveComponentLink(this.components[i].tag);
					continue;
					
				}
				this.tags.RemoveAt(index);

			}

			foreach (var tag in this.tags) {
				
				this.AddComponentLink(tag);
				
			}
			
			this.components = this.components.Distinct(new ComponentComparer()).ToArray();

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