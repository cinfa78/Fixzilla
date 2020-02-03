using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUi : MonoBehaviour {
	public GameObject[] turnOnOnGameOver;
	public TMP_Text resultLabel;
	private GlobalPiecesController globalPiecesController;

	private bool isGameOver;

	private void Awake() {
		globalPiecesController = FindObjectOfType<GlobalPiecesController>();
		globalPiecesController.GameOver += OnGameOver;
	}

	private void OnEnable() {
		isGameOver = false;
		foreach (var VARIABLE in turnOnOnGameOver) {
			VARIABLE.SetActive(false);
			VARIABLE.SetActive(false);
		}
	}

	private void OnDestroy() {
		globalPiecesController.GameOver -= OnGameOver;
	}

	private void Update() {
		if (Input.GetKey(KeyCode.Escape)) {
			Application.Quit();
		}
		if (isGameOver) {
			if (Input.GetKey(KeyCode.R)) {
				SceneManager.LoadScene(0);
			}
		}
	}

	private void OnGameOver(float finalHealth) {
		foreach (var VARIABLE in turnOnOnGameOver) {
			VARIABLE.SetActive(true);
		}
		resultLabel.text = $"Success rate: {Mathf.Round(finalHealth * 100)}%";
		isGameOver = true;
	}
}