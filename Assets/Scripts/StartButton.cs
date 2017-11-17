using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour {
	public Texture2D highlightT;

	public Sprite highlight;
	public Sprite normal;

	public Image image;

	void Start() {
		image = GetComponent<Image>();
		normal = image.sprite;
		highlight = Sprite.Create(highlightT, new Rect(0, 0, highlightT.width, highlightT.height), new Vector2(.5f, .5f));
	}


	public void Highlight() {
		image.sprite = highlight;
	}

	public void Dehighlight() {
		image.sprite = normal;
	}

	public void Click() {
		SceneManager.LoadScene("main");
	}
}
