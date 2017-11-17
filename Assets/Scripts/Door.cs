using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
	public SpriteRenderer open;
	public SpriteRenderer closed;
	public Collider doorCollider;

	public DoorColor color;

	public enum DoorColor {
		red,
		blue
	}

	// Use this for initialization
	void Start () {
		Board.OnSwitchDoors += new Board.DoorCallback(SwitchDoors);
	}
	
	public void Init(DoorColor color) {
		foreach (Transform c in transform) {
			if (c.name == "door_open_" + color.ToString()) open = c.GetComponent<SpriteRenderer>();
			if (c.name == "door_closed_" + color.ToString()) {
				closed = c.GetComponent<SpriteRenderer>();
				doorCollider = c.GetComponent<Collider>();
			} else if (c.name != "door_open_" + color.ToString() && c.name != "door_closed_" + color.ToString()) {
				Destroy(c.gameObject);
			}
		}

		this.color = color;
		SwitchDoors(Board.currentDoorOpen);
	}

	public void SwitchDoors(DoorColor activate) {
		if (activate == color) {
			open.enabled = true;
			closed.enabled = false;
			doorCollider.enabled = false;
		} else {
			closed.enabled = true;
			open.enabled = false;
			doorCollider.enabled = true;
		}
	}
}
