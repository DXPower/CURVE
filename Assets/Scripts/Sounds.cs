using UnityEngine;

public class Sounds {
	public static AudioClip turningSound;
	public static AudioClip transitionSound;
	public static AudioClip resetSound;
	public static AudioClip doorSound;
	public static AudioClip bigExplosionSound;
	public static AudioClip smallExplosionSound;

	public static AudioSource audioSource;

	public static void LoadAllSounds() {
		audioSource = GameObject.Find("Main Camera").AddComponent<AudioSource>();
		turningSound = Resources.Load<AudioClip>("Sounds/Turning");
		transitionSound = Resources.Load<AudioClip>("Sounds/Transition");
		resetSound = Resources.Load<AudioClip>("Sounds/Reset");
		doorSound = Resources.Load<AudioClip>("Sounds/DoorSound");
		bigExplosionSound = Resources.Load<AudioClip>("Sounds/ExplosionBig");
		smallExplosionSound = Resources.Load<AudioClip>("Sounds/ExplosionSmall");
	}

	public static AudioClip LoadRandomSound(AudioClip[] sounds) {
		if (sounds == null) return null;
		return sounds[Random.Range(0, sounds.Length)];
	}

	public static void PlaySound(ref AudioClip clip) {
		if (clip == null) return;
		audioSource.PlayOneShot(clip);
	}

	public static void PlaySound(AudioClip clip, AudioSource source, bool loop) {
		if (clip == null || source == null) return;
		source.PlayOneShot(clip);
		source.loop = loop;
	}

	public static void StopSound(AudioSource source) {
		if (source == null) return;
		source.Stop();
	}
}