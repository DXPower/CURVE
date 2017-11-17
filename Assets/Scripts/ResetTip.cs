using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTip : MonoBehaviour {
	static RectTransform rt;
	public static ResetTip self;
	static float startHeight;

	public float change;

	public int duration;

	void Start() {
		self = this;
		rt = gameObject.GetComponent<RectTransform>();
		startHeight = rt.position.y;
	}

	public static IEnumerator Show() {
		for (int i = 0; i < self.duration; i++) {
			rt.position = new Vector3(rt.position.x, EaseOutQuart(i, startHeight, self.change, self.duration), rt.position.z);
			yield return new WaitForSeconds(0.01f);
		}
	}

	public static void Hide() {
		rt.position = new Vector3(rt.position.x, startHeight, rt.position.z);
	}

	public static float EaseOutQuart(float t, float b, float c, float d) {
		t /= d;
		t--;
		return -c * (t * t * t * t - 1) + b;
	}
}
