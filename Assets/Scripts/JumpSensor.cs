using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpSensor : MonoBehaviour {
	public JumpingObject jumpingObject;
	public bool isJumping;
	public bool isOnGround;
	private int floorLayer;

	private void Awake() {
		floorLayer = LayerMask.NameToLayer("Floor");
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.layer == floorLayer) {
			isJumping = true;
			isOnGround = !isJumping;
		}
	}

	private void OnTriggerStay2D(Collider2D other) {
		OnTriggerEnter2D(other);
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.layer == floorLayer) {
			if (isJumping) {
				jumpingObject.Land();
			}
			isJumping = false;
			isOnGround = !isJumping;
		}
	}
}