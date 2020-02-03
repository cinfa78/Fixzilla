using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdController : MonoBehaviour {
	public ParticleSystem confetti;
	private GlobalPiecesController globalPiecesController;
	private Animator animator;

	private void Awake() {
		animator = GetComponent<Animator>();
		globalPiecesController = FindObjectOfType<GlobalPiecesController>();
		globalPiecesController.GoodAction += Cheer;
	}

	private void OnEnable() {
		animator.SetTrigger("arrive");
	}

	private void OnDisable() {
		animator.SetTrigger("leave");
	}

	public void Cheer() {
		confetti.Play();
	}
}