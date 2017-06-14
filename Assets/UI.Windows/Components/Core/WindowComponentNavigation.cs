using UnityEngine;
using UnityEngine.UI.Windows.Components;
using System.Linq;
using System.Collections.Generic;
using ME;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows {

	public interface IWindowNavigation : IHoverableComponent {

		/*T NavigationFindLeft<T>(bool interactableOnly = false, bool lookupRootOnFail = true) where T : IComponent;
		T NavigationFindRight<T>(bool interactableOnly = false, bool lookupRootOnFail = true) where T : IComponent;
		T NavigationFindUp<T>(bool interactableOnly = false, bool lookupRootOnFail = true) where T : IComponent;
		T NavigationFindDown<T>(bool interactableOnly = false, bool lookupRootOnFail = true) where T : IComponent;*/
		WindowComponentNavigation NavigationFindLeft();
		WindowComponentNavigation NavigationFindRight();
		WindowComponentNavigation NavigationFindUp();
		WindowComponentNavigation NavigationFindDown();
		//WindowComponentNavigation NavigationFind(NavigationSide side, bool interactableOnly = false, bool lookupRootOnFail = true);
		WindowComponentNavigation GetNavigation(System.Func<WindowComponentNavigation, WindowComponentNavigation> getter);
		WindowComponentNavigation GetNavigation(Vector3 dir, NavigationSideInfo sideInfo);
		WindowComponentNavigation GetNavigation(NavigationSide side);
		void SetNavigationInfo(NavigationSide side, NavigationSideInfo info);
		void NavigationConnectToLinker(LinkerComponent linker);
		NavigationType GetNavigationType();
		bool IsNavigateOnDisabled();
		bool HasNavigationCustomSelector();
		WindowComponent GetNavigationCustomSelectorObject();

		bool IsNavigationPreventChildEvents(NavigationSide side);
		bool NavigateSendEvents(WindowComponentNavigation source, NavigationSide side);

		bool IsNavigationControlledSide(NavigationSide side);
		void OnNavigate(NavigationSide side);
		void OnNavigate(WindowComponentNavigation source, NavigationSide side);
		void OnNavigateLeft();
		void OnNavigateRight();
		void OnNavigateUp();
		void OnNavigateDown();

		void NavigationSetList(IListComponent listContainer);
		void NavigationUnsetList();

	}

	public enum NavigationSide : byte {

		None = 0,
		Left,
		Right,
		Up,
		Down,

	};

	public enum NavigationType : byte {

		None = 0,
		Auto,

	};

	[System.Serializable]
	public struct NavigationSideInfo {

		public bool stop;
		public bool directionAxisOnly;
		public bool @explicit;
		[Hidden("explicit", state: false)]
		public WindowComponentNavigation next;

	};

	[System.Serializable]
	public class NavigationGroup {

		[SerializeField]
		private bool enabled = false;
		[SerializeField][Hidden("enabled", state: false)]
		private List<WindowComponentNavigation> navigationComponents = new List<WindowComponentNavigation>();

		public bool IsActive() {

			return this.enabled;

		}

		public void SetActive(bool state) {

			this.enabled = state;

		}

		public void RegisterNavigationComponent(IComponent component) {

			if (component is WindowComponentNavigation) this.navigationComponents.Add(component as WindowComponentNavigation);

		}

		public void UnregisterNavigationComponent(IComponent component) {

			if (component is WindowComponentNavigation) this.navigationComponents.Remove(component as WindowComponentNavigation);

		}

		public List<WindowComponentNavigation> GetNavigationComponents() {

			return this.navigationComponents;

		}

		public void Clear() {

			this.navigationComponents.Clear();

		}

		#if UNITY_EDITOR
		public void OnValidate() {

			if (Application.isPlaying == true) return;
			#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isUpdating == true) return;
			#endif
			
			this.navigationComponents = this.navigationComponents.Where(x => x != null).ToList();

		}
		#endif

	};

	public abstract class WindowComponentNavigation : ColoredComponent, IWindowNavigation {

		[Header("Navigation")]
		[BeginGroup()]
		public NavigationType navigationType = NavigationType.None;
		public bool navigateOnDisabled = false;

		public bool navigationCustomSelector = false;
		[Hidden("navigationCustomSelector", state: false)]
		public WindowComponent navigationCustomSelectorObject;

		public NavigationSideInfo navigateLeftInfo;
		public NavigationSideInfo navigateRightInfo;
		public NavigationSideInfo navigateUpInfo;
		[EndGroup()]
		public NavigationSideInfo navigateDownInfo;

		public IListComponent listContainer;

		private ComponentEvent onNavigationEnter = new ComponentEvent();
		private ComponentEvent onNavigationLeave = new ComponentEvent();

		public override void OnShowBegin() {

			base.OnShowBegin();

			var navGroup = this.GetNavigationGroup();
			if (navGroup != null) navGroup.RegisterNavigationComponent(this);

		}

		public override void OnHideBegin() {

			base.OnHideBegin();

			var navGroup = this.GetNavigationGroup();
			if (navGroup != null) navGroup.UnregisterNavigationComponent(this);

		}

		public override void OnDeinit(System.Action callback) {

			base.OnDeinit(callback);

			this.onNavigationEnter.RemoveAllListeners();
			this.onNavigationLeave.RemoveAllListeners();

		}

		public void NavigationAddCallbackEnter(System.Action callback) {

			this.onNavigationEnter.AddListener(callback);

		}

		public void NavigationAddCallbackExit(System.Action callback) {

			this.onNavigationLeave.AddListener(callback);

		}

		public void NavigationRemoveCallbackEnter(System.Action callback) {

			this.onNavigationEnter.RemoveListener(callback);

		}

		public void NavigationRemoveCallbackLeave(System.Action callback) {

			this.onNavigationLeave.RemoveListener(callback);

		}

		public void NavigationSetList(IListComponent listContainer) {

			this.listContainer = listContainer;

		}

		public void NavigationUnsetList() {

			this.listContainer = null;

		}

		public NavigationGroup GetNavigationGroup() {

			NavigationGroup navGroup = null;
			var root = this.GetRootComponent();
			if (root != null) {
				
				navGroup = root.navigationGroup;
				while (root != null && navGroup.IsActive() == false) {

					root = root.GetRootComponent();
					if (root != null) navGroup = root.navigationGroup;

				}

			}

			if (root == null || navGroup.IsActive() == false) {

				var window = this.GetWindow();
				if (window != null) {

					return window.navigationGroup;

				} else {

					return null;

				}

			} else {

				return navGroup;

			}

		}

		public bool HasNavigationCustomSelector() {

			return this.navigationCustomSelector;

		}

		public WindowComponent GetNavigationCustomSelectorObject() {

			return this.navigationCustomSelectorObject;

		}

		public bool IsNavigateOnDisabled() {

			return this.navigateOnDisabled;

		}

		public NavigationType GetNavigationType() {

			return this.navigationType;

		}

		public void NavigationConnectToLinker(LinkerComponent linker) {

			var nav = linker.Get<IWindowNavigation>();
			this.NavigateCopyTo(nav);

		}

		public override void RegisterSubComponent(WindowObjectElement subComponent) {

			base.RegisterSubComponent(subComponent);

			var nav = subComponent as IWindowNavigation;
			if (nav != null) {
				
				this.NavigateCopyTo(nav);

				if (this is IListComponent) {
					
					nav.NavigationSetList(this as IListComponent);

				}

			}

		}

		public override void UnregisterSubComponent(WindowObjectElement subComponent, System.Action callback = null, bool immediately = true) {

			base.UnregisterSubComponent(subComponent, callback, immediately);

			var nav = subComponent as IWindowNavigation;
			if (nav != null) {
				
				if (this is IListComponent) {

					nav.NavigationUnsetList();

				}

			}

		}

		private void NavigateCopyTo(IWindowNavigation target) {
			
			if (target != null) {

				target.SetNavigationInfo(NavigationSide.Left, this.navigateLeftInfo);
				target.SetNavigationInfo(NavigationSide.Right, this.navigateRightInfo);
				target.SetNavigationInfo(NavigationSide.Up, this.navigateUpInfo);
				target.SetNavigationInfo(NavigationSide.Down, this.navigateDownInfo);

			}

		}

		public void SetNavigationInfo(NavigationSide side, NavigationSideInfo info) {

			switch (side) {

				case NavigationSide.Down:
					this.navigateDownInfo = info;
					break;

				case NavigationSide.Up:
					this.navigateUpInfo = info;
					break;

				case NavigationSide.Left:
					this.navigateLeftInfo = info;
					break;

				case NavigationSide.Right:
					this.navigateRightInfo = info;
					break;

			}

		}
		/*
		public T NavigationFindLeft<T>(bool interactableOnly = false, bool lookupRootOnFail = true) where T : IComponent {

			var element = this.NavigationFind(NavigationSide.Left, interactableOnly, lookupRootOnFail);
			if (element is LinkerComponent) {

				return (element as LinkerComponent).Get<T>();

			}

			return (T)(element as IComponent);

		}

		public T NavigationFindRight<T>(bool interactableOnly = false, bool lookupRootOnFail = true) where T : IComponent {

			var element = this.NavigationFind(NavigationSide.Right, interactableOnly, lookupRootOnFail);
			if (element is LinkerComponent) {

				return (element as LinkerComponent).Get<T>();

			}

			return (T)(element as IComponent);

		}

		public T NavigationFindUp<T>(bool interactableOnly = false, bool lookupRootOnFail = true) where T : IComponent {

			var element = this.NavigationFind(NavigationSide.Up, interactableOnly, lookupRootOnFail);
			if (element is LinkerComponent) {

				return (element as LinkerComponent).Get<T>();

			}

			return (T)(element as IComponent);

		}

		public T NavigationFindDown<T>(bool interactableOnly = false, bool lookupRootOnFail = true) where T : IComponent {

			var element = this.NavigationFind(NavigationSide.Down, interactableOnly, lookupRootOnFail);
			if (element is LinkerComponent) {

				return (element as LinkerComponent).Get<T>();

			}

			return (T)(element as IComponent);

		}*/

		public WindowComponentNavigation NavigationFindLeft() {

			if (this.navigationType == NavigationType.None) {

				return null;

			}

			return this.GetNavigation(NavigationSide.Left);

		}

		public WindowComponentNavigation NavigationFindRight() {

			if (this.navigationType == NavigationType.None) {

				return null;

			}

			return this.GetNavigation(NavigationSide.Right);

		}

		public WindowComponentNavigation NavigationFindUp() {

			if (this.navigationType == NavigationType.None) {

				return null;

			}

			return this.GetNavigation(NavigationSide.Up);

		}

		public WindowComponentNavigation NavigationFindDown() {

			if (this.navigationType == NavigationType.None) {

				return null;

			}

			return this.GetNavigation(NavigationSide.Down);

		}

		public WindowComponentNavigation GetNavigation(System.Func<WindowComponentNavigation, WindowComponentNavigation> getter) {

			var result = getter.Invoke(this);
			while (result != null) {

				if (result.IsInteractableAndActive() == false) {

					result = getter.Invoke(result);

				} else {

					break;

				}

			}

			return result;

		}

		public WindowComponentNavigation GetNavigation(NavigationSide side) {

			var dir = Vector3.zero;
			var rotation = this.GetRectTransform().rotation;
			var sideInfo = new NavigationSideInfo();
			switch (side) {

				case NavigationSide.Left:
					dir = rotation * Vector3.left;
					sideInfo = this.navigateLeftInfo;
					break;

				case NavigationSide.Right:
					dir = rotation * Vector3.right;
					sideInfo = this.navigateRightInfo;
					break;

				case NavigationSide.Up:
					dir = rotation * Vector3.up;
					sideInfo = this.navigateUpInfo;
					break;

				case NavigationSide.Down:
					dir = rotation * Vector3.down;
					sideInfo = this.navigateDownInfo;
					break;

			}

			if (sideInfo.@explicit == true) {
				
				return sideInfo.next;

			}

			return this.GetNavigation(dir, sideInfo);

		}

		public WindowComponentNavigation GetNavigation(Vector3 dir, NavigationSideInfo sideInfo) {

			var navTarget = this.FindSelectable(dir, sideInfo, pointOnEdge: true) as WindowComponentNavigation;
			if (navTarget == null) navTarget = this.FindSelectable(dir, sideInfo, pointOnEdge: false) as WindowComponentNavigation;
			return navTarget;

		}

		public virtual bool IsNavigationControlledSide(NavigationSide side) {

			return false;

		}

		public virtual bool IsNavigationPreventChildEvents(NavigationSide side) {

			return false;

		}

		public virtual bool IsNavigationPreventEvents(NavigationSide side) {

			return false;

		}

		public bool NavigateSendEvents(WindowComponentNavigation source, NavigationSide side) {
			
			if (this.listContainer != null) {

				this.listContainer.NavigateSendEvents(this, side);

				if (this.listContainer.IsNavigationPreventChildEvents(side) == true) {

					return false;

				}

			} else {

				if (this.rootComponent != null && this.rootComponent is IWindowNavigation) {

					var nav = (this.rootComponent as IWindowNavigation);
					nav.NavigateSendEvents(this, side);

					if (nav.IsNavigationPreventChildEvents(side) == true) {

						return false;

					}

				}

			}

			if (this.IsNavigationPreventEvents(side) == false) {

				if (this.IsNavigationControlledSide(side) == true) {

					this.OnNavigate(side);
					this.OnNavigate(source, side);

					switch (side) {

						case NavigationSide.Down:
							this.OnNavigateDown();
							break;

						case NavigationSide.Up:
							this.OnNavigateUp();
							break;

						case NavigationSide.Left:
							this.OnNavigateLeft();
							break;

						case NavigationSide.Right:
							this.OnNavigateRight();
							break;

					}

				}

				return true;

			}

			return false;

		}

		public virtual void OnNavigate(NavigationSide side) {}
		public virtual void OnNavigate(WindowComponentNavigation source, NavigationSide side) {}
		public virtual void OnNavigateLeft() {}
		public virtual void OnNavigateRight() {}
		public virtual void OnNavigateUp() {}
		public virtual void OnNavigateDown() {}

		public virtual void OnNavigationEnter() {

			this.onNavigationEnter.Invoke();

		}

		public virtual void OnNavigationLeave() {

			this.onNavigationLeave.Invoke();

		}

		#region Utils
		public WindowComponentNavigation FindSelectable(Vector3 dir, NavigationSideInfo sideInfo, bool pointOnEdge = true) {

			if (sideInfo.stop == true) return null;

			var navGroup = this.GetNavigationGroup();
			if (navGroup == null) return null;

			dir = dir.normalized;
			var tr = this.transform as RectTransform;
			var localDir = Quaternion.Inverse(tr.rotation) * dir;
			var pos = tr.TransformPoint(pointOnEdge == true ? WindowComponentNavigation.GetPointOnRectEdge(tr, localDir) : (Vector3)tr.rect.center);
			var maxScore = Mathf.NegativeInfinity;

			var components = navGroup.GetNavigationComponents();

			WindowComponentNavigation bestPick = null;
			for (int i = 0; i < components.Count; ++i) {
				
				var sel = components[i] as WindowComponentNavigation;
				if (sel == this || sel == null) continue;

				var selInteractable = sel as IInteractableStateComponent;
				if (selInteractable != null) {

					if (selInteractable.GetNavigationType() == NavigationType.None ||
						(
							selInteractable.IsNavigateOnDisabled() == false &&
							selInteractable.IsVisible() == false
						) ||
					    (
							selInteractable.IsNavigateOnDisabled() == false &&
							selInteractable.IsInteractable() == false
					    ))
						continue;

				} else {
					
					if (sel.GetNavigationType() == NavigationType.None ||
						sel.IsVisible() == false)
						continue;
					
				}

				var selRect = sel.GetRectTransform();
				var selCenter = selRect != null ? selRect.rect.center.XY() : Vector3.zero;
				var myVector = selRect.TransformPoint(selCenter) - pos;

				if (sideInfo.directionAxisOnly == true) {

					if (dir.x == 0f) myVector.x = 0f;
					if (dir.y == 0f) myVector.y = 0f;

				}

				var dot = Vector3.Dot(dir, myVector);
				if (dot <= 0f) continue;

				var score = dot / myVector.sqrMagnitude;
				if (score > maxScore) {
					
					maxScore = score;
					bestPick = sel;

				}

			}

			return bestPick;

		}

		private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir) {
			
			if (rect == null) return Vector3.zero;
			if (dir != Vector2.zero) dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
			dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);

			return dir;

		}
		#endregion

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			this.ValidateItemsEditor();

		}

		private void ValidateItemsEditor() {

			if (Application.isPlaying == false) {
				
				if (this is WindowComponentNavigation) {

					var navGroup = this.GetNavigationGroup();
					if (navGroup == null) return;

					var navComponents = navGroup.GetNavigationComponents();
					if (navComponents.Contains(this) == false) {

						if (this.GetNavigationType() == NavigationType.Auto) {

							navGroup.RegisterNavigationComponent(this);

						}

					} else {

						if (this.GetNavigationType() == NavigationType.None) {

							navGroup.UnregisterNavigationComponent(this);

						}

					}

				}

			}

		}

		public override void OnDrawGizmos() {

			base.OnDrawGizmos();

			this.OnDrawGUI_EDITOR(false, false);

		}

		public override void OnDrawGizmosSelected() {

			base.OnDrawGizmosSelected();

			var selected = (UnityEditor.Selection.activeGameObject == this.gameObject);
			this.OnDrawGUI_EDITOR(selected, true);

		}

		private void OnDrawGUI_EDITOR(bool selected, bool selectedHierarchy) {

			this.ValidateItemsEditor();

			/*ME.EditorUtilities.BeginDraw();

			var scale = 1f;
			var canvas = this.GetComponentsInParent<Canvas>().FirstOrDefault((c) => c.isRootCanvas);
			if (canvas != null) scale = canvas.transform.localScale.x;

			var arrowsSize = 80f * scale;
			if (this.navigation.left != null) ME.EditorUtilities.DrawArrow(Color.white, this.transform.position, this.navigation.left.transform.position, arrowsSize);
			if (this.navigation.right != null) ME.EditorUtilities.DrawArrow(Color.white, this.transform.position, this.navigation.right.transform.position, arrowsSize);
			if (this.navigation.up != null) ME.EditorUtilities.DrawArrow(Color.white, this.transform.position, this.navigation.up.transform.position, arrowsSize);
			if (this.navigation.down != null) ME.EditorUtilities.DrawArrow(Color.white, this.transform.position, this.navigation.down.transform.position, arrowsSize);

			ME.EditorUtilities.EndDraw();*/

			WindowComponentNavigation.DrawNavigationForSelectable(this);

		}

		private static void DrawNavigationForSelectable(IWindowNavigation sel) {
			
			if (sel == null) return;

			var transform = sel.GetRectTransform();
			bool active = UnityEditor.Selection.transforms.Any(e => e == transform);

			UnityEditor.Handles.color = new Color(1.0f, 0.9f, 0.1f, active ? 1.0f : 0.4f);

			WindowComponentNavigation.DrawNavigationArrow(-Vector2.right, sel, sel.NavigationFindLeft());
			WindowComponentNavigation.DrawNavigationArrow(Vector2.right, sel, sel.NavigationFindRight());
			WindowComponentNavigation.DrawNavigationArrow(Vector2.up, sel, sel.NavigationFindUp());
			WindowComponentNavigation.DrawNavigationArrow(-Vector2.up, sel, sel.NavigationFindDown());

		}

		const float kArrowThickness = 2.5f;
		const float kArrowHeadSize = 1.2f;

		private static void DrawNavigationArrow(Vector2 direction, IWindowNavigation fromObj, IWindowNavigation toObj) {
			
			if (fromObj == null || toObj == null) return;

			var fromTransform = fromObj.GetRectTransform();
			var toTransform = toObj.GetRectTransform();

			Vector2 sideDir = new Vector2(direction.y, -direction.x);
			Vector3 fromPoint = fromTransform.TransformPoint(WindowComponentNavigation.GetPointOnRectEdge(fromTransform, direction));
			Vector3 toPoint = toTransform.TransformPoint(WindowComponentNavigation.GetPointOnRectEdge(toTransform, -direction));
			float fromSize = UnityEditor.HandleUtility.GetHandleSize(fromPoint) * 0.05f;
			float toSize = UnityEditor.HandleUtility.GetHandleSize(toPoint) * 0.05f;
			fromPoint += fromTransform.TransformDirection(sideDir) * fromSize;
			toPoint += toTransform.TransformDirection(sideDir) * toSize;
			float length = Vector3.Distance(fromPoint, toPoint);
			Vector3 fromTangent = fromTransform.rotation * direction * length * 0.3f;
			Vector3 toTangent = toTransform.rotation * -direction * length * 0.3f;

			UnityEditor.Handles.DrawBezier(fromPoint, toPoint, fromPoint + fromTangent, toPoint + toTangent, UnityEditor.Handles.color, null, kArrowThickness);
			UnityEditor.Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + toTransform.rotation * (-direction - sideDir) * toSize * kArrowHeadSize);
			UnityEditor.Handles.DrawAAPolyLine(kArrowThickness, toPoint, toPoint + toTransform.rotation * (-direction + sideDir) * toSize * kArrowHeadSize);

		}
		#endif

	}

}