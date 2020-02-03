using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class Fixzilla : MonoBehaviour {
	public float maxSpeed = 1.5f;
	public float tooFastThreshold = 1f;
	public GameObject leftHand;
	public SpriteRenderer leftHandSprite;
	public GameObject rightHand;
	public SpriteRenderer rightHandSprite;
	public float minGraspDifferenceBetweenHands = 0.5f;
	[FormerlySerializedAs("graspBias")] public float graspMovementBias = 0.5f;
	[FormerlySerializedAs("restLayer")] public int restingLayer = 2;
	public int graspingLayer = 4;
	public float minGraspStrength;

	public float graspDamage = 2f;

	public GameObject armsContainer;
	[FormerlySerializedAs("armsBias")] public float armsMovementBias = 0.225f;

	public float searchingRadius;
	public float jumpStrength = 10f;
	[Title("graphics")] public GameObject spriteContainer;
	public Animator animator;
	public ParticleSystem stompClouds;
	[Title("sfx")] public AudioClip roarClip;
	[Title("Debug")] private Vector3 armsDefaultPosition;
	private Transform leftHandDefaultTransform;
	private Transform rightHandDefaultTransform;
	private Arms arms;

	private AudioSourceMultichannel audioSourceMultichannel;
	private Vector3 leftHandDefaultOffset;
	private Vector3 rightHandDefaultOffset;
	private Vector3 leftHandCustomRestOffset;
	private Vector3 rightHandCustomRestOffset;

	private int playerId = 0;
	private Player rewiredPlayer;
	private Rigidbody2D rigidbody2D;

	private GameObject possiblePick;
	private PickableObject pickedObject;
	[ShowInInspector] private bool isHoldingSomething;
	[ShowInInspector] private bool canPickSomething;
	[ShowInInspector] private float appliedStrength;

	private Vector3 desiredLeftHandLocalPosition;
	private Vector3 desiredRightHandLocalPosition;

	private GlobalPiecesController globalPiecesController;
	private bool isGameOver;
	private JumpingObject jumpingObject;

	public event Action<float> TooFast;
	public event Action NormalSpeed;

	private void Awake() {
		rigidbody2D = GetComponent<Rigidbody2D>();
		leftHandDefaultTransform = leftHand.transform;
		rightHandDefaultTransform = rightHand.transform;

		leftHandDefaultOffset = leftHand.transform.localPosition;
		rightHandDefaultOffset = rightHand.transform.localPosition;
		armsDefaultPosition = armsContainer.transform.localPosition;
		leftHandCustomRestOffset = leftHandDefaultOffset;
		rightHandCustomRestOffset = rightHandDefaultOffset;
		arms = armsContainer.GetComponent<Arms>();
		globalPiecesController = FindObjectOfType<GlobalPiecesController>();
		spriteContainer.transform.localScale = Vector3.one;
		rewiredPlayer = ReInput.players.GetPlayer(playerId);
		jumpingObject = GetComponent<JumpingObject>();
		audioSourceMultichannel = GetComponent<AudioSourceMultichannel>();
	}

	private void OnEnable() {
		isGameOver = false;
		globalPiecesController.GameOver += OnGameOver;
		rewiredPlayer.AddInputEventDelegate(OnJump, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed,
			RewiredConsts.Action.Jump);
	}

	private void OnJump(InputActionEventData obj) {
		jumpingObject.Jump();
	}

	private void OnDisable() {
		isGameOver = false;
		globalPiecesController.GameOver -= OnGameOver;
	}

	private void OnGameOver(float finalHealth) {
		isGameOver = true;
	}

	private void OnDrawGizmos() {
		Gizmos.DrawWireSphere(transform.position, searchingRadius);
	}

	private void Update() {
		float move = rewiredPlayer.GetAxis(RewiredConsts.Action.Move) * maxSpeed;
		if (move * move > 0.04f) {
			animator.SetBool("isWalking", true);
			spriteContainer.transform.localScale = new Vector3(move >= 0 ? 1f : -1f, 1f, 1f);
			if (jumpingObject.jumpSensor.isOnGround) {
				rigidbody2D.MovePosition(transform.position + Vector3.right * (move * maxSpeed * Time.fixedDeltaTime));
			}
			if (!isGameOver && Mathf.Abs(move) > tooFastThreshold) {
				if (jumpingObject.jumpSensor.isOnGround) {
					TooFast?.Invoke(Mathf.Abs(move) / maxSpeed - tooFastThreshold);
					if (!stompClouds.isPlaying) {
						stompClouds.Play();
					}
				}
			}
			else {
				NormalSpeed?.Invoke();
				if (stompClouds.isPlaying) {
					stompClouds.Stop();
				}
			}
		}
		else {
			animator.SetBool("isWalking", false);
		}
		float leftGrasp = rewiredPlayer.GetAxis(RewiredConsts.Action.Grab_Left);
		float rightGrasp = rewiredPlayer.GetAxis(RewiredConsts.Action.Grab_Right);
		if (leftGrasp > 0 && rightGrasp > 0 && Mathf.Abs(leftGrasp - rightGrasp) < minGraspDifferenceBetweenHands) {
			appliedStrength = leftGrasp + rightGrasp;
		}
		else {
			appliedStrength = 0;
		}
		animator.SetFloat("eyes", appliedStrength / 2f);
		if (!isHoldingSomething) {
			var triggers = Physics2D.CircleCastAll(transform.position, searchingRadius, Vector2.up, 1f);
			PickableTrigger closestTrigger = null;
			float minDistance = 100f;
			globalPiecesController.TurnOffHighlights();
			foreach (var trigger in triggers) {
				if (trigger.collider.gameObject.layer == LayerMask.NameToLayer("BuildPieceSelfCollision")) {
					var distance = Vector2.Distance(trigger.collider.gameObject.transform.position, transform.position);
					var t = trigger.collider.gameObject.GetComponent<PickableTrigger>();
					if (t != null && t.CanBePicked() && distance < minDistance) {
						minDistance = distance;
						closestTrigger = trigger.collider.gameObject.GetComponent<PickableTrigger>();
					}
				}
			}
			if (closestTrigger != null) {
				possiblePick = closestTrigger.GetPickableObject().gameObject;
				closestTrigger.GetPickableObject().Highlight(true);
				(Transform, Transform) graspPoints = closestTrigger.GetGraspPoints();
				leftHandCustomRestOffset = armsContainer.transform.worldToLocalMatrix * (new Vector3(graspPoints.Item1.localPosition.x, leftHandDefaultOffset.y, 0));
				rightHandCustomRestOffset = armsContainer.transform.worldToLocalMatrix * (new Vector3(graspPoints.Item2.localPosition.x, leftHandDefaultOffset.y, 0));
			}
			else {
				possiblePick = null;
				leftHandCustomRestOffset = leftHandDefaultOffset;
				rightHandCustomRestOffset = rightHandDefaultOffset;
			}
			desiredLeftHandLocalPosition = leftHandCustomRestOffset;
			desiredRightHandLocalPosition = rightHandCustomRestOffset;

			if (appliedStrength > minGraspStrength && arms.CanGrasp && possiblePick != null) {
				leftHandSprite.sortingOrder = graspingLayer;
				rightHandSprite.sortingOrder = graspingLayer;
				pickedObject = possiblePick.GetComponent<PickableObject>();
				pickedObject.Grabbed();
				isHoldingSomething = true;
			}
			else {
				//leftHand.transform.localPosition = leftHandCustomRestOffset;
				//rightHand.transform.localPosition = rightHandCustomRestOffset;
				leftHandSprite.sortingOrder = restingLayer;
				rightHandSprite.sortingOrder = restingLayer;
				isHoldingSomething = false;
			}
			leftHand.transform.localPosition = leftHandCustomRestOffset + Vector3.right * (graspMovementBias * leftGrasp);
			rightHand.transform.localPosition = rightHandCustomRestOffset + Vector3.left * (graspMovementBias * rightGrasp);
		}
		leftHand.transform.localPosition = Vector3.Lerp(leftHand.transform.localPosition, desiredLeftHandLocalPosition, 0.6f);
		rightHand.transform.localPosition = Vector3.Lerp(rightHand.transform.localPosition, desiredRightHandLocalPosition, 0.6f);
		if (isHoldingSomething) {
			pickedObject.transform.position = armsContainer.transform.position;
			pickedObject.ApplyGrasp(appliedStrength, graspDamage);
			if (pickedObject.health == 0) {
				Drop();
			}
		}

		if (isHoldingSomething && appliedStrength < Mathf.Max(minGraspStrength - 0.2f, 0f)) {
			Drop();
		}
		float armsAxis = rewiredPlayer.GetAxis(RewiredConsts.Action.Lift);
		armsContainer.transform.localPosition = armsDefaultPosition + Vector3.up * (armsAxis * armsMovementBias);
	}

	private void Drop() {
		isHoldingSomething = false;
		pickedObject.Left();
	}
}