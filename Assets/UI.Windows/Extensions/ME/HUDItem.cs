using UnityEngine;
using System.Collections;

public class HUDItem : MonoBehaviour {
	
	private Camera uiCamera;
	private Camera gameCamera;
	private Transform alignTo;

	private bool pointAligned = false;
	private Vector3 alignToPoint;

	public Vector3 offset;

	private bool isVisible;
	
	public void InitHUD(Transform alignTo, Camera uiCamera, Camera gameCamera, Vector3 offset = default(Vector3)) {
		
		this.uiCamera = uiCamera;
		this.gameCamera = gameCamera;

		if (offset != default(Vector3)) this.offset = offset;
		
		this.alignTo = alignTo;
		this.pointAligned = false;
		
		this.transform.localRotation = Quaternion.identity;
		this.transform.localScale = Vector3.one;
		
		this.Reposition();
		
	}
	
	public void InitHUD(Vector3 alignToPoint, Camera uiCamera, Camera gameCamera, Vector3 offset = default(Vector3)) {
		
		this.uiCamera = uiCamera;
		this.gameCamera = gameCamera;
		
		if (offset != default(Vector3)) this.offset = offset;

		this.alignToPoint = alignToPoint;
		this.pointAligned = true;
		
		this.transform.localRotation = Quaternion.identity;
		this.transform.localScale = Vector3.one;
		
		this.Reposition();
		
	}

    public void SetAlignToPoint(Vector3 point) {

        this.alignToPoint = point;
        this.pointAligned = true;

        this.Reposition();

    }

	public void Reset() {

		this.alignTo = null;
		this.uiCamera = null;
		this.gameCamera = null;
		this.pointAligned = false;

	}

	public bool IsVisible() {

		return this.isVisible;

	}

	public void SetVisible(bool state) {

		this.isVisible = state;

	}

	public void LateUpdate() {

	    if (this.pointAligned == true) return;

		if ((this.alignTo != null || this.pointAligned == true) && this.uiCamera != null && this.gameCamera != null) {

			this.Reposition();

		}

	}

	public void Reposition() {
		
		if (this.alignTo != null || this.pointAligned == true)  {
			
			var position = this.pointAligned == true ? (this.alignToPoint + this.offset) : (this.alignTo.transform.position + this.offset);
			var pos = this.gameCamera.WorldToViewportPoint(position);
			
			var isVisible = (pos.z > 0f && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
			
			if (this.isVisible != isVisible) {

				this.SetVisible(isVisible);

			}

			var rPos = this.uiCamera.ViewportToWorldPoint(pos);

			if (rPos != this.transform.position) {

				this.transform.position = rPos;
				pos = this.transform.localPosition;
				pos.z = 0f;
				
				this.transform.localPosition = pos;

			}

		}
		
	}
}
