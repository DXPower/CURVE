using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {
	public GameObject blackBackground;
	public GameObject level;
	public GameObject anyKey;
	public GameObject turns;
	public GameObject currentLevel;
	public GameObject cutsceneText;
	public GameObject breakdown;
	public GameObject total;
	public GameObject quit;
	public GameObject stats;
	public GameObject blocks;
	public Image black;
	public Text levelText;
	public Text anyKeyText;
	public Text turnsText;
	public Text blocksText;

	public float fadeRate;

	public bool canPress = false;
	public static bool doneFading = false;

	public Coroutine cutsceneTextCoroutine = null;

	const string keyText = "Press any key to continue...";

	void Start() {
		cutsceneText = GameObject.Find("CutsceneText");
		cutsceneText.SetActive(false);
		blocksText = blocks.GetComponent<Text>();
	}

	void OnGUI() {
		if (Input.anyKeyDown && canPress) {
			StartCoroutine(FadeOut());
		}
	}

	public void NextLevel(int level) {
		black = blackBackground.GetComponent<Image>();
		levelText = this.level.GetComponent<Text>();
		anyKeyText = anyKey.GetComponent<Text>();

		if (level == 0) levelText.text = "Tutorial";
		else {
			if (level == 0) levelText.text = "Tutorial";
			else levelText.text = "Level " + level;
		}
		anyKeyText.text = "Loading...";
		Board.controlsEnabled = false;

		StartCoroutine(FadeIn());
	}

	IEnumerator FadeIn() {
		Color b = black.color = new Color(black.color.r, black.color.g, black.color.b, 0);
		Color l = levelText.color = new Color(levelText.color.r, levelText.color.g, levelText.color.b, 0);
		Color a = anyKeyText.color = new Color(anyKeyText.color.r, anyKeyText.color.g, anyKeyText.color.b, 0);
		currentLevel.SetActive(false);
		blocks.SetActive(false);
		turns.SetActive(false);
		ResetTip.Hide();
		if (Board.tipCoroutine != null) Board.self.StopCoroutine(Board.tipCoroutine);
		Board.controlsEnabled = false;

		for (float i = 0; i < 1; i += fadeRate) {
			black.color = Color.Lerp(b, new Color(black.color.r, black.color.g, black.color.b, 1f), i);
			levelText.color = Color.Lerp(l, new Color(levelText.color.r, levelText.color.g, levelText.color.b, 1f), i);
			anyKeyText.color = Color.Lerp(a, new Color(anyKeyText.color.r, anyKeyText.color.g, anyKeyText.color.b, 1f), i);
			yield return new WaitForSeconds(0.01f);
		}

		if (CutsceneManager.faceCoroutine != null) CutsceneManager.self.StopCoroutine(CutsceneManager.faceCoroutine);
		if (CutsceneManager.shakeCoroutine != null) CutsceneManager.self.StopCoroutine(CutsceneManager.shakeCoroutine);

		try {
			CutsceneManager.self.StopCoroutine(CutsceneManager.self.moveAlienCoroutine);
			Destroy(CutsceneManager.currentCutscene.alien);
		} catch (System.Exception e) { }
		CutsceneManager.self.background.SetActive(false);

		doneFading = true;
	}

	public IEnumerator FadeInStats() {
		Text breakdownText = this.breakdown.GetComponent<Text>();
		Text totalText = this.total.GetComponent<Text>();
		Text quitText = this.quit.GetComponent<Text>();
		Text statsText = this.stats.GetComponent<Text>();
		Color b = black.color = new Color(black.color.r, black.color.g, black.color.b, 0);
		Color breakdown = breakdownText.color = new Color(breakdownText.color.r, breakdownText.color.g, breakdownText.color.b, 0);
		Color total = totalText.color = new Color(totalText.color.r, totalText.color.g, totalText.color.b, 0);
		Color quit = quitText.color = new Color(quitText.color.r, quitText.color.g, quitText.color.b, 0);
		Color stats = statsText.color = new Color(statsText.color.r, statsText.color.g, statsText.color.b, 0);
		string breakdownString = "";
		string totalString = "";
		Stats.BuildFinalStats(out breakdownString, out totalString);

		breakdownText.text = breakdownString;
		totalText.text = totalString;

		for (float i = 0; i < 1; i += fadeRate) {
			black.color = Color.Lerp(b, new Color(black.color.r, black.color.g, black.color.b, 1f), i);
			totalText.color = Color.Lerp(total, new Color(totalText.color.r, totalText.color.g, totalText.color.b, 1f), i);
			breakdownText.color = Color.Lerp(breakdown, new Color(breakdownText.color.r, breakdownText.color.g, breakdownText.color.b, 1f), i);
			quitText.color = Color.Lerp(quit, new Color(quitText.color.r, quitText.color.g, quitText.color.b, 1f), i);
			statsText.color = Color.Lerp(stats, new Color(statsText.color.r, statsText.color.g, statsText.color.b, 1f), i);
			yield return new WaitForSeconds(0.01f);
		}

		Board.canQuit = true;
	}

	public IEnumerator PressAnyKey() {
		yield return new WaitForSeconds(3f);
		canPress = true;
		anyKeyText.text = keyText;

		Color a = anyKeyText.color = new Color(anyKeyText.color.r, anyKeyText.color.g, anyKeyText.color.b, 0);

		for (float i = 0; i < 1; i += fadeRate) {
			anyKeyText.color = Color.Lerp(a, new Color(anyKeyText.color.r, anyKeyText.color.g, anyKeyText.color.b, 255), i);
			yield return new WaitForSeconds(0.01f);
		}
	}

	IEnumerator FadeOut() {
		Color b = black.color = new Color(black.color.r, black.color.g, black.color.b, 1);
		Color l = levelText.color = new Color(levelText.color.r, levelText.color.g, levelText.color.b, 1);
		Color a = anyKeyText.color = new Color(anyKeyText.color.r, anyKeyText.color.g, anyKeyText.color.b, 1);
		canPress = false;

		for (float i = 0; i < 1; i += fadeRate) {
			black.color = Color.Lerp(b, new Color(black.color.r, black.color.g, black.color.b, 0), i);
			levelText.color = Color.Lerp(l, new Color(levelText.color.r, levelText.color.g, levelText.color.b, 0), i);
			anyKeyText.color = Color.Lerp(a, new Color(anyKeyText.color.r, anyKeyText.color.g, anyKeyText.color.b, 0), i);
			yield return new WaitForSeconds(0.01f);
		}

		black.color = new Color(0, 0, 0, 0);
		levelText.color = new Color(1, 1, 1, 0);
		anyKeyText.color = new Color(1, 1, 1, 0);
		Board.controlsEnabled = true;

		Board.tipCoroutine = Board.self.StartCoroutine(Board.ResetTimer());
		if (Board.currentLevel == 4) Board.totalGoals = 1; // Fixes a weird bug that I cannot track down
		blocksText.text = "Squares in: 0/" + Board.totalGoals;
		currentLevel.SetActive(true);
		blocks.SetActive(true);
		turns.SetActive(true);

		if (Tutorial.currentStage == 0) {
			yield return new WaitForSeconds(1.5f);
			Tutorial.SetConditions(2);
			Tutorial.ShowTutorialText("Use Q and E or Left and Right to rotate the screen ");
		} else if (Board.currentLevel == 3) {
			// This is the first level that has buttons, so show the button text.
			Tutorial.self.StartCoroutine(Tutorial.DelayedAdvancement(.75f));
		} else if (Board.currentLevel == 2) {
			// This shows the multiple goals message
			Tutorial.self.StartCoroutine(Tutorial.DelayedAdvancement(.75f));
		}
	}

	public void ShowCutsceneText(string text, float duration) {
		if (cutsceneTextCoroutine != null) StopCoroutine(cutsceneTextCoroutine);
		StartCoroutine(ShowCutsceneTextCoroutine(text, duration));
	}

	IEnumerator ShowCutsceneTextCoroutine(string text, float dur) {
		cutsceneText.GetComponentInChildren<Text>().text = text;
		cutsceneText.SetActive(true);
		yield return new WaitForSecondsRealtime(dur);
		cutsceneText.SetActive(false);
	}
}
