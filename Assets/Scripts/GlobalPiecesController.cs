using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GlobalPiecesController : MonoBehaviour {
	public List<PickableObject> pickableObjects;
	public float timeBeforeEarthquakeDamage = 1f;
	public float cooldown = 2f;

	public CrowdController[] crowds;

	public TMP_Text endingText;
	public ParticleSystem WellPlacedParticles;

	private Coroutine preparingEarthquakeCoroutine;
	private Fixzilla fixzilla;
	private float cooldownTimer;

	private float globalHealth = 0;
	private float finalHealth = 0;

	public event Action<float> GameOver;
	public event Action GoodAction;
	public event Action BadAction;

	public AudioClip backgroundMusic;
	public AudioClip cheeringClip;
	public AudioClip booingClip;
	public AudioClip earthquakeClip;
	private AudioSource audioSource;
	private AudioSourceMultichannel audioSourceMultichannel;
	private int activeCrowds;
	private int playerId = 0;
	private Player rewiredPlayer;

	private void Awake() {
		fixzilla = FindObjectOfType<Fixzilla>();
		fixzilla.TooFast += PrepareEarthquake;
		fixzilla.NormalSpeed += StopEarthQuake;
		fixzilla.GetComponent<JumpingObject>().Landed += OnEarthQuake;
		audioSource = gameObject.AddComponent<AudioSource>();
		cooldownTimer = cooldown;
		audioSourceMultichannel = GetComponent<AudioSourceMultichannel>();
		foreach (var crowd in crowds) {
			crowd.gameObject.SetActive(false);
		}
		activeCrowds = 0;
		rewiredPlayer = ReInput.players.GetPlayer(playerId);
	}

	private void OnEarthQuake() {
		OnEarthQuake(1f);
	}

	private void StopEarthQuake() {
		if (preparingEarthquakeCoroutine != null) {
			StopCoroutine(preparingEarthquakeCoroutine);
			preparingEarthquakeCoroutine = null;
		}
	}

	private void PrepareEarthquake(float strengthMoving) {
		if (preparingEarthquakeCoroutine == null) {
			preparingEarthquakeCoroutine = StartCoroutine(PreparingEarthquake(strengthMoving));
		}
	}

	private IEnumerator PreparingEarthquake(float strengthMoving) {
		yield return new WaitForSeconds(timeBeforeEarthquakeDamage);
		OnEarthQuake(strengthMoving);
		StopEarthQuake();
	}

	private void Start() {
		audioSource.loop = true;
		if (backgroundMusic != null) {
			audioSource.clip = backgroundMusic;
			audioSource.Play();
		}
		var tempObjects = FindObjectsOfType<PickableObject>();
		pickableObjects = new List<PickableObject>();
		foreach (var tempObject in tempObjects) {
			if (!tempObject.isPlaced) {
				pickableObjects.Add(tempObject);
				globalHealth += tempObject.maxHealth;
				tempObject.Placed += CheckEverythingPlaced;
				tempObject.Placed += OnPiecePlaced;
				tempObject.Fallen += OnPieceFallen;
			}
		}
		rewiredPlayer.AddInputEventDelegate(OnRestart, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed,
			RewiredConsts.Action.Restart);
	}

	private void OnRestart(InputActionEventData obj) {
		SceneManager.LoadScene(0);
	}

	private void OnPieceFallen() {
		BadAction?.Invoke();
		if (activeCrowds > 0) {
			activeCrowds--;
			crowds[activeCrowds].gameObject.SetActive(false);
		}
		audioSourceMultichannel.GetChannel().PlayOneShot(booingClip);
	}

	private void OnPiecePlaced(bool lastPlaceGood, Vector3 positionPlaced) {
		GoodAction?.Invoke();
		crowds[activeCrowds].gameObject.SetActive(true);
		activeCrowds++;
		if (lastPlaceGood) {
			audioSourceMultichannel.GetChannel().PlayOneShot(cheeringClip);
			WellPlacedParticles.transform.position = positionPlaced;
			WellPlacedParticles.Play();
		}
		else {
			audioSourceMultichannel.GetChannel().PlayOneShot(booingClip);
		}
	}

	private void OnDestroy() {
		foreach (var tempObject in pickableObjects) {
			tempObject.Placed -= CheckEverythingPlaced;
			tempObject.Placed += OnPiecePlaced;
			tempObject.Fallen += OnPieceFallen;
		}
	}

	private void OnEarthQuake(float strength) {
		if (earthquakeClip != null) {
			audioSourceMultichannel.GetChannel().PlayOneShot(earthquakeClip);
		}
		if (cooldownTimer >= cooldown) {
			cooldownTimer = 0;
			foreach (var pickableObject in pickableObjects) {
				if (pickableObject.isPlaced && (pickableObject.snapOpen || pickableObject.isRoof)) {
					pickableObject.isPlaced = false;
					if (pickableObject.placedOn != null) {
						pickableObject.placedOn.snapOpen = true;
					}
					pickableObject.placedOn = null;
					pickableObject.rigidbody2D.constraints &= ~RigidbodyConstraints2D.FreezePosition;
					pickableObject.rigidbody2D.AddRelativeForce(new Vector2(0.5f, 1f));
				}
				if (!pickableObject.isPlaced) {
					pickableObject.rigidbody2D.AddRelativeForce(new Vector2(-Random.Range(-1f, 1f), 1f));
				}
			}
			OnPieceFallen();
		}
	}

	private void LateUpdate() {
		if (cooldownTimer < cooldown) {
			cooldownTimer += Time.deltaTime;
		}
	}

	public void TurnOffHighlights() {
		foreach (var pickableObject in pickableObjects) {
			pickableObject.Highlight(false);
		}
	}
	public void CheckEverythingPlaced(bool lastPlaceGood, Vector3 positionPlaced) {
		bool ok = true;
		finalHealth = 0;
		foreach (var pickableObject in pickableObjects) {
			finalHealth += pickableObject.health;
			ok &= (pickableObject.isPlaced || pickableObject.health <= 0);
		}
		if (ok) {
			GameOver?.Invoke(finalHealth / globalHealth);
			if (finalHealth / globalHealth < 0.5f) {
				endingText.text = "Good but not excellent";
			}
			else {
				endingText.text = "Domo arigato, Fixzilla san!";
			}
		}
	}
}