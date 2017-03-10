
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.Windows.Components;
using UnityEngine.Events;

namespace UnityEngine.UI.Windows {

	public enum PointerState : byte {

		Default,
		Down,

	};

	public class WindowSystemInput : WindowSystemInputModule {
		
		public const string SCROLL_AXIS = "Mouse ScrollWheel";
		
		public float scrollSensitivityPC = 100f;
		public float scrollSensitivityMac = 10f;

		public class PointerEventKey : ME.Events.SimpleEvent<int> {}
		public class PointerEvent : ME.Events.SimpleEvent {}
		public static PointerEventKey onPointerUp = new PointerEventKey();
		public static PointerEventKey onPointerDown = new PointerEventKey();
		public static PointerEvent onAnyKeyDown = new PointerEvent();

		private static PointerEventData scrollEvent;

		private static WindowSystemInput instance;

		protected override void Awake() {

			base.Awake();

			WindowSystemInput.instance = this;

		}

	    protected override void OnDestroy() {
	        
            base.OnDestroy();

            WindowSystemInput.instance = null;
            WindowSystemInput.scrollEvent = null;

        }

		protected override void Start() {

			base.Start();

			WindowSystemInput.scrollEvent = new PointerEventData(EventSystem.current);

			#if UNITY_TVOS
			UnityEngine.Apple.TV.Remote.allowExitToHome = false;
			UnityEngine.Apple.TV.Remote.touchesEnabled = true;
			#endif

		}

		public override bool IsModuleSupported() {

			return false;

		}

		public override void Process() {
			
		}

		public override void UpdateModule() {
			
			#if UNITY_STANDALONE || UNITY_TVOS || UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_EDITOR
			if (Input.GetMouseButtonDown(0) == true) WindowSystemInput.onPointerDown.Invoke(0);
			if (Input.GetMouseButtonUp(0) == true) WindowSystemInput.onPointerUp.Invoke(0);
			if (Input.GetMouseButtonDown(1) == true) WindowSystemInput.onPointerDown.Invoke(1);
			if (Input.GetMouseButtonUp(1) == true) WindowSystemInput.onPointerUp.Invoke(1);
			if (Input.GetMouseButtonDown(2) == true) WindowSystemInput.onPointerDown.Invoke(2);
			if (Input.GetMouseButtonUp(2) == true) WindowSystemInput.onPointerUp.Invoke(2);
			#endif

			#if UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8
			if (Input.touchCount > 0) {

				for (int i = 0; i < Input.touchCount; ++i) {

					var touch = Input.GetTouch(i);
					if (touch.phase == TouchPhase.Began) {

						WindowSystemInput.onPointerDown.Invoke(touch.fingerId);

					}

					if (touch.phase == TouchPhase.Ended ||
						touch.phase == TouchPhase.Canceled) {

						WindowSystemInput.onPointerUp.Invoke(touch.fingerId);

					}

				}

			}
			#endif

			if (Input.anyKeyDown == true ||
				Input.touchCount > 0 ||
				Input.GetMouseButtonDown(0) == true ||
				Input.GetMouseButtonDown(1) == true ||
				Input.GetMouseButtonDown(2) == true) {

				WindowSystemInput.onAnyKeyDown.Invoke();

			}

			if (Input.GetKeyDown(KeyCode.Tab) == true) {

				var system = EventSystem.current;
				if (system == null || system.currentSelectedGameObject == null) return;

				var next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
				
				if (next != null) {
					
					var inputfield = next.GetComponent<InputField>();
					if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(system));
					
					system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));

				}
				
			}

		}

		private static void Select_INTERNAL(IInteractableComponent button) {

			var sel = button.GetSelectable();
			if (sel != null) {
				
				WindowSystemInput.Deselect(button);
				sel.Select();
				if (sel is ButtonExtended) {

					(sel as ButtonExtended).Select(forced: true);

				}

			}

		}

		public static void Select(IInteractableComponent button) {

			if (button != null) {

				var sel = button.GetSelectable();
				if (sel != null) {

					if (button.IsVisible() == false) {

						var buttonBase = (button as WindowComponentBase);

						System.Action callback = null;
						callback = () => {

							WindowSystem.GetEvents().Unregister(buttonBase, WindowEventType.OnShowEnd, callback);
							WindowSystemInput.Select_INTERNAL(button);

						};

						WindowSystem.GetEvents().Register(buttonBase, WindowEventType.OnShowEnd, callback);

					} else {

						WindowSystemInput.Select_INTERNAL(button);

					}

				}

			}

		}

		public static void Deselect(IInteractableComponent button) {

			if (button != null) {

				var sel = button.GetSelectable();
				if (sel != null) {

					if (EventSystem.current.currentSelectedGameObject == sel.gameObject) EventSystem.current.SetSelectedGameObject(null);

				}

			}

		}

		public static float GetScrollSensitivity() {

			if (WindowSystemInput.instance == null) return 0f;

			#if UNITY_STANDALONE_OSX
			return WindowSystemInput.instance.scrollSensitivityMac;
			#else
			return WindowSystemInput.instance.scrollSensitivityPC;
			#endif

		}

		public static PointerEventData GetPointerScroll() {

			var scrollEvent = WindowSystemInput.scrollEvent;

			#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
			scrollEvent.scrollDelta = new Vector2(0f, Input.GetAxisRaw(WindowSystemInput.SCROLL_AXIS));
			#endif

			return scrollEvent;

		}

		public static Vector2 GetPointerPosition() {

			// TODO: Make it crossplatform

			#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
			return Input.mousePosition;
			#else
			return Vector2.zero;
			#endif

		}

		public static PointerState GetPointerState() {

			var state = PointerState.Default;
			#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
			var button1 = Input.GetMouseButton(0);
			var button2 = Input.GetMouseButton(1);
			var button3 = Input.GetMouseButton(2);
			if (button1 == true || button2 == true || button3 == true) {

				state = PointerState.Down;

			}
			#endif

			return state;

		}

	}

}