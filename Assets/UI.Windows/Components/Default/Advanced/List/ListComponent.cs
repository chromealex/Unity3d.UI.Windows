using UnityEngine;
using UnityEngine.Extensions;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.EventSystems;
using System.Collections;

namespace UnityEngine.UI.Windows.Components {
	
	public class ListComponent : WindowComponent {
		
		[Header("List")]
		public List<WindowComponent> list = new List<WindowComponent>();

		[Header("Required")]
		public WindowComponent source;

		[Header("Optional")]
		public Extensions.ScrollRect scrollRect;
		public GameObject content;
		public GameObject noElements;
		public GameObject loading;

		[Header("Navigation")]
		public Navigation.Mode navigationMode = Navigation.Mode.None;
		public bool navigationInverse = false;

		public override void OnInit() {

			base.OnInit();

			this.Refresh();
			if (this.noElements != null) this.noElements.SetActive(false);
			if (this.loading != null) this.loading.SetActive(true);

		}

		/*public override void OnDeinit() {

			this.Clear();

			base.OnDeinit();

		}*/

		public void InitPool(int capacity) {

			this.source.CreatePool(capacity, this.transform);
			if (this.source is LinkerComponent) {

				(this.source as LinkerComponent).InitPool(capacity);

			}

		}

		/*public void SetupAsDropdown(float maxHeight) {

			this.scrollRect.SetupAsDropdown(maxHeight);

		}*/

		public virtual WindowComponent GetInstance(int index) {

			var instance = this.source.Spawn();
			instance.Setup(this.GetLayoutRoot());
			instance.Setup(this.GetWindow());
			this.list.Add(instance);
			this.RegisterSubComponent(instance);
			if (this.loading != null) this.loading.SetActive(false);

			return instance;

		}

		private ISelectable lastSelectableInstance;
		public virtual T AddItem<T>() where T : IComponent {

			return this.AddItem<T>(this.navigationMode, this.navigationInverse);

		}

		public virtual T AddItem<T>(Navigation.Mode navigationMode, bool navigationInverse) where T : IComponent {

			if (this.source == null) return default(T);

			var instance = this.GetInstance(this.Count());
			if (instance == null) return default(T);

			if (this.scrollRect != null && this.scrollRect.content != null) {

				instance.SetParent(this.scrollRect.content, setTransformAsSource: false);

			} else if (this.content != null) {

				instance.SetParent(this.content.transform, setTransformAsSource: false);

			}

			if (instance is LinkerComponent) {

				//instance.OnInit();
				instance.gameObject.SetActive(true);
				instance = (instance as LinkerComponent).Get<WindowComponent>();

			}

			if (instance != null) {

				instance.gameObject.SetActive(true);

			}

			#region Navigation
			if (this.lastSelectableInstance != instance) {

				if (navigationMode != Navigation.Mode.None) {

					var selectableComp = instance as ISelectable;
					if (selectableComp != null) {

						var selectable = selectableComp.GetSelectable();
						if (selectable != null) {

							var nav = selectable.navigation;

							if (navigationMode == Navigation.Mode.Automatic) {
								
								nav.mode = Navigation.Mode.Automatic;
								
								selectable.navigation = nav;

							} else {

								nav.mode = Navigation.Mode.Explicit;

								if (navigationMode == Navigation.Mode.Vertical ||
									navigationMode == Navigation.Mode.Horizontal) {

									var prevSelectableComp = this.lastSelectableInstance;
									if (prevSelectableComp != null) {

										var prevSelectable = prevSelectableComp.GetSelectable();
										if (prevSelectable != null) {

											nav = new Navigation();
											nav.mode = Navigation.Mode.Explicit;
											nav.selectOnDown = selectable.navigation.selectOnDown;
											nav.selectOnLeft = selectable.navigation.selectOnLeft;
											nav.selectOnRight = selectable.navigation.selectOnRight;
											nav.selectOnUp = selectable.navigation.selectOnUp;

											if (navigationInverse == false) {

												if (navigationMode == Navigation.Mode.Horizontal) {
													
													nav.selectOnLeft = prevSelectable;

												} else {

													nav.selectOnDown = prevSelectable;

												}

											} else {
												
												if (navigationMode == Navigation.Mode.Horizontal) {
													
													nav.selectOnRight = prevSelectable;

												} else {

													nav.selectOnUp = prevSelectable;

												}

											}
											selectable.navigation = nav;

											nav = new Navigation();
											nav.mode = Navigation.Mode.Explicit;
											nav.selectOnDown = prevSelectable.navigation.selectOnDown;
											nav.selectOnLeft = prevSelectable.navigation.selectOnLeft;
											nav.selectOnRight = prevSelectable.navigation.selectOnRight;
											nav.selectOnUp = prevSelectable.navigation.selectOnUp;

											if (navigationInverse == false) {
												
												if (navigationMode == Navigation.Mode.Horizontal) {
													
													nav.selectOnRight = selectable;

												} else {

													nav.selectOnUp = selectable;
													
												}

											} else {
													
												if (navigationMode == Navigation.Mode.Horizontal) {
													
													nav.selectOnLeft = selectable;

												} else {

													nav.selectOnDown = selectable;

												}

											}
											prevSelectable.navigation = nav;

										} // Last selectable found

									} // Last component not null

								} // Vertical or Horizontal

							} // Mode not Auto

						} // Selectable found

					} // Component not null
					
					this.lastSelectableInstance = selectableComp;

				} // Navigation not None

			}
			#endregion

			this.Refresh();

			return (T)(instance as IComponent);

		}

