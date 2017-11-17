using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {
	public bool isAlreadyActivated = false;

	private const int fps = 7;
	private static SpriteRenderer renderer;
	private static Texture2D[] frames = null;
	
	public Collider activator = null;

	void Start() {
		Board.totalGoals++;
		Debug.Log("New goal");
		/*
		if (frames == null) {
			frames = Resources.LoadAll<Texture2D>("GoalFrames/");
			renderer = GetComponent<SpriteRenderer>();
		}*/
	}
	/*
	void FixedUpdate() {
		if (renderer == null) return;
		int index = Mathf.FloorToInt((Time.time * fps) % frames.Length);
		renderer.sprite = Sprite.Create(frames[index], new Rect(0, 0, frames[index].width, frames[index].height), new Vector2(.5f, .5f));
	}*/

	public void OnTriggerEnter(Collider other) {
		if (!isAlreadyActivated && other.tag == "MainObject") {
			isAlreadyActivated = true;
			activator = other;
			Board.GoalReached();
		}
	}

	public void OnTriggerExit(Collider other) {
		if (other.tag == "MainObject" && activator == other) {
			isAlreadyActivated = false;
			Board.guiManager.blocksText.text = "Squares in: " + --Board.goalsActivated + "/" + Board.totalGoals;
		}
	}
}
