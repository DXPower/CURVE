using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotaryDoor : MonoBehaviour {
	public int currentRotation = 0;
	public float defaultRotation = 0;
	public int rotateDur;
	public float rotateSpeed;

	void Start() {
		Board.OnButtonPress += new Board.ButtonCallback(OnButtonPressed);
		Board.OnButtonDepress += new Board.ButtonDepressCallback(OnButtonDepressed);

		defaultRotation = transform.eulerAngles.z;
		currentRotation = (int) defaultRotation;
	}


	private IEnumerator SmoothRotate(float angle) {
		float o = transform.rotation.eulerAngles.z;

		for (float i = 0; i <= rotateDur; i++) {
			transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x
				, transform.rotation.eulerAngles.y
				, EaseInOutExpo(i, o, angle, rotateDur)));
			i++;
			yield return new WaitForSeconds(rotateSpeed);
		}

		// Fixes floating point errors causing a slight tilt to the level
		transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, currentRotation));
	}

	// A nice smooth easing equation 
	private float EaseInOutExpo(float t, float b, float c, float d) {
		t /= d / 2;
		if (t < 1) return c / 2 * Mathf.Pow(2, 10 * (t - 1)) + b;
		t--;
		return c / 2 * (-Mathf.Pow(2, -10 * t) + 2) + b;
	}

	public void OnButtonPressed() {
		if (currentRotation == defaultRotation) {
			currentRotation += 90;
			StartCoroutine(SmoothRotate(90));
		}
	}

	public void OnButtonDepressed() {
		if (currentRotation != defaultRotation) {
			currentRotation -= 90;
			StartCoroutine(SmoothRotate(-90));
		}
	}
}
