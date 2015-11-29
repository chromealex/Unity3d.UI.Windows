using UnityEngine;
using System.Collections;

public class ExampleRotator : MonoBehaviour {

	public float speed = 2f;
	private float angle = 0f;

	public void LateUpdate() {

		this.angle += this.speed * Time.deltaTime;
		this.transform.localRotation = Quaternion.AngleAxis(this.angle, Vector3.up);

	}

}
