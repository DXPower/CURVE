using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Explosion : MonoBehaviour {
	private static Sprite[] sprites;

	public bool isBIG = false; // IS IT B I G?

	public IEnumerator AnimateExplosion(float duration) {
		Image image = gameObject.GetComponent<Image>();

		if (sprites == null) { // We don't need to load the explosions multiple times
			Texture2D[] images = Resources.LoadAll<Texture2D>("Explosion/");
			sprites = new Sprite[images.Length];
			for (int i = 0; i < images.Length; i++) {
				sprites[i] = Sprite.Create(images[i], new Rect(0, 0, images[i].width, images[i].height), new Vector2(.5f, .5f));
				Resources.UnloadAsset(images[i]);
			}
		}

		if (!isBIG) Sounds.PlaySound(ref Sounds.smallExplosionSound);
		else {
			Sounds.audioSource.Stop();
			Sounds.PlaySound(ref Sounds.bigExplosionSound);
		}

		for (int i = 0; i < sprites.Length; i++) {
			image.sprite = sprites[i];
			if (i == 0) image.color = Color.white;
			yield return new WaitForSecondsRealtime(duration / sprites.Length);
		}

		Destroy(gameObject);
	}
}
