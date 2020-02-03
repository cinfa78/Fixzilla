using UnityEngine;

public class AnimatorSound : MonoBehaviour {
	public AudioClip[] audioClips;
	private AudioSource audioSource;

	private void Awake() {
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.loop = false;
	}

	public void PlaySound(int sound) {
		audioSource.PlayOneShot(audioClips[sound]);
	}
}