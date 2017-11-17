using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.UI;

using UnityEngine;

public class CutsceneManager : MonoBehaviour {
	public static CutsceneManager self;

	public static List<CutsceneData> cutscenes = new List<CutsceneData>();

	public static CutsceneData currentCutscene;

	public GameObject alien;
	public GameObject background;
	public GameObject black;
	public GameObject zeekPrefab;
	public GameObject smallExplosionPrefab;
	public GameObject explosionPrefab;

	public Coroutine moveAlienCoroutine = null;
	public static Coroutine faceCoroutine = null;
	public static Coroutine shakeCoroutine = null;
	public static Coroutine explosionCoroutine = null;
	public enum EventType {
		ShowText,
		CameraShake,
		FaceChange,
		AdvanceText,
		MoveAlien,
		SetAlienPos,
		PlaySound,
		Zeek,
		ShakeAlien,
		Explode,
		SmallExplode,
		StopShaking
	}

	// Use this for initialization
	void Start () {
		self = this;
		background.SetActive(false);
		black.SetActive(false);
		LoadCutscenes();
	}

	private void LoadCutscenes() {
		var events = new List<CutsceneEvent>();

		TextAsset[] jsons = Resources.LoadAll<TextAsset>("Cutscenes/");

		for(int i = 0; i <= 5; i++) {
			CutsceneJSON imported;
			TextAsset json = jsons[i];
			Texture2D bg = Resources.Load<Texture2D>("Cutscenes/" + i.ToString() + "image");
			imported = JsonUtility.FromJson<CutsceneJSON>(json.text);
			Resources.UnloadAsset(json);

			Debug.Log("Index: " + i);
			currentCutscene = imported.CreateCutscene();
			if (bg != null) currentCutscene.background = bg;
			cutscenes.Add(currentCutscene);
		}
	}

	private static Vector3 alienOriginalPosition;
	private static Vector3 backgroundOriginalPosition;
	private static RectTransform alienRT;
	private static RectTransform backgroundRT;
	private static float shakeDecay;
	private static float shakeIntensity;

	void Update() {
		if (shakeIntensity > 0) {
			alienRT.position = alienOriginalPosition + UnityEngine.Random.insideUnitSphere * shakeIntensity;
			backgroundRT.position = backgroundOriginalPosition + UnityEngine.Random.insideUnitSphere * shakeIntensity;
			shakeIntensity -= shakeDecay;
		}
	}

	public static void CameraShake(float intensity, float decay) {
		alienRT = currentCutscene.alien.GetComponent<RectTransform>();
		backgroundRT = self.background.GetComponent<RectTransform>();
		alienOriginalPosition = alienRT.position;
		backgroundOriginalPosition = backgroundRT.position;
		shakeIntensity = intensity;
		shakeDecay = decay;
	}

	public static void PlayCutscene(int index) {
		if (index >= cutscenes.Count) return;
		Board.inCutscene = true;
		currentCutscene = cutscenes[index];
		self.StartCoroutine(self.DoCutscene(currentCutscene.events, currentCutscene.duration));
	}

	public static IEnumerator SmallExplosions(float period, float explosionDur) {
		while (true) {
			Explosion ex = Instantiate<GameObject>(self.smallExplosionPrefab, currentCutscene.alien.transform.position + UnityEngine.Random.insideUnitSphere * 50, new Quaternion(), GameObject.Find("CanvasTop").transform).GetComponent<Explosion>();
			ex.StartCoroutine(ex.AnimateExplosion(explosionDur));
			yield return new WaitForSecondsRealtime(period);
		}
	}

	public IEnumerator DoCutscene(List<CutsceneEvent> events, float duration) {
		Image bl = self.black.GetComponent<Image>();
		Color from = new Color(bl.color.r, bl.color.g, bl.color.b, 0);
		Color target = new Color(bl.color.r, bl.color.g, bl.color.b, 1f);
		self.black.SetActive(true);
		Board.guiManager.currentLevel.SetActive(false);
		Board.guiManager.blocks.SetActive(false);
		Board.guiManager.turns.SetActive(false);
		ResetTip.Hide();

		for (float i = 0; i < 1; i += 0.03f) {
			bl.color = Color.Lerp(from, target, i);
			yield return new WaitForSeconds(0.01f);
		}

		currentCutscene.Init(ref self.alien, ref self.background);

		for (float i = 0; i < 1; i += 0.03f) {
			bl.color = Color.Lerp(target, from, i);
			yield return new WaitForSeconds(0.01f);
		}

		int index = 0;
		float timeToWaitFor = events[index].timing;
		
		for (float t = 0; t <= duration; t += 0.1f) {
			if (timeToWaitFor != -1 && t >= timeToWaitFor) {
				events[index].DoEvent();

				if (index < (events.Count - 1)) {
					index++;
					timeToWaitFor = events[index].timing;
				} else {
					timeToWaitFor = -1;
				}
			}

			yield return new WaitForSecondsRealtime(0.1f);
		}

		if (Board.currentLevel + 1 < 6) {
			Board.guiManager.NextLevel(Board.currentLevel + 1);
			Board.waitForLoad = true;
			Board.inCutscene = false;
		} else {
			Board.guiManager.StartCoroutine(Board.guiManager.FadeInStats());
		}
	}

