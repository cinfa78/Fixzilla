using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class PickableObject : MonoBehaviour {
	public SpriteRenderer sprite;
	public SpriteRenderer breakingSprite;
	public SpriteRenderer brokenSprite;

	public SpriteRenderer highlightSprite;
	public GameObject breakingEffect;
	[FormerlySerializedAs("breakingEffect")]
	public GameObject brokeEffect;
	public Transform leftGraspPoint;
	public Transform rightGraspPoint;
	public Transform topGraspPoint;
	public Transform bottomGraspPoint;
	public PickableTrigger pickableTrigger;
	public float maxGraspBeforeBreak;
	//public Gradient healthColorGradient;
	public float health;
	public float maxHealth;
	public bool isRoof;
	public Rigidbody2D rigidbody2D;
	private bool isHeld;
	public bool isPlaced;
	public Transform snapPoint;
	public bool snapOpen;
	public PlacedTrigger placedTrigger;
	public PickableObject placedOn;
	[Title("Sounds")] public AudioClip placedClip;
	public AudioClip grabbedClip;
	public AudioClip destroyedClip;
	public AudioClip graspingClip;

	private AudioSourceMultichannel audioSourceMultichannel;

	private float relativeVelcotiy = 0;
	private Vector3 previousPosition;
	private Vector3 currentPosition;

	public event Action<bool, Vector3> Placed;
	public event Action Fallen;
	public event Action Broken;

	private void Awake() {
		rigidbody2D = GetComponent<Rigidbody2D>();
		if (isPlaced) {
			rigidbody2D.mass = 1000;
		}
		breakingSprite.enabled = false;
		brokenSprite.enabled = false;
		highlightSprite.enabled = false;
		brokeEffect.SetActive(true);
		health = maxHealth;
		//sprite.color = healthColorGradient.Evaluate(health / maxHealth);
		audioSourceMultichannel = GetComponent<AudioSourceMultichannel>();
		previousPosition = transform.position;
		currentPosition = transform.position;
	}

	public void ApplyGrasp(float graspStrength, float graspDamage) {
		if (health > 0) {
			//sprite.color = healthColorGradient.Evaluate(health / maxHealth);
			if (graspStrength > maxGraspBeforeBreak) {
				breakingEffect.GetComponent<ParticleSystem>().Emit(1);
				// if (graspingClip != null) {
				// 	audioSourceMultichannel.GetChannel().PlayOneShot(graspingClip);
				// }
				health -= graspDamage * Time.deltaTime;
				if (health < 0.5 * maxHealth) {
					sprite.enabled = false;
					breakingSprite.enabled = true;
				}
				if (health <= 0) {
					OnBreak();
				}
			}
		}
	}

	public void OnBreak() {
		if (destroyedClip != null) {
			audioSourceMultichannel.GetChannel().PlayOneShot(destroyedClip);
		}
		isPlaced = true;
		Placed?.Invoke(false, transform.position);
		rigidbody2D.mass = 10;
		snapOpen = false;
		sprite.enabled = false;
		breakingSprite.enabled = false;
		brokenSprite.enabled = true;
		highlightSprite.enabled = false;
		isHeld = false;
		placedTrigger.active = false;
		placedTrigger.pickableObject = null;
		placedTrigger.canBePlaced = false;
		brokeEffect.SetActive(true);
		brokeEffect.GetComponent<ParticleSystem>().Play();
		health = 0;
		Fallen?.Invoke();
		Broken?.Invoke();
		pickableTrigger.enabled = false;
		StartCoroutine(FadingOutBroken());
	}

	private IEnumerator FadingOutBroken() {
		float timer = 0;
		float duration = 1f;
		Color startingColor = brokenSprite.color;
		Color endingColor = Color.clear;
		while (timer < duration) {
			brokenSprite.color = Color.Lerp(startingColor, endingColor, timer / duration);
			timer += Time.deltaTime;
			yield return null;
		}
		brokenSprite.color = endingColor;
	}

	public void RotateLeft() {
		Transform temp = leftGraspPoint;
		leftGraspPoint = topGraspPoint;
		topGraspPoint = rightGraspPoint;
		rightGraspPoint = bottomGraspPoint;
		bottomGraspPoint = temp;
	}

	public void RotateRight() {
		Transform temp = rightGraspPoint;
		leftGraspPoint = bottomGraspPoint;
		topGraspPoint = leftGraspPoint;
		rightGraspPoint = topGraspPoint;
		bottomGraspPoint = temp;
	}

	public (Transform, Transform) GetGraspPoints() {
		return (leftGraspPoint, rightGraspPoint);
	}

	public void Highlight(bool turnOn) {
		highlightSprite.enabled = turnOn;
	}

	public void Grabbed() {
		if (health > 0 && !isHeld) {
			if (grabbedClip != null) {
				audioSourceMultichannel.GetChannel().PlayOneShot(grabbedClip);
			}
		}
		isHeld = true;
		highlightSprite.enabled = false;
		pickableTrigger.enabled = false;
	}

	public void Left() {
		isHeld = false;
		highlightSprite.enabled = false;
		if (placedTrigger.canBePlaced) {
			rigidbody2D.constraints |= RigidbodyConstraints2D.FreezeAll;
			//rigidbody2D.mass = 10000;
			transform.position = placedTrigger.whereToPlace.position;
			placedTrigger.pickableObject.snapOpen = false;
			placedOn = placedTrigger.pickableObject;
			isPlaced = true;
			if (placedClip != null) {
				audioSourceMultichannel.GetChannel().PlayOneShot(placedClip);
			}
			//placedTrigger.whereToPlaceHightlight.SetActive(false);
			Placed?.Invoke(true, transform.position);
		}
		else {
			pickableTrigger.enabled = false;
			rigidbody2D.AddForce((currentPosition - previousPosition) * 300f, ForceMode2D.Impulse);
		}
	}

	public void Update() {
		currentPosition = transform.position;
		relativeVelcotiy = Vector3.Magnitude(currentPosition - previousPosition) / Time.deltaTime;
		//Highlight(false);
	}

	private void LateUpdate() {
		previousPosition = transform.position;
	}
}