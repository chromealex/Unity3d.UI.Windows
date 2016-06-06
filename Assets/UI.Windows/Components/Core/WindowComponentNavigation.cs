using UnityEngine;
using UnityEngine.UI.Windows.Components;

namespace UnityEngine.UI.Windows {

	public interface IWindowNavigation : IComponent {

		T NavigationFindLeft<T>(bool interactableOnly = false) where T : IComponent;
		T NavigationFindRight<T>(bool interactableOnly = false) where T : IComponent;
		T NavigationFindUp<T>(bool interactableOnly = false) where T : IComponent;
		T NavigationFindDown<T>(bool interactableOnly = false) where T : IComponent;
		WindowComponentNavigation NavigationFindLeft(bool interactableOnly = false);
		WindowComponentNavigation NavigationFindRight(bool interactableOnly = false);
		WindowComponentNavigation NavigationFindUp(bool interactableOnly = false);
		WindowComponentNavigation NavigationFindDown(bool interactableOnly = false);
		WindowComponentNavigation NavigationFind(NavigationSide side, bool interactableOnly = false);
		WindowComponentNavigation GetNavigation(bool interactableOnly, System.Func<WindowComponentNavigation, WindowComponentNavigation> getter);
		void SetNavigation(NavigationSource source);
		void NavigationConnectToLinker(LinkerComponent linker);

	}

	public enum NavigationSide : byte {

		Left,
		Right,
		Up,
		Down,

	};

	[System.Serializable]
	public struct NavigationSource {

		public int childIndex;
		public WindowComponentNavigation left;
		public WindowComponentNavigation right;
		public WindowComponentNavigation up;
		public WindowComponentNavigation down;

	};

	public class WindowComponentNavigation : WindowObjectElement, IWindowNavigation {
		
		public NavigationSource navigation;

		public void NavigationConnectToLinker(LinkerComponent linker) {

			var nav = linker.Get<IWindowNavigation>();
			if (nav != null) {

				nav.SetNavigation(this.navigation);

			}

		}

		public int GetNavigationIndex() {

			return this.navigation.childIndex;

		}

		public void SetNavigation(NavigationSource source) {

			this.navigation = source;

		}

		public T NavigationFindLeft<T>(bool interactableOnly = false) where T : IComponent {

			var element = this.NavigationFind(NavigationSide.Left, interactableOnly);
			if (element is LinkerComponent) {

				return (element as LinkerComponent).Get<T>();

			}

			return (T)(element as IComponent);

		}

		public T NavigationFindRight<T>(bool interactableOnly = false) where T : IComponent {

			var element = this.NavigationFind(NavigationSide.Right, interactableOnly);
			if (element is LinkerComponent) {

				return (element as LinkerComponent).Get<T>();

			}

			return (T)(element as IComponent);

		}

		public T NavigationFindUp<T>(bool interactableOnly = false) where T : IComponent {

			var element = this.NavigationFind(NavigationSide.Up, interactableOnly);
			if (element is LinkerComponent) {

				return (element as LinkerComponent).Get<T>();

			}

			return (T)(element as IComponent);

		}

		public T NavigationFindDown<T>(bool interactableOnly = false) where T : IComponent {

			var element = this.NavigationFind(NavigationSide.Down, interactableOnly);
			if (element is LinkerComponent) {

				return (element as LinkerComponent).Get<T>();

			}

			return (T)(element as IComponent);

		}

		public WindowComponentNavigation NavigationFindLeft(bool interactableOnly = false) {

			return this.GetNavigation(interactableOnly, WindowComponentNavigation.GetNavigationLeft_INTERNAL);

		}

		public WindowComponentNavigation NavigationFindRight(bool interactableOnly = false) {

			return this.GetNavigation(interactableOnly, WindowComponentNavigation.GetNavigationRight_INTERNAL);

		}

		public WindowComponentNavigation NavigationFindUp(bool interactableOnly = false) {

			return this.GetNavigation(interactableOnly, WindowComponentNavigation.GetNavigationUp_INTERNAL);

		}

		public WindowComponentNavigation NavigationFindDown(bool interactableOnly = false) {

			return this.GetNavigation(interactableOnly, WindowComponentNavigation.GetNavigationDown_INTERNAL);

		}

		public WindowComponentNavigation GetNavigation(bool interactableOnly, System.Func<WindowComponentNavigation, WindowComponentNavigation> getter) {

			var item = getter.Invoke(this);
			if (interactableOnly == true) {
				
				while (item != null) {

					IButtonComponent button = null;
					if (item is LinkerComponent) {

						button = (item as LinkerComponent).Get<IButtonComponent>();

					} else {

						button = item as IButtonComponent;

					}

					if (button == null || button.IsInteractable() == false) {
						
						item = getter.Invoke(item);

					} else {

						break;

					}

				}

			}

			if (item == null) {

				// search in root element
				if (this.rootComponent != null && this.rootComponent is IWindowNavigation) {

					item = (this.rootComponent as IWindowNavigation).GetNavigation(interactableOnly, getter);

				}

			}

			return item;

		}