	public static IEnumerator AlienFace(bool mad) {
		string path = "";

		if (mad) path = "AlienFaces/Mad/";
		else path = "AlienFaces/Normal/";

		Debug.Log(path);

		Texture2D[] faceTextures = Resources.LoadAll<Texture2D>(path);
		Sprite[] faces = new Sprite[faceTextures.Length];
		Image face = currentCutscene.alien.transform.GetChild(0).GetComponent<Image>();

		for (int i = 0; i < faceTextures.Length; i++) {
			faces[i] = Sprite.Create(faceTextures[i], new Rect(0, 0, faceTextures[i].width, faceTextures[i].height), new Vector2(.5f, .5f));
			Resources.UnloadAsset(faceTextures[i]);
		}

		faceTextures = null;

		while (face != null) {
			face.sprite = faces[Mathf.FloorToInt(UnityEngine.Random.Range(0, faces.Length))];
			yield return new WaitForSecondsRealtime(0.15f);
		}
	}

	public static IEnumerator MoveAlien(Vector3 from, Vector3 to, float dur) {
		to = new Vector3(Screen.width * to.x, Screen.height * to.y, 0);

		for (float i = 0; i < dur; i += 0.01f) {
			Vector3 pos = Vector3.Lerp(from, to, i / dur);
			currentCutscene.alien.transform.position = pos;
			yield return new WaitForSecondsRealtime(0.01f);
		}
	}
	
	public static IEnumerator ShakeAlien(float amount) {
		RectTransform alienRT = currentCutscene.alien.GetComponent<RectTransform>();
		Vector3 originalPos = alienRT.position;

		while (true) { 
			alienRT.position = originalPos + UnityEngine.Random.insideUnitSphere * amount;
			yield return new WaitForSeconds(0.01f);
		}
	}
	
	[Serializable]
	public class CutsceneData : MonoBehaviour {
		public List<CutsceneEvent> events;

		public GameObject alien;
		public Texture2D background;

		public float duration = 0;

		public CutsceneData(List<CutsceneEvent> events, float duration) {
			this.events = new List<CutsceneEvent>(events);
			this.duration = duration;
		}

		public void Init(ref GameObject alien, ref GameObject background) {
			Transform canvas = GameObject.Find("CutsceneCanvas").transform;
			background.SetActive(true);
			background.GetComponent<Image>().sprite = Sprite.Create(this.background, new Rect(0, 0, this.background.width, this.background.height), new Vector2(.5f, .5f));
			this.alien = Instantiate<GameObject>(alien, canvas);
			//this.background = Instantiate<GameObject>(background);
		}
	}

	[Serializable]
	public struct CutsceneEvent {
		// ShowText: data[0] = text (string); data[1] = duration (float);
		// CameraShake: data[0] = amount (float); data[1] = decay (float);
		// FaceChange: data[0] = face (Texture2D);
		// MoveAlien: data[0] = pos (screen pos %) (2 floats); data[1] =  duration (float);
		// SetAlienPos: data[0] = pos (screen pos %) (2 floats) 
		// PlaySound: data[0] = sound (string);
		public List<object> data;
		public float timing;
		public readonly EventType type;

		public CutsceneEvent(object[] eventData, float timing, EventType type) {
			data = new List<object>(eventData);
			this.timing = timing;
			this.type = type;
		}

