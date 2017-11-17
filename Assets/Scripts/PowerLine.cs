using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerLine : MonoBehaviour {
	public SpriteRenderer sprite;

	private readonly Color on = new Color(1f, 1f, 0f);
	private readonly Color off = new Color(169f / 255f, 92f / 255f, 0f);

	void Start() {
		sprite = GetComponent<SpriteRenderer>();

		Board.OnButtonPress += new Board.ButtonCallback(PowerOn);
		Board.OnButtonDepress += new Board.ButtonDepressCallback(PowerOff);
		PowerOff();
	}

	public void PowerOn() {
		sprite.material.color = on;
	}

	public void PowerOff() {
		sprite.material.color = off;
	}
}