		private static WindowComponentNavigation GetNavigationLeft_INTERNAL(WindowComponentNavigation nav) {

			return nav.navigation.left;

		}

		private static WindowComponentNavigation GetNavigationRight_INTERNAL(WindowComponentNavigation nav) {

			return nav.navigation.right;

		}

		private static WindowComponentNavigation GetNavigationUp_INTERNAL(WindowComponentNavigation nav) {

			return nav.navigation.up;

		}

		private static WindowComponentNavigation GetNavigationDown_INTERNAL(WindowComponentNavigation nav) {

			return nav.navigation.down;

		}

		public WindowComponentNavigation NavigationFind(NavigationSide side, bool interactableOnly = false) {

			WindowComponentNavigation result = null;
			switch (side) {

				case NavigationSide.Left:
					result = this.GetNavigation(interactableOnly, WindowComponentNavigation.GetNavigationLeft_INTERNAL);
					break;

				case NavigationSide.Right:
					result = this.GetNavigation(interactableOnly, WindowComponentNavigation.GetNavigationRight_INTERNAL);
					break;

				case NavigationSide.Up:
					result = this.GetNavigation(interactableOnly, WindowComponentNavigation.GetNavigationUp_INTERNAL);
					break;

				case NavigationSide.Down:
					result = this.GetNavigation(interactableOnly, WindowComponentNavigation.GetNavigationDown_INTERNAL);
					break;

			}

			return result;

		}

		#if UNITY_EDITOR
		[ContextMenu("Navigation: Clear Horizontal")]
		public void EditorClearHorizontal() {

			var root = this.transform.parent;
			for (int i = 0; i < root.childCount; ++i) {

				var child = root.GetChild(i);
				var nav = child.GetComponent<WindowComponentNavigation>();
				if (nav == null) continue;

				var navigation = nav.navigation;
				navigation.childIndex = i;
				navigation.left = null;
				navigation.right = null;
				nav.navigation = navigation;

			}

		}

		[ContextMenu("Navigation: Clear Vertical")]
		public void EditorClearVerical() {

			var root = this.transform.parent;
			for (int i = 0; i < root.childCount; ++i) {

				var child = root.GetChild(i);
				var nav = child.GetComponent<WindowComponentNavigation>();
				if (nav == null) continue;

				var navigation = nav.navigation;
				navigation.childIndex = i;
				navigation.up = null;
				navigation.down = null;
				nav.navigation = navigation;

			}

		}

		[ContextMenu("Navigation: Find&Setup Horizontal")]
		public void EditorFindHorizontal() {

			this.EditorFindHorizontal_INTERNAL(inverse: false);

		}

		[ContextMenu("Navigation: Find&Setup Horizontal (inversed)")]
		public void EditorFindHorizontalInversed() {

			this.EditorFindHorizontal_INTERNAL(inverse: true);

		}

		public void EditorFindHorizontal_INTERNAL(bool inverse) {
			
			WindowComponentNavigation prev = null;
			WindowComponentNavigation next = null;

			var root = this.transform.parent;
			for (int i = 0; i < root.childCount; ++i) {

				var child = root.GetChild(i);
				var nav = child.GetComponent<WindowComponentNavigation>();
				if (nav == null) continue;

				if (i > 0) {

					prev = root.GetChild(i - 1).GetComponent<WindowComponentNavigation>();

				} else {

					prev = null;

				}

				if (i < root.childCount - 1) {

					next = root.GetChild(i + 1).GetComponent<WindowComponentNavigation>();

				} else {

					next = null;

				}

				var navigation = nav.navigation;
				navigation.childIndex = i;
				if (inverse == true) {

					navigation.left = next;
					navigation.right = prev;

				} else {

					navigation.left = prev;
					navigation.right = next;

				}
				nav.navigation = navigation;

			}

		}

		[ContextMenu("Navigation: Find&Setup Vertical")]
		public void EditorFindVertical() {

			this.EditorFindVertical_INTERNAL(inverse: false);

		}

		[ContextMenu("Navigation: Find&Setup Vertical (inversed)")]
		public void EditorFindVerticalInversed() {

			this.EditorFindVertical_INTERNAL(inverse: true);

		}

		private void EditorFindVertical_INTERNAL(bool inverse) {
			
			WindowComponentNavigation prev = null;
			WindowComponentNavigation next = null;

			var root = this.transform.parent;
			for (int i = 0; i < root.childCount; ++i) {

				var child = root.GetChild(i);
				var nav = child.GetComponent<WindowComponentNavigation>();
				if (nav == null) continue;

				if (i > 0) {

					prev = root.GetChild(i - 1).GetComponent<WindowComponentNavigation>();

				} else {

					prev = null;

				}

				if (i < root.childCount - 1) {
					
					next = root.GetChild(i + 1).GetComponent<WindowComponentNavigation>();

				} else {

					next = null;

				}

				var navigation = nav.navigation;
				navigation.childIndex = i;
				if (inverse == true) {

					navigation.up = next;
					navigation.down = prev;

				} else {
					
					navigation.up = prev;
					navigation.down = next;

				}
				nav.navigation = navigation;

			}

		}
		#endif

	}

}