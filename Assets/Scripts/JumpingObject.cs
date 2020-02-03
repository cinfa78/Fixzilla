using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingObject : MonoBehaviour {
	public JumpSensor jumpSensor;
	private Rigidbody2D rigidbody2D;
	public float jumpStrength = 200;
	public event Action Landed;
	public AudioClip landingClip;
	private AudioSourceMultichannel audioSourceMultichannel;
	public ParticleSystem stompCloud;

	private void Awake() {
		rigidbody2D = GetComponent<Rigidbody2D>();
		audioSourceMultichannel = GetComponent<AudioSourceMultichannel>();
	}

	public void Jump() {
		if (!jumpSensor.isJumping) {
			rigidbody2D.AddForce(Vector2.up * jumpStrength, ForceMode2D.Impulse);
		}
	}

	public void Land() {
		audioSourceMultichannel.GetChannel().PlayOneShot(landingClip);
		if (stompCloud != null) {
			stompCloud.Emit(10);
		}
		Landed?.Invoke();
	}
}