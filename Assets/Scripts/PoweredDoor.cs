using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoweredDoor : MonoBehaviour {
	public SpriteRenderer open;
	public SpriteRenderer closed;
	public Collider doorCollider;
	public static Coroutine doorSound = null;

	// Use this for initialization
	void Start () {
		Board.OnButtonPress += new Board.ButtonCallback(OnButtonPressed);
		Board.OnButtonDepress += new Board.ButtonDepressCallback(OnButtonDepressed);

		foreach (Transform c in transform) {
			if (c.name == "door_open") open = c.GetComponent<SpriteRenderer>();
			if (c.name == "door_closed") {
				closed = c.GetComponent<SpriteRenderer>();
				doorCollider = c.GetComponent<Collider>();
			}
		}

		OnButtonDepressed();
	}

	public static IEnumerator PlayDoorSound() {
		Sounds.PlaySound(ref Sounds.doorSound);
		yield return new WaitForSeconds(.2f);
		doorSound = null;
	}

	public void OnButtonPressed() {
		open.enabled = true;
		closed.enabled = false;
		doorCollider.enabled = false;
		
		if (doorSound == null) doorSound = StartCoroutine(PlayDoorSound());
	}

	public void OnButtonDepressed() {
		closed.enabled = true;
		open.enabled = false;
		doorCollider.enabled = true;

		if (doorSound == null) doorSound = StartCoroutine(PlayDoorSound());
	}
}
