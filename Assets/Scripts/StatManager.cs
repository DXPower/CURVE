using UnityEngine.UI;
using System;

public static class Stats {
	private static int _turns;
	private static int _turnsBefore;
	private static int _doors;
	private static int _buttons;
	private static int _resets;

	private const double par = 34;

	public static int turns {
		get { return _turns; }
		set {
			_turns = value;
			UpdateText(ref turnsText, _turns);
		}
	}

	public static int doors {
		get { return _doors; }
		set {
			_doors = value;
		}
	}

	public static int buttons {
		get { return _buttons;  }
		set {
			_buttons = value;
		}
	}

	public static int resets {
		get { return _resets; }
		set {
			_resets = value;
		}
	}

	// These are set in Board.Start()
	public static Text turnsText; 
	public static Text levelText;

	public static void BuildFinalStats(out string breakdown, out string total) {
		int bonus = (int) Math.Floor((double) UnityEngine.Random.Range(3, 10));
		double curve = Math.Sqrt(par / ((double) turns));
		int efficiency = (int) (curve * 100);
		
		breakdown = "Turns:    " + turns + "\nResets:    " + resets + "\nButtons  Pressed:    " + buttons + "\nEfficiency:    " + efficiency + "\nBonus:    x" + bonus;
		total = "Total:    " + ((turns * 9 + resets * 3 + buttons * 12 + efficiency * 8) * bonus);	
	}

	public static void ResetTurns() {
		turns = _turnsBefore;
		resets++;
	}

	public static void NextLevel() {
		_turnsBefore = _turns;
	}

	public static void UpdateText(ref Text text, int value) {
		text.text = text.text.Split(' ')[0] + " " + value; // Splits the string to get the text before the number, then adds the new stat number
	}
}