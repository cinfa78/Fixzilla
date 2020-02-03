using UnityEngine;
using UnityEngine.Audio;

public class AudioSourceMultichannel : MonoBehaviour {
	#region Fields

	public int numberOfChannels;
	public AudioMixerGroup mixerGroup;
	private AudioSource[] channels;

	#endregion

	private void Awake() {
		channels = new AudioSource[numberOfChannels];
		for (int i = 0; i < numberOfChannels; i++) {
			channels[i] = gameObject.AddComponent<AudioSource>();
			channels[i].playOnAwake = false;
			channels[i].loop = false;
			channels[i].outputAudioMixerGroup = mixerGroup;
		}
	}

	#region Methods

	public AudioSource GetChannel() {
		foreach (AudioSource channel in channels) {
			if (!channel.isPlaying) {
				return channel;
			}
		}
		return channels[0];
	}

	#endregion
}