		public void DoEvent() {
			try {
				switch (type) {
					case EventType.ShowText:
						Board.guiManager.ShowCutsceneText((string) data[0], (float) data[1]);
						if (faceCoroutine == null && (bool) data[2] == true) self.StartCoroutine(AlienFace(false));
						break;
					case EventType.CameraShake:
						CutsceneManager.CameraShake((float) data[0], (float) data[1]);
						break;
					case EventType.MoveAlien:
						if (self.moveAlienCoroutine != null) self.StopCoroutine(self.moveAlienCoroutine);
						self.moveAlienCoroutine = CutsceneManager.self.StartCoroutine(MoveAlien(CutsceneManager.currentCutscene.alien.GetComponent<RectTransform>().position, (Vector3) data[0], (float) data[1]));
						break;
					case EventType.SetAlienPos:
						Vector3 dataRaw = (Vector3) data[0];
						Vector3 pos = new Vector3(dataRaw.x * Screen.width, dataRaw.y * Screen.height, 0);
						CutsceneManager.currentCutscene.alien.GetComponent<RectTransform>().position = pos;
						break;
					case EventType.PlaySound:
						AudioClip sound = (AudioClip) (typeof(Sounds)).GetField((string) data[0]).GetValue(null);
						Sounds.PlaySound(ref sound);
						break;
					case EventType.Zeek:
						Debug.Log("ZEEK");
						Instantiate(self.zeekPrefab, GameObject.Find("CanvasTop").transform); // ZEEK
						break;
					case EventType.ShakeAlien:
						if (shakeCoroutine != null) CutsceneManager.self.StopCoroutine(shakeCoroutine);
						shakeCoroutine = CutsceneManager.self.StartCoroutine(ShakeAlien((float) data[0]));
						break;
					case EventType.StopShaking:
						if (shakeCoroutine != null) CutsceneManager.self.StopCoroutine(shakeCoroutine);
						break;
					case EventType.FaceChange:
						if (faceCoroutine != null) self.StopCoroutine(faceCoroutine);

						if ((string) data[0] == "random_mad") {
							self.StartCoroutine(AlienFace(true));
						} else {
							Image image = currentCutscene.alien.transform.GetChild(0).GetComponent<Image>();
							Texture2D face = Resources.Load<Texture2D>((string) data[0]);
							image.sprite = Sprite.Create(face, new Rect(0, 0, face.width, face.height), new Vector2(.5f, .5f));
							Resources.UnloadAsset(face);
						}

						break;
					case EventType.SmallExplode:
						if (explosionCoroutine != null) self.StopCoroutine(explosionCoroutine);
						explosionCoroutine = self.StartCoroutine(SmallExplosions((float) data[0], (float) data[1]));
						break;
					case EventType.Explode:
						if (explosionCoroutine != null) self.StopCoroutine(explosionCoroutine);
						Explosion ex = Instantiate<GameObject>(self.explosionPrefab, currentCutscene.alien.transform.position, new Quaternion(), GameObject.Find("CanvasTop").transform).GetComponent<Explosion>();
						ex.StartCoroutine(ex.AnimateExplosion((float) data[0]));
						if (shakeCoroutine != null) CutsceneManager.self.StopCoroutine(shakeCoroutine);
						Destroy(currentCutscene.alien);
						break;
				}
			} catch (System.Exception e) {
				Debug.LogError("Invalid CutsceneEvent Data");
				Debug.LogException(e);
			}
		}
	}

	[Serializable]
	public class CutsceneJSON {
		public float duration;
		public EventJSON[] events;

		[Serializable]
		public class EventJSON {
			public string type;
			public string data;
			public float timing;
		}

		public Vector3 GetVector3(ref string[] data, string s) {
			string[] x = GetValue(ref data, s).Split(' ');
			return new Vector3(float.Parse(x[0]), float.Parse(x[1]), 10);
		}

		public string GetValue(ref string[] data, string s) {
			 return data[Array.IndexOf(data, s) + 1];
		}

		public float GetFloat(ref string[] data, string s) {
			return float.Parse(GetValue(ref data, s));
		}

		public bool GetBool(ref string[] data, string s) {
			return Boolean.Parse(GetValue(ref data, s));
		}

		public CutsceneData CreateCutscene() {
			List<CutsceneEvent> evs = new List<CutsceneEvent>();

			foreach (EventJSON o in events) {
				EventType type = (EventType) Enum.Parse(typeof(EventType), o.type);

				switch (type) {
					case EventType.Zeek:
						evs.Add(new CutsceneEvent(new object[] { }, o.timing, type));
						continue;
					case EventType.StopShaking:
						evs.Add(new CutsceneEvent(new object[] { }, o.timing, type));
						continue;
					case EventType.FaceChange:
						evs.Add(new CutsceneEvent(new object[] { o.data }, o.timing, type));
						continue;
				}

				string[] data = o.data.Split(';');
				object[] toSend = null;

				for (int i = 0; i < data.Length; i += 2) {
					switch (type) {
						case EventType.ShowText:
							toSend = new object[] { GetValue(ref data, "text").Replace(" ", "  "), GetFloat(ref data, "duration"), GetBool(ref data, "face") };
							break;
						case EventType.CameraShake:
							toSend = new object[] { GetFloat(ref data, "amount"), GetFloat(ref data, "decay") };
							break;
						case EventType.FaceChange:
							toSend = new object[] { GetValue(ref data, "face") };
							break;
						case EventType.MoveAlien:
							toSend = new object[] { GetVector3(ref data, "pos"), GetFloat(ref data, "duration") };
							break;
						case EventType.SetAlienPos:
							toSend = new object[] { GetVector3(ref data, "pos") };
							break;
						case EventType.PlaySound:
							toSend = new object[] { GetValue(ref data, "sound") };
							break;
						case EventType.ShakeAlien:
							toSend = new object[] { GetFloat(ref data, "amount") };
							break;
						case EventType.SmallExplode:
							toSend = new object[] { GetFloat(ref data, "period"), GetFloat(ref data, "explosion_duration") };
							break;
						case EventType.Explode:
							toSend = new object[] { GetFloat(ref data, "explosion_duration") };
							break;
					}
				}

				evs.Add(new CutsceneEvent(toSend, o.timing, type));
			}

			return new CutsceneData(evs, duration);
		}
	}
}
