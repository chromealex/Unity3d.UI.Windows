using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MECanvasScaler : CanvasScaler {

	public bool drawAsWorld = false;
	public bool manualUpdate = false;

	protected override void Update() {
		
		if (this.manualUpdate == false) this.UpdateWithMode();
		
	}

	private float prevScaleFactor;
	protected void SetScaleFactor_INTERNAL(float scaleFactor) {
		
		base.SetScaleFactor(scaleFactor);

		if (this.drawAsWorld == true) {

			//if (this.prevScaleFactor != scaleFactor) {

				//this.prevScaleFactor = scaleFactor;

				this.transform.localScale = Vector3.one * scaleFactor / 100f;

			//}

		}

	}
	
	public void UpdateWithMode() {
		
		this.UpdateWithMode(this.uiScaleMode, Vector2.zero);
		
	}
	
	public void UpdateWithMode(Vector2 screenSize) {
		
		this.UpdateWithMode(this.uiScaleMode, screenSize);
		
	}

	public void UpdateWithMode(CanvasScaler.ScaleMode uiScaleMode, Vector2 screenSize) {

		if (this.drawAsWorld == false) {

			this.Handle();

		} else {

			switch (uiScaleMode) {

			case CanvasScaler.ScaleMode.ScaleWithScreenSize: this.WorldConstantPixelSize(); break; // fixed mode
			case CanvasScaler.ScaleMode.ConstantPixelSize: this.WorldScaleWithScreenSize(screenSize); break;	// dynamic
			case CanvasScaler.ScaleMode.ConstantPhysicalSize: this.WorldConstantPhysicalSize(); break;

			}

		}

	}

	protected void WorldConstantPhysicalSize() {

		float dpi = Screen.dpi;
		float num = (dpi != 0f) ? dpi : this.m_FallbackScreenDPI;
		float num2 = 1f;
		switch (this.m_PhysicalUnit)
		{
		case CanvasScaler.Unit.Centimeters:
			num2 = 2.54f;
			break;
		case CanvasScaler.Unit.Millimeters:
			num2 = 25.4f;
			break;
		case CanvasScaler.Unit.Inches:
			num2 = 1f;
			break;
		case CanvasScaler.Unit.Points:
			num2 = 72f;
			break;
		case CanvasScaler.Unit.Picas:
			num2 = 6f;
			break;
		}
		this.SetScaleFactor_INTERNAL(num / num2);
		this.SetReferencePixelsPerUnit (this.m_ReferencePixelsPerUnit * num2 / this.m_DefaultSpriteDPI);

	}
	
	protected void WorldConstantPixelSize() {

		this.SetScaleFactor_INTERNAL (this.m_ScaleFactor);
		this.SetReferencePixelsPerUnit (this.m_ReferencePixelsPerUnit);

	}

	protected void WorldScaleWithScreenSize(Vector2 screenSize) {

		Vector2 vector = (screenSize == Vector2.zero) ? new Vector2 ((float)Screen.width, (float)Screen.height) : screenSize;

		float scaleFactor = 0f;
		switch (this.m_ScreenMatchMode)
		{
		case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
			{
				float num = Mathf.Log (vector.x / this.m_ReferenceResolution.x, 2f);
				float num2 = Mathf.Log (vector.y / this.m_ReferenceResolution.y, 2f);
				float num3 = Mathf.Lerp (num, num2, this.m_MatchWidthOrHeight);
				scaleFactor = Mathf.Pow (2f, num3);
				break;
			}
		case CanvasScaler.ScreenMatchMode.Expand:
			scaleFactor = Mathf.Min (vector.x / this.m_ReferenceResolution.x, vector.y / this.m_ReferenceResolution.y);
			break;
		case CanvasScaler.ScreenMatchMode.Shrink:
			scaleFactor = Mathf.Max (vector.x / this.m_ReferenceResolution.x, vector.y / this.m_ReferenceResolution.y);
			break;
		}

		this.SetScaleFactor_INTERNAL (1f / scaleFactor);
		this.SetReferencePixelsPerUnit (this.m_ReferencePixelsPerUnit);

	}

}
