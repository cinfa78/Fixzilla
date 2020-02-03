using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineOffset : MonoBehaviour {
	public float amplitude;
	public float offset;
	public float frequency;
	public bool x;
	public bool y;
	private Vector3 startingPosition;

	private void Awake() {
		startingPosition = transform.position;
	}

	// Update is called once per frame
	private void Update() {
		transform.position = startingPosition + (x ? amplitude * Mathf.Sin(Time.time * frequency + offset) * Vector3.right : Vector3.zero) +
		                     (y ? amplitude * Mathf.Sin(Time.time * frequency + offset) * Vector3.up : Vector3.zero);
	}
}