		public int GetIndexOf<T>(T item) where T : IComponent {
			
			return this.GetItems().FindIndex((c) => {

				if (c is LinkerComponent) {

					return (c as LinkerComponent).Get<WindowComponent>() == (item as WindowComponent);

				}

				return (item as WindowComponent) == c;

			});

		}

		public virtual bool IsEmpty() {

			return this.Count() == 0;

		}

		public int Count() {

			return this.list.Count;

		}

		public List<WindowComponent> GetItems() {
			
			return this.list;
			
		}
		
		public List<T> GetItems<T>() where T : IComponent {
			
			return this.list.Cast<T>().ToList();
			
		}
		
		public virtual WindowComponent GetItem(int index) {

			if (this.list[index] is LinkerComponent) {
				
				return (this.list[index] as LinkerComponent).Get<WindowComponent>();
				
			}

			return this.list[index];
			
		}

		public virtual T GetItem<T>(int index) where T : IComponent {

			return (T)(this.GetItem(index) as IComponent);
			
		}
		
		public void ForEach<T>(UnityAction<T, int> onItem = null) where T : IComponent {

			for (int i = 0, capacity = this.Count(); i < capacity; ++i) {

				var instance = this.GetItem<T>(i);
				if (instance != null && onItem != null) onItem.Invoke(instance, i);
				
			}

		}

		public void SetItems<T>(int capacity, UnityAction<T, int> onItem = null) where T : IComponent {

			this.SetItems(capacity, (element, index) => {

				if (onItem != null) onItem((T)element, index);

			});

		}
		
		protected virtual void SetItems(int capacity, UnityAction<IComponent, int> onItem) {
			
			this.Clear();
			
			for (int i = 0; i < capacity; ++i) {
				
				var instance = this.AddItem<IComponent>();
				if (instance != null && onItem != null) onItem.Invoke(instance, i);
				
			}

			this.Refresh(withNoElements: true);

		}

		public virtual void SetItemsAsync<T>(int capacity, UnityAction onComplete, UnityAction<T, int> onItem = null) where T : IComponent {
			
			this.Clear();

			this.StopAllCoroutines();
			this.StartCoroutine(this.SetItemsAsync_INTERNAL(capacity, onComplete, onItem));

		}

		private IEnumerator SetItemsAsync_INTERNAL<T>(int capacity, UnityAction onComplete, UnityAction<T, int> onItem = null) where T : IComponent {

			for (int i = 0; i < capacity; ++i) {

				var instance = this.AddItem<T>();
				if (instance != null && onItem != null) onItem.Invoke(instance, i);

				yield return false;
				
			}

			if (onComplete != null) onComplete.Invoke();
			
			this.Refresh(withNoElements: true);

		}

		public virtual void Clear() {

			this.StopAllCoroutines();

			foreach (var element in this.list) {

				this.UnregisterSubComponent(element);
				element.Recycle();

			}
			this.list.Clear();
			
			this.Refresh(withNoElements: true);

		}

		public void Refresh(bool withNoElements = false) {

			if (withNoElements == true && this.noElements != null) this.noElements.SetActive(this.IsEmpty() == true);
			if (this.content != null) this.content.SetActive(this.IsEmpty() == false);

			if (this.IsEmpty() == false) {

				if (this.loading != null) this.loading.SetActive(false);

			}

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			if (this.autoRegisterSubComponents == true) {

				this.UnregisterSubComponent(this.source);

			}

			//this.scrollRect = this.GetComponentInChildren<ScrollRect>();

		}
		#endif
		
	}
	
}