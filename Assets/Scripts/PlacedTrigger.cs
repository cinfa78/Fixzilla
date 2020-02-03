using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedTrigger : MonoBehaviour {
	public bool canBePlaced;
	public Transform whereToPlace;
	public PickableObject pickableObject;
	public bool active = true;
	//public GameObject whereToPlaceHightlight;

	// private void Awake() {
	// 	whereToPlaceHightlight.SetActive(false);
	// }

	// private void Update() {
	// 	if (whereToPlace != null) {
	// 		whereToPlaceHightlight.transform.position = whereToPlace.transform.position;
	// 	}
	// }

	private void OnTriggerStay2D(Collider2D other) {
		if (active) {
			if (other.gameObject.layer == LayerMask.NameToLayer("BuildingPiece")) {
				if (other.GetComponent<PickableObject>().isPlaced && other.GetComponent<PickableObject>().snapOpen) {
					canBePlaced = true;
					pickableObject = other.GetComponent<PickableObject>();
					whereToPlace = other.GetComponent<PickableObject>().snapPoint;
					//whereToPlaceHightlight.SetActive(true);
				}
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (active) {
			if (other.gameObject.layer == LayerMask.NameToLayer("BuildingPiece")) {
				if (other.GetComponent<PickableObject>().isPlaced) {
					canBePlaced = false;
					//whereToPlaceHightlight.SetActive(false);
				}
			}
		}
	}
}