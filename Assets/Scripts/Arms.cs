using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arms : MonoBehaviour {
	public bool CanGrasp;

	private void OnDrawGizmos() {
		Gizmos.DrawWireSphere(transform.position,0.03f);
	}

	private void OnTriggerStay2D(Collider2D other) {
		if (other.gameObject.layer == LayerMask.NameToLayer("BuildingPiece")) {
			CanGrasp = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (CanGrasp) {
			if (other.gameObject.layer == LayerMask.NameToLayer("BuildingPiece")) {
				CanGrasp = false;
			}
		}
	}
}