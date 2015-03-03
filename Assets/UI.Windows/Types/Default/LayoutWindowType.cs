using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows {

	[ExecuteInEditMode()]
	public class LayoutWindowType : WindowBase {

		public Layout layout;
		
		new public LayoutWindowType GetWindow() {
			
			return base.GetWindow() as LayoutWindowType;
			
		}
		
		public T GetLayoutComponent<T>(LayoutTag tag = LayoutTag.None) where T : WindowComponent {
			
			return this.layout.Get<T>(tag);
			
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
		protected override void Update() {
			
			base.Update();
			
			if (Application.isPlaying == true) return;
			
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
			public WindowComponent component;
			public int sortingOrder = 0;
			
			public Component(LayoutTag tag, string description) {
				
				#if UNITY_EDITOR
				this.description = description;
				#endif
				this.tag = tag;
				
			}
			
			[HideInInspector][System.NonSerialized]
			private IWindowEvents instance;
			
			[HideInInspector][System.NonSerialized]
			private IWindowAnimation root;
			
			public void Create(WindowBase window, WindowLayoutElement root) {
				
				if (this.component == null) return;
				
				this.root = root;
				
				var instance = this.component.Spawn();
				instance.transform.SetParent(root.transform);
				instance.transform.localPosition = Vector3.zero;
				instance.transform.localRotation = Quaternion.identity;
				instance.transform.localScale = Vector3.one;
				
				var rect = instance.transform as RectTransform;
				rect.sizeDelta = (this.component.transform as RectTransform).sizeDelta;
				rect.anchoredPosition = (this.component.transform as RectTransform).anchoredPosition;
				
				this.root.Setup(instance);
				instance.Setup(window);
				
				instance.transform.SetSiblingIndex(this.sortingOrder);
				
				this.instance = instance;
				
			}
			
			public T Get<T>(LayoutTag tag = LayoutTag.None) where T : WindowComponent {
				
				if (tag != LayoutTag.None) {
					
					if (this.tag != tag) return null;
					
				}
				
				return this.instance as T;
				
			}
			
			public float GetDuration(bool forward) {
				
				if (this.component == null) return 0f;
				
				return this.root.GetAnimationDuration(forward);
				
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
				
				if (this.instance != null) this.instance.OnShowBegin();
				if (this.root != null) this.root.OnShowBegin(callback);
				
			}
			
			public void OnShowEnd() {
				
				if (this.instance != null) this.instance.OnShowEnd();
				if (this.root != null) this.root.OnShowEnd();
				
			}
			
			public void OnHideBegin(System.Action callback) {
				
				if (this.instance != null) this.instance.OnHideBegin();
				if (this.root != null) this.root.OnHideBegin(callback);
				
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
			
			this.instance.OnShowBegin(); 
			
			ME.Utilities.CallInSequence(callback, this.components, (e, c) => { e.OnShowBegin(c); });
			/*
			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < this.components.Length) return;
				
				if (callback != null) callback();
				
			};
			
			foreach (var component in this.components) {
				
				component.OnShowBegin(callbackItem);
				
			}

			if (this.components.Length == 0 && callback != null) callback();
			*/
		}
		public void OnShowEnd() { this.instance.OnShowEnd(); foreach (var component in this.components) component.OnShowEnd(); }
		public void OnHideBegin(System.Action callback) {
			
			this.instance.OnHideBegin(); 
			
			ME.Utilities.CallInSequence(callback, this.components, (e, c) => { e.OnHideBegin(c); });
			/*
			var counter = 0;
			System.Action callbackItem = () => {
				
				++counter;
				if (counter < this.components.Length) return;
				
				if (callback != null) callback();
				
			};
			
			foreach (var component in this.components) {
				
				component.OnHideBegin(callbackItem);
				
			}
			
			if (this.components.Length == 0 && callback != null) callback();
*/
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