using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraManager : MonoBehaviour {
	public float shakeBias = 0.05f;
	private Vector3 defaultPosition;
	private GameObject camera;
	private Fixzilla fixzilla;
	private GlobalPiecesController globalPiecesController;
	private Coroutine shakingCoroutine;
	public float duration = 0.5f;
	private float currentDuration;
	private int playerId;
	private Player rewiredPlayer;

	private void Awake() {
		globalPiecesController = FindObjectOfType<GlobalPiecesController>();
	}

	private void Start() {
		fixzilla = FindObjectOfType<Fixzilla>();
		fixzilla.TooFast += OnFixzillaRun;
		fixzilla.GetComponent<JumpingObject>().Landed += OnFixzillaLanded;
		camera = Camera.main.gameObject;
		defaultPosition = camera.transform.localPosition;
		rewiredPlayer = ReInput.players.GetPlayer(playerId);
	}

	private void OnFixzillaLanded() {
		rewiredPlayer.SetVibration(1, .5f, .5f);
		Shake(.2f);
	}

	private void OnFixzillaRun(float speed) {
		Shake(speed);
	}

	[Button("Shake)")]
	public void Shake(float strength) {
		if (shakingCoroutine != null) {
			currentDuration = duration;
		}
		else {
			currentDuration = duration;
			shakingCoroutine = StartCoroutine(Shaking(strength));
		}
	}

	private IEnumerator Shaking(float strength) {
		float t = 0;
		float duration = globalPiecesController.cooldown;
		rewiredPlayer.SetVibration(0, .5f, currentDuration);
		while (t < currentDuration) {
			float dampingStrength = 1; //1 - (t / duration);
			camera.transform.localPosition = defaultPosition + new Vector3(Random.Range(-shakeBias * strength, shakeBias * strength) * dampingStrength,
				                                 Random.Range(-shakeBias * strength, shakeBias * strength) * dampingStrength, defaultPosition.z);
			t += Time.deltaTime;
			yield return null;
		}
		camera.transform.localPosition = defaultPosition;
		shakingCoroutine = null;
	}
}