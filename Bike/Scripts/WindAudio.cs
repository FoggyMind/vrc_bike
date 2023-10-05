
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class WindAudio : UdonSharpBehaviour {
	// For proper crossfading, the clips pitches should all match, with an octave offset between low and high.
	public AudioSource lowAccel;
	public AudioSource highAccel;
	[Space]
	public AudioClip lowAccelClip;
	public AudioClip highAccelClip;
	[Space]
	public bool simpleMode = true;               // Single AudioSource engine sound
	public float lowPitchMin = 0.1f;             // The lowest possible pitch for the low sounds
	public float lowPitchMax = 5.0f;             // The highest possible pitch for the low sounds
	public float pitchMultiplier = 0.6f;         // Used for altering the pitch of audio clips
	public float highPitchMultiplier = 0.25f;    // Used for altering the pitch of high sounds

	[Space]
	public float maxVelocity = 40f;
	private float revs = 0.0f;
	private Vector3 prevPosition;

	void Start() {
		SetUpEngineAudioSource(lowAccel, lowAccelClip);
		SetUpEngineAudioSource(highAccel, highAccelClip);
	}

	private void SetUpEngineAudioSource(AudioSource source, AudioClip clip) {
		if (source == null || clip == null) {
			return;
		}

		source.clip = clip;
		source.volume = 0;
		source.loop = true;
		source.time = Random.Range(0f, clip.length); // start the clip from a random point
		source.minDistance = 1;
		source.maxDistance = 2;
		source.dopplerLevel = 0;
		source.Play();
	}

	private void Update() {
		prevPosition = transform.position;
		transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

		var velocity = (transform.position - prevPosition).magnitude / Time.deltaTime * 3.6f;

		revs = Mathf.Lerp(revs, Mathf.Min(velocity / maxVelocity, 1f), Time.deltaTime / 0.1f);

		UpdateAudio();
	}

	private void UpdateAudio() {
		// The pitch is interpolated between the min and max values, according to the car's revs.
		float pitch = ULerp(lowPitchMin, lowPitchMax, revs);

		// Clamp to minimum pitch (note, not clamped to max for high revs while burning out)
		pitch = Mathf.Min(lowPitchMax, pitch);

		if (simpleMode) {
			highAccel.pitch = pitch * pitchMultiplier * highPitchMultiplier;
			highAccel.volume = revs;
		}
		else {
			// get values for fading the sounds based on the acceleration
			float accFade = 1;// Mathf.Abs(velocity);

			// get the high fade value based on the cars revs
			float highFade = Mathf.InverseLerp(0.2f, 0.8f, revs);
			float lowFade = 1 - highFade;

			// adjust the values to be more realistic
			highFade = 1 - ((1 - highFade) * (1 - highFade));
			lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
			accFade = 1 - ((1 - accFade) * (1 - accFade));

			// adjust the pitches based on the multipliers
			lowAccel.pitch = pitch * pitchMultiplier;
			highAccel.pitch = pitch * highPitchMultiplier * pitchMultiplier;

			// adjust the source volumes based on the fade values
			lowAccel.volume = lowFade * accFade;
			highAccel.volume = highFade * accFade;
		}
	}

	private float ULerp(float from, float to, float value) {
		// unclamped versions of Lerp and Inverse Lerp, to allow value to exceed the from-to range
		return (1.0f - value) * from + value * to;
	}
}
