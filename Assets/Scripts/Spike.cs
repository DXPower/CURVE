using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		if (other.CompareTag("MainObject") || other.CompareTag("Object")) {
			other.GetComponent<Renderer>().enabled = false;
			other.GetComponent<Collider>().enabled = false;
		}
	}
}
