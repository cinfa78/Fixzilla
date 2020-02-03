using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour {
	public Animator fadeAnimator;
	private int playerId = 0;
	private Player rewiredPlayer;

	private void Awake() {
		rewiredPlayer = ReInput.players.GetPlayer(playerId);
		
	}

	private void OnEnable() {
		rewiredPlayer.AddInputEventDelegate(OnStart, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed,
			RewiredConsts.Action.Start);
	}

	private void OnStart(InputActionEventData obj) {
		StopAllCoroutines();
		LoadScene();
	}
	
	private void Start() {
		StartCoroutine(LoadingGame());
	}

	private IEnumerator LoadingGame() {
		yield return new WaitForSeconds(8.5f);
		fadeAnimator.SetTrigger("fadeout");
		yield return new WaitForSeconds(2.2f);
		LoadScene();
	}

	private void LoadScene() {
		SceneManager.LoadScene("Fixzilla");
	}
}