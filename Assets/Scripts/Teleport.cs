using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {
	public GameObject complement;
	private Teleport ctp;

	public bool onCooldown = false;

	public void Init(GameObject complement) {
		this.complement = complement;
		ctp = complement.GetComponent<Teleport>();
	}

	IEnumerator TeleportGameobject(GameObject go) {
		Renderer r = go.GetComponent<Renderer>();
		Collider c = go.GetComponent<Collider>();
		Rigidbody rb = go.GetComponent<Rigidbody>();
		Vector3 oV = rb.velocity;
		int oRot = Board.currentRotation;
		r.enabled = false;
		c.enabled = false;
		rb.velocity = Vector3.zero;
		StartCoroutine(Cooldown());

		yield return new WaitForSeconds(1.5f);

		int dif = Board.currentRotation - oRot;
		switch (dif) {
			case 90:
				oV = new Vector3(-oV.y, oV.x, oV.z);
				break;
			case 180:
				oV = new Vector3(-oV.x, -oV.y, oV.z);
				break;
			case 270:
				oV = new Vector3(oV.y, -oV.x, oV.z);
				break;
			case -90:
				oV = new Vector3(oV.y, -oV.x, oV.z);
				break;
			case -180:
				oV = new Vector3(-oV.x, -oV.y);
				break;
			case -270:
				oV = new Vector3(-oV.y, oV.x, oV.z);
				break;
		}

		go.transform.position = complement.transform.position;
		r.enabled = true;
		c.enabled = true;
		rb.velocity = oV;

	}

	IEnumerator Cooldown() {
		onCooldown = true;
		ctp.onCooldown = true;

		yield return new WaitForSeconds(3f);

		onCooldown = false;
		ctp.onCooldown = false;
	}

	private void OnTriggerEnter(Collider other) {
		if (!onCooldown && !ctp.onCooldown && other.CompareTag("MainObject") || other.CompareTag("Object")) {
			StartCoroutine(TeleportGameobject(other.gameObject));
		}
	}
}
