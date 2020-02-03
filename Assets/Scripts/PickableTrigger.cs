using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableTrigger : MonoBehaviour {
	public PickableObject pickable;

	public (Transform, Transform) GetGraspPoints() {
		return pickable.GetGraspPoints();
	}

	public PickableObject GetPickableObject() {
		return pickable;
	}

	public bool CanBePicked() {
		return !pickable.isPlaced;
	}
}