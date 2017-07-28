using UnityEngine;
using System.Collections;
using UnityEngine.UI.Extensions;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.Extensions;

namespace UnityEngine.UI.Windows.Components {

	public class RouletteComponent : ListComponent, IDragHandler, IBeginDragHandler, IEndDragHandler {

		public Transform container;
		public CircleLayoutGroup circeLayoutGroup;
		public UIPolygon uiPolygon;

		public bool interactable = true;
		public bool draggable = true;

		public float dragSpeed = 1f;
		public float velocitySlowdownSpeed = 1f;
		public float angleSpeed = 1f;
		public float clampMinValue = 0.1f;

		private bool isDragging;

		public float velocity;
		public float angle;

		private System.Action<WindowComponent> onElementSelected;

		#region Drag Handlers
		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {

			if (this.interactable == false) return;
			if (this.draggable == false) return;

			this.isDragging = true;

		}

		void IDragHandler.OnDrag(PointerEventData eventData) {

			if (this.interactable == false) return;

			if (this.isDragging == true) {

				var windowDelta = WindowSystem.ConvertPointScreenToWindow(eventData.delta, this);
				this.Move(-windowDelta * this.dragSpeed, immediately: true);

			}

		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData) {

			if (this.interactable == false) return;

			this.isDragging = false;

		}
		#endregion

		public void Move(Vector2 delta, bool immediately = false) {

			this.velocity += delta.x * delta.y;

		}

		public void SetCallback(System.Action<WindowComponent> onElementSelected) {

			this.onElementSelected = onElementSelected;

		}

		public void SetMaxElementAngle(float value) {

			if (this.circeLayoutGroup != null) {

				this.circeLayoutGroup.maxAnglePerElement = value;

			}

		}

		public void SetElementsAngle(float value) {

			if (this.circeLayoutGroup != null) {

				this.circeLayoutGroup.angle = value;

			}

		}

		[ContextMenu("Arrange")]
		public void Arrange() {

			if (this.container == null || this.uiPolygon == null || this.circeLayoutGroup == null) return;

			//this.circeLayoutGroup.Arrange();

			this.velocity = Mathf.Lerp(this.velocity, 0f, Time.unscaledDeltaTime * this.velocitySlowdownSpeed);
			if (Mathf.Abs(this.velocity) < this.clampMinValue) this.velocity = 0f;

			this.angle += this.velocity * this.angleSpeed;
			this.angle = this.angle % 360f;
			this.container.rotation = Quaternion.AngleAxis(this.angle, Vector3.forward);

			if (this.velocity == 0f) {

				var element = this.circeLayoutGroup.GetElementByAngle(this.angle, Vector3.down);
				if (this.onElementSelected != null) this.onElementSelected.Invoke(element.GetComponent<WindowComponent>());

			}

		}

		#if UNITY_EDITOR
		public override void OnValidateEditor() {

			base.OnValidateEditor();

			this.velocity = Mathf.Max(0f, this.velocity);

			if (this.circeLayoutGroup == null) {

				this.circeLayoutGroup = this.gameObject.AddComponent<CircleLayoutGroup>();

			}

		}
		#endif

	}

}