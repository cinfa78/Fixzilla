using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour {
	private int playerId = 0;
	private Player rewiredPlayer;

	private void Start() {
		Cursor.visible = false;
		rewiredPlayer = ReInput.players.GetPlayer(playerId);
		rewiredPlayer.AddInputEventDelegate(OnStart, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed,
			RewiredConsts.Action.Start);
	}

	private void OnStart(InputActionEventData obj) {
		Debug.Log($"{name} start pressed");
		SceneManager.LoadScene(1);
	}
}