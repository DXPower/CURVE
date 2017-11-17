using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour {
	public static Board self;
	public static LevelReader levelReader;
	public static GUIManager guiManager;
	public static CutsceneManager cutsceneManager;

	#region Events
	public delegate void DoorCallback(Door.DoorColor activate);
	public delegate void ButtonCallback();
	public delegate void ButtonDepressCallback();
	public delegate void ButtonCheckCallback();
	public delegate void TutorialNextStageCallback(short stage);

	public static event TutorialNextStageCallback OnNextTutorialStage;
	public static event DoorCallback OnSwitchDoors;
	public static event ButtonCallback OnButtonPress;
	public static event ButtonDepressCallback OnButtonDepress;
	public static event ButtonCheckCallback ButtonCheck;
	#endregion

	public static Coroutine tipCoroutine = null;

	public static GameObject board;
	public GameObject boardLocal;
	public GameObject objectsParent;
	public GameObject turnsText;
	public GameObject levelText;

	public static OffsetData currentOffset = new OffsetData(0, 0);
	public static LevelSizeData boardSize = new LevelSizeData(0, 0);
	public static LevelSizeData roomSize = new LevelSizeData(0, 0);
	public static int buttonsPressed = 0;
	public static int currentLevel = 0; // The game starts at this level
	public static int goalsActivated = 0;
	public static int totalGoals = 0;
	public static int currentRoom = 0;
	public static int totalRooms = 4;

	public static bool canQuit = false;

	#region Camera Variables
	public static Vector3 centroid = Vector3.zero;
	public static Camera camera;

	public float cameraRotateSpeed;
	public float cameraRotateDur;
	public float cameraResetSpeed;
	public float cameraResetDur;
	public float zoomOutSpeed;
	public float zoomOutDur;
	public float zoomOutTarget;
	public float zoomInTarget;

	public static int currentRotation = 0; // This will be in multiples of 90s
	public static int tutorialRotations = 0;
	public int rotDir = 0; // 0 = not rotating, -1 = rotating ccw, 1 = rotating cw

	public static bool controlsEnabled = false;
	public static bool waitForLoad = false;
	public static bool loadingNextLevel = false;
	public static bool gameAlreadyLoaded = false;
	public static bool inCutscene = false;
	public bool isRotating = false;
	public bool isZoomedOut = false;

	public List<int> rotQueue = new List<int>();

	private Coroutine cameraCoroutine;
	private Coroutine zoomCoroutine;
	#endregion

	public List<Rigidbody> objects; 
	public static Door.DoorColor currentDoorOpen = Door.DoorColor.red;

	void Start() {
		self = this;
		board = boardLocal;

		// Put placeholder event listeners to fix a bug with the events being cleaned by garbage collection
		OnSwitchDoors += new DoorCallback(SwitchDoorsFix);
		OnButtonPress += new ButtonCallback(ButtonFix);
		OnButtonDepress += new ButtonDepressCallback(ButtonFix);
		ButtonCheck += new ButtonCheckCallback(ButtonFix);
		OnNextTutorialStage += new TutorialNextStageCallback(NextTutorialStage);

		// Initialize some variables
		levelReader = GetComponent<LevelReader>();
		camera = GetComponent<Camera>();
		guiManager = GetComponent<GUIManager>();
		cutsceneManager = GetComponent<CutsceneManager>();
		zoomInTarget = camera.orthographicSize;
		Stats.turnsText = turnsText.GetComponent<Text>();
		Stats.levelText = levelText.GetComponent<Text>();
		
		Sounds.LoadAllSounds();
		Tutorial.HideTutorialText();

		guiManager.NextLevel(currentLevel);
		waitForLoad = true;

	}

	public static IEnumerator ResetTimer() {
		while (true) {
			yield return new WaitForSeconds(18f);
			ResetTip.self.StartCoroutine(ResetTip.Show());
		}
	}

	// Update is called once per frame
	void Update() {
		if (canQuit && Input.anyKeyDown) Application.Quit();
		if (controlsEnabled && !inCutscene) { 
			if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow)) && rotQueue.Count < 4) {
				if (Tutorial.currentStage == 0) {
					Tutorial.ConditionMet();
				}

				rotQueue.Add(-1);
			}

			if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow)) && rotQueue.Count < 4) {
				if (Tutorial.currentStage == 0) Tutorial.ConditionMet();

				rotQueue.Add(1);
			}
			/*
			if (Input.GetKeyDown(KeyCode.W) && !isZoomedOut) {
				if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
				zoomCoroutine = StartCoroutine(ZoomCamera(true));
			}
			
			if (Input.GetKeyUp(KeyCode.W) && isZoomedOut) {
				if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
				zoomCoroutine = StartCoroutine(ZoomCamera(false));
			}
			*/
			if (Input.GetKeyDown(KeyCode.Space)) {
				if (Tutorial.currentStage == 2) Tutorial.ConditionMet();
				if (OnSwitchDoors.GetInvocationList().Length > 1) Sounds.PlaySound(ref Sounds.doorSound);
				currentDoorOpen = currentDoorOpen == Door.DoorColor.red ? Door.DoorColor.blue : Door.DoorColor.red;
				OnSwitchDoors(currentDoorOpen);
			}

			if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.DownArrow)) {
				if (!isRotating) {
					foreach (Rigidbody o in objects) {
						o.constraints = RigidbodyConstraints.FreezeAll;
					}

					Stats.ResetTurns();
					Reset(ref Sounds.resetSound);
				}
			}
		}
		// Queues up successive inputs- allows the controls to feel 'smoother'
		if (rotQueue.Count > 0 && !isRotating) {
			foreach (Rigidbody o in objects) {
				o.constraints = RigidbodyConstraints.FreezeAll;
			}

			Rotate(Vector3.forward, rotQueue[0] * 90);
		}

		if (waitForLoad && GUIManager.doneFading) {
			waitForLoad = false;
			GUIManager.doneFading = false;
			LoadNextLevel();
		}
	}

	// Tutorial goes in stages. 
	public static void NextTutorialStage(short stage) {
		switch (stage) {
			case 1: // After the first condition is met, it moves to stage 1, which is just a timed message
				Tutorial.ShowTutorialText("Gravity will always point down");
				Tutorial.SetConditions(-1);
				Tutorial.self.StartCoroutine(Tutorial.DelayedAdvancement(3.5f));
				break;
			case 2: // This is a condition based one, it will only move to 3 after the doors are opened 2 times
				Tutorial.SetConditions(1);
				Tutorial.ShowTutorialText("Press SPACE to open and close doors");
				break;
			case 3: // This is timed and will dissapear after a few seconds
				Tutorial.ShowTutorialText("Get the cube to the goal to advance");
				Tutorial.SetConditions(-1);
				Tutorial.self.StartCoroutine(Tutorial.DelayedAdvancement(3.5f));
				break;
			case 4:
				Tutorial.HideTutorialText();
				Tutorial.SetConditions(-1);
				break;
			case 5:
				Tutorial.ShowTutorialText("Each goal needs to have 1 block in it to advance");
				Tutorial.SetConditions(-1);
				Tutorial.self.StartCoroutine(Tutorial.DelayedAdvancement(5f));
				break;
			case 6:
				Tutorial.ShowTutorialText("Press R or Down to reset if you get stuck");
				Tutorial.SetConditions(-1);
				Tutorial.self.StartCoroutine(Tutorial.DelayedAdvancement(5f));
				break;
			case 7:
				Tutorial.HideTutorialText();
				Tutorial.SetConditions(-1);
				break;
			case 8: 
				Tutorial.ShowTutorialText("Buttons can be used to power doors");
				Tutorial.SetConditions(-1);
				Tutorial.self.StartCoroutine(Tutorial.DelayedAdvancement(3.5f));
				break;
			case 9:
				Tutorial.SetConditions(-1);
				Tutorial.HideTutorialText();
				break;
			default:
				Tutorial.SetConditions(-1);
				Tutorial.HideTutorialText();
				break;
		}
	}

	public static void TriggerNextTutorialStage(short stage) {
		OnNextTutorialStage(stage);
	}

	public static void LoadNextLevel() {
		
		if (gameAlreadyLoaded) {
			Debug.Log("Cleanup");
			foreach (Transform c in board.transform) {
				Destroy(c.gameObject);
			}

			foreach (Rigidbody r in self.objects) {
				Destroy(r.gameObject);
			}

			self.objects.Clear();

			// The following removes all the subscribers from the various events
			foreach (DoorCallback d in OnSwitchDoors.GetInvocationList()) {
				OnSwitchDoors -= (DoorCallback) d;
			}

			foreach (ButtonCallback d in OnButtonPress.GetInvocationList()) {
				OnButtonPress -= (ButtonCallback) d;
			}

			foreach (ButtonDepressCallback d in OnButtonDepress.GetInvocationList()) {
				OnButtonDepress -= (ButtonDepressCallback) d;
			}

			foreach (ButtonCheckCallback d in ButtonCheck.GetInvocationList()) {
				ButtonCheck -= (ButtonCheckCallback) d;
			}

			// Then adds the 'fixes'
			OnSwitchDoors += new DoorCallback(self.SwitchDoorsFix);
			OnButtonPress += new ButtonCallback(self.ButtonFix);
			OnButtonDepress += new ButtonDepressCallback(self.ButtonFix);
			ButtonCheck += new ButtonCheckCallback(self.ButtonFix);

			totalGoals = 0;
			goalsActivated = 0;
			Stats.NextLevel();
			self.StopCoroutine(tipCoroutine);
			ResetTip.Hide();
			levelReader.BuildLevel(++currentLevel, new LevelSizeData(32, 32), new OffsetData(0, 0));
		} else {
			levelReader.BuildLevel(currentLevel, new LevelSizeData(32, 32), new OffsetData(0, 0));
			gameAlreadyLoaded = true;
		}
	}

	public static void NextRoom() {
		OffsetData deltaOffset = new OffsetData(0, 0);

		switch (currentRotation) {
			case 0:
				deltaOffset.y = 1;
				break;
			case 90:
				deltaOffset.x = 1;
				break;
			case 180:
				deltaOffset.y = -1;
				break;
			case 270:
				deltaOffset.x = -1;
				break;
			case 360:
				deltaOffset.y = 1;
				break;
		}

		currentOffset += deltaOffset;
		centroid = CalculateCentroid(roomSize, currentOffset);
		SetCameraPosition(centroid);
	}

	public static Vector3 CalculateCentroid(LevelSizeData levelSize, OffsetData offset) {
		return new Vector3(((levelSize.x - 1) / 2) + (offset.x * levelSize.x), ((levelSize.y - 1) / 2) + (offset.y * levelSize.y), 0);
	}

	public static void GoalReached() {
		if (inCutscene) return;
		guiManager.blocksText.text = "Squares in: " + ++goalsActivated + "/" + totalGoals;
		if (goalsActivated < totalGoals) return;
		
		if (Tutorial.currentStage < 4) Tutorial.SkipToStage(4); // If the player goes faster than the beginning tutorial, skip ahead

		self.Reset(ref Sounds.transitionSound);
		if (!inCutscene) CutsceneManager.PlayCutscene(currentLevel);
	}

	public static void PressButtons() {
		buttonsPressed++;
		OnButtonPress();
	}

	public static void DepressButtons() {
		if (--buttonsPressed == 0) OnButtonDepress();
		else if (buttonsPressed < 0) buttonsPressed = 0;
	}

	void SwitchDoorsFix(Door.DoorColor d) {
		return;
	}

	void ButtonFix() {
		return;
	}

	public void Reset(ref AudioClip sound) {
		float a = -(transform.rotation.eulerAngles.z);
		if (a >= 0) a += 360;
		else a -= 360;

		Physics.gravity = new Vector3(0, -9.81f, 0);
		Sounds.PlaySound(ref sound);
		StopCoroutine(tipCoroutine);
		ResetTip.Show();
		tipCoroutine = StartCoroutine(ResetTimer());
		StartCoroutine(SmoothResetCamera(a));
	}

	private void Rotate(Vector3 axis, int a) {
		int s = currentRotation + a;
		Vector3 g = Vector3.zero;

		// Clamp the rotation to 0-360
		if (s < 0) s = 360 + s;
		if (s > 360) s -= 360;
		currentRotation = s;

		// Decide the direction of gravity based on the new rotation direction
		switch (s) {
			case 0:
				g = new Vector3(0, -9.81f, 0);
				break;
			case 90:
				g = new Vector3(9.81f, 0, 0);
				break;
			case 180:
				g = new Vector3(0, 9.81f, 0);
				break;
			case 270:
				g = new Vector3(-9.81f, 0, 0);
				break;
			default:
				g = new Vector3(0, -9.81f, 0);
				break;
		}

		// If the player presses the same direction multiple times it will just keep rotating in that direction. Resets if the opp. dir. is pressed.
		StartCoroutine(SmoothRotateCamera(a));
		Stats.turns++;
		Physics.gravity = g;
		Sounds.PlaySound(ref Sounds.turningSound);
	}

	// Coroutine to handle camera rotation. This offloads it to another thread to keep it smooth and consistent.
	private IEnumerator SmoothRotateCamera(float angle) {
		float o = transform.rotation.eulerAngles.z;
		isRotating = true;

		for (float i = 0; i <= cameraRotateDur; i++) {
			transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x
				, transform.rotation.eulerAngles.y
				, EaseInOutExpo(i, o, angle, cameraRotateDur)));
			i++; // I just discovered that I'm incrementing twice... but it works at the speed I want so I'm not asking any questions.
			yield return new WaitForSeconds(cameraRotateSpeed);
		}

		// Fixes floating point errors causing a slight tilt to the level
		transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, currentRotation));
		rotQueue.RemoveAt(0);
		isRotating = false;

		if (rotQueue.Count == 0) {
			foreach (Rigidbody c in objects) {
				c.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
			}

			ButtonCheck();
		}
	}

	public static void ShakeCamera(float amount, float dur) {

	}

	private IEnumerator SmoothResetCamera(float angle) {
		float o = transform.rotation.eulerAngles.z, half = cameraResetDur / 2f;
		bool hasReset = false;

		isRotating = true;

		for (float i = 0; i <= cameraResetDur; i++) {
			if (!hasReset && i >= half) {
				LevelReader.ResetPositions();
			}
			transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x
				, transform.rotation.eulerAngles.y
				, EaseInOutQuad(i, o, angle, cameraResetDur)));
			i++;
			yield return new WaitForSeconds(cameraResetSpeed);
		}

		// Fixes floating point errors causing a slight tilt to the level
		currentRotation = 0;
		transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, currentRotation));
		isRotating = false;

		if (rotQueue.Count == 0) {
			foreach (Rigidbody c in objects) {
				c.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
			}
			
			ButtonCheck();
		}

		currentDoorOpen = Door.DoorColor.red;
		OnSwitchDoors(Door.DoorColor.red);
	}

	public IEnumerator ZoomCamera(bool zoomOut) {
		// Zoom the camera out if zoomOut is true, zoom in if false
		if (zoomOut) {
			isZoomedOut = true;
			float size = camera.orthographicSize, diff = zoomOutTarget - size;
			for (float i = 0; i < zoomOutDur; i++) {
				camera.orthographicSize = EaseInOutExpo(i, size, diff, zoomOutDur);

				yield return new WaitForSeconds(zoomOutSpeed);
			}
		} else {
			isZoomedOut = false;
			float size = camera.orthographicSize, diff = zoomInTarget - size;
			for (float i = 0; i < zoomOutDur; i++) {
				camera.orthographicSize = EaseInOutExpo(i, size, diff, zoomOutDur);
				yield return new WaitForSeconds(zoomOutSpeed);
			}
		}
	}

	// A nice smooth easing equation. t is current time, b is from, c is change in value, d is duration
	private float EaseInOutExpo(float t, float b, float c, float d) {
		t /= d / 2;
		if (t < 1) return c / 2 * Mathf.Pow(2, 10 * (t - 1)) + b;
		t--;
		return c / 2 * (-Mathf.Pow(2, -10 * t) + 2) + b;
	}

	private float EaseInOutQuad(float t, float b, float c, float d) {
		t /= d / 2;
		if (t < 1) return c / 2 * t * t + b;
		t--;
		return -c / 2 * (t * (t - 2) - 1) + b;
	}

	public static void SetCameraPosition(Vector3 p) {
		self.gameObject.transform.position = new Vector3(p.x, p.y, self.gameObject.transform.position.z);
	}
}

public struct OffsetData {
	public int x;
	public int y;

	public OffsetData(int x, int y) {
		this.x = x;
		this.y = y;
	}

	public static OffsetData operator+ (OffsetData a, OffsetData b) {
		return new OffsetData(a.x + b.x, a.y + b.y);
	}
}

public struct LevelSizeData {
	public ushort x;
	public ushort y;

	public LevelSizeData(ushort x, ushort y) {
		this.x = x;
		this.y = y;
	}
}