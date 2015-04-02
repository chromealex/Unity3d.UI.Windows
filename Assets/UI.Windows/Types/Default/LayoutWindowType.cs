using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Types {

	[ExecuteInEditMode()]
	public class LayoutWindowType : WindowBase {

		public Layout layout;

		new public LayoutWindowType GetWindow() {
			
			return base.GetWindow() as LayoutWindowType;
			
		}
		
		public void GetLayoutComponent<T>(out T component, LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			component = this.GetLayoutComponent<T>(tag);
			
		}

		public T GetLayoutComponent<T>(LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			return this.layout.Get<T>(tag);
			
		}

		public WindowLayoutElement GetLayoutContainer(LayoutTag tag) {

			return this.layout.GetContainer(tag);

		}

		public override string GetSortingLayerName() {
			
			return this.layout.GetSortingLayerName();
			
		}
		
		public override int GetSortingOrder() {
			
			return this.layout.GetSortingOrder();
			
		}
		
		public override float GetLayoutAnimationDuration(bool forward) {
			
			return this.layout.GetAnimationDuration(forward);
			
		}

		protected override Transform GetLayoutRoot() {
			
			return this.layout.GetRoot();
			
		}

		protected override void OnLayoutInit(float depth, int raycastPriority, int orderInLayer) {
			
			this.layout.Create(this, this.transform, depth, raycastPriority, orderInLayer);
			this.layout.OnInit();

		}
		protected override void OnLayoutDeinit() {

			this.layout.OnDeinit();

		}
		protected override void OnLayoutShowBegin(System.Action callback) {

			this.layout.OnShowBegin(callback);

		}
		protected override void OnLayoutShowEnd() {

			this.layout.OnShowEnd();

		}
		protected override void OnLayoutHideBegin(System.Action callback) {

			this.layout.OnHideBegin(callback);

		}
		protected override void OnLayoutHideEnd() {

			this.layout.OnHideEnd();

		}

		#if UNITY_EDITOR
		public override void OnValidate() {
			
			base.OnValidate();

			this.layout.Update_EDITOR();
			
		}
		#endif

	}
	
	[System.Serializable]
	public class Layout : IWindowEventsAsync {
		
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
		public class Component : IWindowEventsAsync {
			
			#if UNITY_EDITOR
			public string description;
			#endif
			[ReadOnly]
			public LayoutTag tag;
			[ComponentChooser]
			public WindowComponent component;
			public int sortingOrder = 0;
			
			public Component(LayoutTag tag, string description) {
				
				#if UNITY_EDITOR
				this.description = description;
				#endif
				this.tag = tag;
				
			}
			
			#if UNITY_EDITOR
			private IPreviewEditor editor;
			public void OnPreviewGUI(Rect rect, GUIStyle background) {
				
				if (this.component != null) {

					if (this.editor == null) this.editor = UnityEditor.Editor.CreateEditor(this.component) as IPreviewEditor;
					if (this.editor != null) {

						this.editor.OnPreviewGUI(rect, background);

					}

				}
				
			}
			#endif

			[HideInInspector][System.NonSerialized]
			private IWindowEventsAsync instance;
			
			[HideInInspector][System.NonSerialized]
			private IWindowComponentLayout root;

			public IWindowEventsAsync Create(WindowBase window, WindowLayoutElement root) {
				
				this.root = root;

				if (this.component == null && this.root == null) {

					return null;

				}

				if (this.component == null) {

					this.root.Setup(null, this);
					return null;

				}

				var instance = this.component.Spawn();
				instance.SetParent(root, setTransformAsSource: false);
				instance.SetTransformAs();

				var rect = instance.transform as RectTransform;
				rect.sizeDelta = (this.component.transform as RectTransform).sizeDelta;
				rect.anchoredPosition = (this.component.transform as RectTransform).anchoredPosition;
				
				this.root.Setup(instance, this);
				instance.Setup(window);
				
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
				
				if (this.component == null) return 0f;

				var root = this.root as IWindowAnimation;
				if (root == null) return 0f;

				return root.GetAnimationDuration(forward);
				
			}
			
			public void OnInit() {
				
				if (this.instance != null) this.instance.OnInit();
				if (this.root != null) this.root.OnInit();
				
			}
			
			public void OnDeinit() {

				if (this.instance != null) this.instance.OnDeinit();
				if (this.root != null) this.root.OnDeinit();
				
			}
			
			public void OnShowBegin(System.Action callback) {

				ME.Utilities.CallInSequence(callback, (e, c) => { e.OnShowBegin(c); }, this.instance, this.root);

			}
			
			public void OnShowEnd() {
				
				if (this.instance != null) this.instance.OnShowEnd();
				if (this.root != null) this.root.OnShowEnd();
				
			}
			
			public void OnHideBegin(System.Action callback) {

				ME.Utilities.CallInSequence(callback, (e, c) => { e.OnHideBegin(c); }, this.instance, this.root);

			}
			
			public void OnHideEnd() {
				
				if (this.instance != null) this.instance.OnHideEnd();
				if (this.root != null) this.root.OnHideEnd();
				
			}
			
		}

		public WindowLayout.ScaleMode scaleMode;

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
			instance.Init(depth, raycastPriority, orderInLayer, this.scaleMode);
			
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
		
		public string GetSortingLayerName() {
			
			return this.instance.canvas.sortingLayerName;
			
		}
		
		public int GetSortingOrder() {
			
			return this.instance.canvas.sortingOrder;
			
		}
		
		// Events
		public void OnInit() { this.instance.OnInit(); foreach (var component in this.components) component.OnInit(); }
		public void OnDeinit() { this.instance.OnDeinit(); foreach (var component in this.components) component.OnDeinit(); }
		public void OnShowBegin(System.Action callback) {

			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < 2) return;
				
				if (callback != null) callback();
				
			};

			this.instance.OnShowBegin(callbackItem); 
			
			ME.Utilities.CallInSequence(callbackItem, this.components, (e, c) => { e.OnShowBegin(c); });

		}
		public void OnShowEnd() { this.instance.OnShowEnd(); foreach (var component in this.components) component.OnShowEnd(); }
		public void OnHideBegin(System.Action callback) {
			
			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < 2) return;
				
				if (callback != null) callback();
				
			};

			this.instance.OnHideBegin(callbackItem); 
			
			ME.Utilities.CallInSequence(callbackItem, this.components, (e, c) => { e.OnHideBegin(c); });

		}
		public void OnHideEnd() { this.instance.OnHideEnd(); foreach (var component in this.components) component.OnHideEnd(); }
		
		#if UNITY_EDITOR
		private List<LayoutTag> tags = new List<LayoutTag>();
		private List<string> descriptions = new List<string>();
		internal void Update_EDITOR() {
			
			if (this.layout == null) return;
			
			this.layout.GetTags(this.tags, this.descriptions);
			
			// Used
			for (int i = 0; i < this.components.Length; ++i) {
				
				var index = this.tags.IndexOf(this.components[i].tag);
				if (index == -1) {
					
					this.RemoveComponentLink(this.components[i].tag);
					continue;
					
				}
				this.tags.RemoveAt(index);
				this.descriptions.RemoveAt(index);
				
			}
			
			var j = 0;
			foreach (var tag in this.tags) {
				
				this.AddComponentLink(tag, this.descriptions[j++]);
				
			}
			
			this.components = this.components.Distinct(new ComponentComparer()).ToArray();
			
		}
		
		private void RemoveComponentLink(LayoutTag tag) {
			
			this.components = this.components.Where((c) => c.tag != tag).ToArray();
			
		}
		
		private void AddComponentLink(LayoutTag tag, string description) {
			
			var list = this.components.ToList();
			list.Add(new Component(tag, description));
			this.components = list.ToArray();
			
		}
		#endif
		
	}

}