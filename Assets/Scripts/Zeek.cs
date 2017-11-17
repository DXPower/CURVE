using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zeek : MonoBehaviour {
	public float duration = 100;
	public float time = 0;
	float mid;

	public Vector2 initialSize;
	public Vector2 finalSize;

	public RectTransform rt;

	void Start() {
		rt = GetComponent<RectTransform>();
		rt.position = CutsceneManager.currentCutscene.alien.GetComponent<RectTransform>().position;
		initialSize = rt.sizeDelta;
		mid = duration / 2f;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (time <= mid) {
			rt.sizeDelta = Vector2.Lerp(initialSize, finalSize, time / mid);
		} else if (time <= duration) {
			rt.sizeDelta = Vector2.Lerp(finalSize, initialSize, (time - mid) / mid);
		} else {
			Destroy(gameObject);
		}

		time++;
	}
}
