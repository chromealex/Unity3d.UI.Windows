using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Types;

public class Example3DHover : MonoBehaviour {
	
	public string hoverText;
	public Vector3 offset = new Vector3(0f, 0.5f, 0f);

	private Camera cam;
	private TipWindowType tip;

	public void LateUpdate() {

		if (WindowSystem.IsUIHovered() == true) return;

		if (this.cam == null) this.cam = Camera.main;
		if (this.cam == null) return;

		var ray = this.cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100f, 1 << this.gameObject.layer) == true && hit.collider.GetComponent<Example3DHover>() == this) {

			if (this.tip == null) {

				this.tip = WindowSystem.Show<UIWindowExampleTip>(this.hoverText) as TipWindowType;
				this.tip.OnHover(this.transform, hit.point, this.cam, this.offset);

			}

		} else if (this.tip != null) {

			this.tip.Hide();
			this.tip.OnLeave();
			this.tip = null;

		}

	}

}
