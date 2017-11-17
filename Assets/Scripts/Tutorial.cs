using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
	public static Tutorial self;

	private static GameObject tutorial;
	private static Text tutorialText;

	private static short conditionsMet = 0;
	private static short conditionsNeeded = -1;
	public static short currentStage = 0;

	private static bool alreadyAdvancing = false;
	private static bool isSkipping = false;

	void Start() {
		self = this;
		tutorial = gameObject;
		tutorialText = tutorial.GetComponentInChildren<Text>();
	}

	public static void SetConditions(short num) {
		conditionsMet = 0;
		conditionsNeeded = num;
	}

	public static void ConditionMet() {
		if (alreadyAdvancing) return;
		if (conditionsMet < conditionsNeeded) conditionsMet++;
		if (conditionsNeeded != -1 && conditionsMet == conditionsNeeded) {
			self.StartCoroutine(DelayedAdvancement(.75f));
		}
	}

	public static void SkipToStage(short stage) {
		currentStage = stage;
		isSkipping = true;
		Board.TriggerNextTutorialStage(currentStage);
	}

	public static IEnumerator DelayedAdvancement(float delay) {
		isSkipping = false;
		alreadyAdvancing = true;
		yield return new WaitForSeconds(delay);

		if (!isSkipping) {
			Board.TriggerNextTutorialStage(++currentStage);
			alreadyAdvancing = false;
		}
	}

	public static void ConditionUnmet() {
		if (conditionsMet > 0) conditionsMet--;
	}

	public static void ShowTutorialText(string s) {
		tutorial.GetComponent<Image>().enabled = true;
		tutorialText.text = s;
		tutorialText.enabled = true;
	}

	public static void HideTutorialText() {
		tutorial.GetComponent<Image>().enabled = false;
		tutorialText.enabled = false;
	}
}

