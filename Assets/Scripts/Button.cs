using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {
	public ButtonDirection direction;

	public GameObject unpressed;
	public GameObject pressed;

	public enum ButtonDirection {
		up, 
		right,
		down, 
		left
	}

	public List<Collider> objectsInTrigger = new List<Collider>();

	void Start() {
		Board.ButtonCheck += new Board.ButtonCheckCallback(CheckPressed);
	}

	public void Init(ButtonDirection d) {
		direction = d;
	}

	public void Press() {
		unpressed.GetComponent<Renderer>().enabled = false;
		pressed.GetComponent<Renderer>().enabled = true;
		Stats.buttons++;
		Board.PressButtons();
	}

	public void Depress() {
		unpressed.GetComponent<Renderer>().enabled = true;
		pressed.GetComponent<Renderer>().enabled = false;
		Board.DepressButtons();
	}

	public void CheckPressed() {
		if (objectsInTrigger.Count > 0) {
			switch (Board.currentRotation) {
				case 0:
					if (direction == ButtonDirection.down) Press();
					else Depress();
					break;
				case 90:
					if (direction == ButtonDirection.right) Press();
					else Depress();
					break;
				case 180:
					if (direction == ButtonDirection.up) Press();
					else Depress();
					break;
				case 270:
					if (direction == ButtonDirection.left) Press();
					else Depress();
					break;
				case 360:
					if (direction == ButtonDirection.down) Press();
					else Depress();
					break;
			}
		}
	}

	public void OnTriggerEnter(Collider other) {
		if (other.CompareTag("Object") || other.CompareTag("MainObject")) {
			objectsInTrigger.Add(other);
			CheckPressed();
		}
	}

	public void OnTriggerExit(Collider other) {
		if(other.CompareTag("Object") || other.CompareTag("MainObject")) {
			objectsInTrigger.Remove(other);
			Depress();
		}
	}
}
