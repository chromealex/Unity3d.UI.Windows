using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Components.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Windows {

	public enum PointerState : byte {

		Default,
		Down,

	};

	public class WindowSystemInput : MonoBehaviour {

		public const string SCROLL_AXIS = "Mouse ScrollWheel";

		public static ComponentEvent onPointerUp = new ComponentEvent();
		public static ComponentEvent onPointerDown = new ComponentEvent();

		private static PointerEventData scrollEvent;

		public void Start() {

			WindowSystemInput.scrollEvent = new PointerEventData(EventSystem.current);

		}

		public void LateUpdate() {
			
			#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
			if (Input.GetMouseButtonDown(0) == true) WindowSystemInput.onPointerDown.Invoke();
			if (Input.GetMouseButtonUp(0) == true) WindowSystemInput.onPointerUp.Invoke();
			if (Input.GetMouseButtonDown(1) == true) WindowSystemInput.onPointerDown.Invoke();
			if (Input.GetMouseButtonUp(1) == true) WindowSystemInput.onPointerUp.Invoke();
			if (Input.GetMouseButtonDown(2) == true) WindowSystemInput.onPointerDown.Invoke();
			if (Input.GetMouseButtonUp(2) == true) WindowSystemInput.onPointerUp.Invoke();
			#endif

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