using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelReader : MonoBehaviour {
	public GameObject blockPrefab;
	public GameObject mainObjectPrefab;
	public GameObject objectPrefab;
	public GameObject goalPrefab;
	public GameObject doorPrefab;
	public GameObject poweredDoorPrefab;
	public GameObject rotaryDoorPrefab;
	public GameObject buttonPrefab;
	public GameObject spikePrefab;
	public GameObject teleportPrefab;
	public GameObject objectsParent;
	public GameObject powerDash;
	public GameObject powerLine;
	public GameObject powerT;
	public GameObject powerCorner;
	public GameObject powerCross;
	public GameObject board;
	public GameObject background;

	public Texture2D levelImage;

	readonly Color poweredDoorColor = new Color(150f / 255f, 50f / 255f, 150f / 255f);
	readonly Color rotaryDoorColor = new Color(1, 100f / 255f, 0f);
	readonly Color buttonColor = new Color(1, 1, 0);
	readonly Color buttonDirColor = new Color(.2f, .2f, .2f);
	readonly Color spikeColor = new Color(1f, 0f, 100f / 255f);
	readonly Color teleportAColor = new Color(186f / 255f, 1f, 0f);
	readonly Color teleportBColor = new Color(0f, 103f / 255f, 27f / 255f);
	readonly Color wallTintColor = new Color(180f / 255f, 180f / 255f, 180f / 255f);
	public static List<ObjectPosition> originalPositions = new List<ObjectPosition>();

	// Offset is a multiplier of levelSize
	public void BuildLevel(int level, LevelSizeData roomSize, OffsetData offset) {
		if (level == 6) {

			return;
		}

		Stats.UpdateText(ref Stats.levelText, level);

		levelImage = (Texture2D) Resources.Load("Levels/" + level.ToString());
		if (levelImage == null) return;
		int w = levelImage.width, h = levelImage.height;
		float o = blockPrefab.transform.localScale.x;
		// Blame Unity not me for making me make a table of the same exact colors
		Color[] whiteColors = new Color[10] { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white };
		Color[] whiteColors2 = new Color[14] { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white, Color.white };
		TeleportPositions tp = new TeleportPositions();
		Position objectPosition = new Position(-1, -1);
		Texture2D[] wallTextures;
		Texture2D backgroundTexture;

		GetLevelTextures(level, out wallTextures, out backgroundTexture); // Small optimization (prob like 1 CPU cycle) to call one function instead of two
		originalPositions.Clear();
		Board.self.objects.Clear();
		Board.boardSize = new LevelSizeData((ushort) levelImage.width, (ushort) levelImage.height);
		Board.roomSize = roomSize;

		// Reads the level into memory from an image file
		// Variables are limited here because this is a 2 dimensional loop, running at O(n^2). Variable creation and this speed can hog up ram and CPU cycles. 
		#region Main Loop
		for (int i = 0; i < w; i++) {
			for (int j = 0; j < h; j++) {
				Color c = levelImage.GetPixel(i, j);
				if (c == Color.black || c == buttonDirColor) { // wall
					Texture2D t = wallTextures[Mathf.FloorToInt(Random.Range(0, wallTextures.Length))];
					SpriteRenderer s = Instantiate<GameObject>(blockPrefab, new Vector3(i * o, j * o, 0), new Quaternion(), board.transform).GetComponentInChildren<SpriteRenderer>();
					s.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f)); // Wall, and set sprite texture to a random one
					s.transform.rotation = Quaternion.Euler(0, 0, 90 * Mathf.FloorToInt(Random.Range(0, 4)));
					s.color = wallTintColor;
				} else if (c == Color.blue) {
					Board.self.objects.Add(AddPosition(Instantiate<GameObject>(mainObjectPrefab, new Vector3(i * o, j * o, 0), new Quaternion(), objectsParent.transform), true).GetComponent<Rigidbody>()); // Main Object
					objectPosition = new Position(i, j);
				} else if (c == Color.cyan) Board.self.objects.Add(AddPosition(Instantiate<GameObject>(objectPrefab, new Vector3(i * o, j * o, 0), new Quaternion(), objectsParent.transform), false).GetComponent<Rigidbody>()); // Object
				else if (c == spikeColor) Instantiate<GameObject>(spikePrefab, new Vector3(i * o, j * o, 0), new Quaternion(), board.transform); // Spike
				else if (c == teleportAColor) {
					tp.a = new Vector3(i * o, j * o, 0);
				} else if (c == teleportBColor) {
					tp.b = new Vector3(i * o, j * o, 0);
				} else if (c == Color.green) { // Goal
					int x = 0, y = 0;
					bool xf = false, yf = false;
					Color[] white3;
					// Get the custom size for the goal
					do {
						if (!xf && i + x < w && levelImage.GetPixel(i + x, j) == Color.green) x++;
						else xf = true;
						if (!yf && i + y < h && levelImage.GetPixel(i, j + y) == Color.green) y++;
						else yf = true;
					} while (xf == false || yf == false);

					// Scale the goal object 
					Transform t = Instantiate<GameObject>(goalPrefab, new Vector3(((i + i + x - 1) * o) / 2, ((j + j + y - 1) * o) / 2, 0), new Quaternion(), board.transform).transform;
					t.localScale = new Vector3(t.localScale.x * x, t.localScale.y * y, t.localScale.z);
					white3 = new Color[x * y];
					// Populate an array of white so that we can clear this spot on the image
					for (int k = 0; k < x * y; k++) {
						white3[k] = Color.white;
					}

					levelImage.SetPixels(i, j, x, y, white3);
				} else if (c == buttonColor) { // Button
											   // Now we look for a grey pixel next to the button to find out the direction the button needs to be placed
					if (i + 1 < w && levelImage.GetPixel(i + 1, j) == buttonDirColor) {
						Instantiate<GameObject>(buttonPrefab, new Vector3(i * o, j * o, 0), Quaternion.Euler(0, 0, 90), board.transform).GetComponent<Button>().Init(Button.ButtonDirection.right);
					} else if (i - 1 > 0 && levelImage.GetPixel(i - 1, j) == buttonDirColor) {
						Instantiate<GameObject>(buttonPrefab, new Vector3(i * o, j * o, 0), Quaternion.Euler(0, 0, -90), board.transform).GetComponent<Button>().Init(Button.ButtonDirection.left);
					} else if (j + 1 < h && levelImage.GetPixel(i, j + 1) == buttonDirColor) {
						Instantiate<GameObject>(buttonPrefab, new Vector3(i * o, j * o, 0), Quaternion.Euler(0, 0, 180), board.transform).GetComponent<Button>().Init(Button.ButtonDirection.up);
					} else if (j - 1 > 0 && levelImage.GetPixel(i, j - 1) == buttonDirColor) {
						Instantiate<GameObject>(buttonPrefab, new Vector3(i * o, j * o, 0), new Quaternion(), board.transform).GetComponent<Button>().Init(Button.ButtonDirection.down);
					}
				} else if (c == Color.red || c == new Color(1, 0, 1) || c == poweredDoorColor || c == rotaryDoorColor) { // Doors
																														 // We found the beginning block of a door, find the direction of the door. Doors will always be 2x5.
					if (j + 3 < h) {
						Color t = levelImage.GetPixel(i, j + 3);
						if (t == c) {
							// Door is vertical.
							if (c != poweredDoorColor && c != rotaryDoorColor) Instantiate<GameObject>(doorPrefab
							   , new Vector3(((i + i + 1) * o) / 2, ((j + j + 4) * o) / 2, 0)
							   , Quaternion.Euler(new Vector3(0, 0, 90))
							   , board.transform).GetComponent<Door>().Init(c == Color.red ? Door.DoorColor.red : Door.DoorColor.blue);
							else if (c == poweredDoorColor) Instantiate<GameObject>(poweredDoorPrefab
							   , new Vector3(((i + i + 1) * o) / 2, ((j + j + 4) * o) / 2, 0)
							   , Quaternion.Euler(new Vector3(0, 0, 90))
							   , board.transform);
							else if (c == rotaryDoorColor) {
								Instantiate<GameObject>(rotaryDoorPrefab
							   , new Vector3(((i + i + 1) * o) / 2, ((j + j + 6) * o) / 2, 0)
							   , Quaternion.Euler(new Vector3(0, 0, 90))
							   , board.transform);

								levelImage.SetPixels(i, j, 2, 7, whiteColors2);
								continue;
							}
							// Set the door in the image to white so that we don't keep calling this block of code. Damn unity requires a full array of just Color.white. :/
							levelImage.SetPixels(i, j, 2, 5, whiteColors);
							continue;
						}
					}

					if (i + 3 < h) {
						Color t = levelImage.GetPixel(i + 3, j);
						if (t == c) {
							// Door is horizontal
							if (c != poweredDoorColor && c != rotaryDoorColor) Instantiate<GameObject>(doorPrefab
								, new Vector3(((i + i + 4) * o) / 2, ((j + j + 1) * o) / 2, 0)
								, new Quaternion(),
								board.transform).GetComponent<Door>().Init(c == Color.red ? Door.DoorColor.red : Door.DoorColor.blue);
							else if (c == poweredDoorColor) Instantiate<GameObject>(poweredDoorPrefab
							   , new Vector3(((i + i + 4) * o) / 2, ((j + j + 1) * o) / 2, 0)
							   , new Quaternion()
							   , board.transform);
							else if (c == rotaryDoorColor) {
								Instantiate<GameObject>(rotaryDoorPrefab
							   , new Vector3(((i + i + 6) * o) / 2, ((j + j + 1) * o) / 2, 0)
							   , new Quaternion()
							   , board.transform);

								levelImage.SetPixels(i, j, 7, 2, whiteColors2);
								continue;
							}
							// Set the door in the image to white so that we don't keep calling this block of code
							levelImage.SetPixels(i, j, 5, 2, whiteColors);
							continue;
						}
					}
				} 
			}
		}
		#endregion


		#region Power Loop
		Texture2D power = (Texture2D) Resources.Load("Levels/" + level.ToString() + "_power");

		if (tp.a != Vector3.zero && tp.b != Vector3.zero) {
			GameObject a = Instantiate<GameObject>(teleportPrefab, new Vector3(tp.a.x * o, tp.a.y * o, 0), new Quaternion(), board.transform);
			GameObject b = Instantiate<GameObject>(teleportPrefab, new Vector3(tp.b.x * o, tp.b.y * o, 0), new Quaternion(), board.transform);

			a.GetComponent<Teleport>().Init(b);
			b.GetComponent<Teleport>().Init(a);
		}

		if (power != null) {

			for (int i = 0; i < w; i++) {
				for (int j = 0; j < h; j++) {
					Color c = power.GetPixel(i, j);
					if (c == Color.blue) {
						uint u = 1, r = 2, d = 4, l = 8, x = 0; uint ct = 0;
						// <magic>
						if (i + 1 < w && power.GetPixel(i + 1, j) == Color.blue) {
							x = (x | r);
							ct++;
						}
						if (i - 1 > 0 && power.GetPixel(i - 1, j) == Color.blue) {
							x = (x | l);
							ct++;
						}
						if (j + 1 < h && power.GetPixel(i, j + 1) == Color.blue) {
							x = (x | u);
							ct++;
						}
						if (j - 1 > 0 && power.GetPixel(i, j - 1) == Color.blue) {
							x = (x | d);
							ct++;
						}

						// DO NOT TOUCH ANYTHING IN THIS SWITCH IT WORKS FOR UNKNOWN REASONS
						switch (ct) {
							case 4:
								Instantiate<GameObject>(powerCross, new Vector3(i * o, j * o, -5f), new Quaternion(), board.transform); // cross
								break;
							case 3:
								if ((x & u) != u) Instantiate<GameObject>(powerT, new Vector3(i * o, j * o, -5), Quaternion.Euler(new Vector3(0, 0, -90)), board.transform); // T cross rot -90
								else if ((x & r) != r) Instantiate<GameObject>(powerT, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 180)), board.transform); // T cross rot 180
								else if ((x & d) != d) Instantiate<GameObject>(powerT, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 90)), board.transform); // T cross rot 90
								else if ((x & l) != l) Instantiate<GameObject>(powerT, new Vector3(i * o, j * o, -5f), new Quaternion(), board.transform); // T cross rot 0
								break;
							case 2:
								if ((x & (u | d)) == (u | d)) Instantiate<GameObject>(powerLine, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 90)), board.transform); // vertical line
								else if ((x & (l | r)) == (l | r)) Instantiate<GameObject>(powerLine, new Vector3(i * o, j * o, -5f), new Quaternion(), board.transform); // hor. line
								else if ((x & (u | r)) == (u | r)) Instantiate<GameObject>(powerCorner, new Vector3(i * o, j * o, -5f), new Quaternion(), board.transform); // corner rot 0
								else if ((x & (u | l)) == (u | l)) Instantiate<GameObject>(powerCorner, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 90)), board.transform); // corner rot 90
								else if ((x & (d | r)) == (d | r)) Instantiate<GameObject>(powerCorner, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 270)), board.transform); // corner rot 270
								else if ((x & (d | l)) == (d | l)) Instantiate<GameObject>(powerCorner, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 180)), board.transform); // corner rot 180
								break;
							case 1:
								if ((x & r) == r) Instantiate<GameObject>(powerDash, new Vector3(i * o, j * o, -5f), new Quaternion(), board.transform); // dash rot 0
								else if ((x & u) == u) Instantiate<GameObject>(powerDash, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 90)), board.transform); // dash rot 90
								else if ((x & l) == l) Instantiate<GameObject>(powerDash, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 180)), board.transform); // dash rot 180
								else if ((x & d) == d) Instantiate<GameObject>(powerDash, new Vector3(i * o, j * o, -5f), Quaternion.Euler(new Vector3(0, 0, 270)), board.transform); // dash rot 270
								break;
						}
						// </magic>
					}
				}
			}
		}
		#endregion

		Resources.UnloadAsset(power);

		// Finds the center point for camera rotation automatically, such that map size can be changed
		//offset.x = Mathf.FloorToInt(objectPosition.x / roomSize.x);mmmmmmmmmmmmmmmmmmmmmmk
		//	offset.y = Mathf.FloorToInt(objectPosition.y / roomSize.y);

		// And last but not least, the background texture
		// It is parented to the camera to prevent it from rotating
		background.GetComponent<SpriteRenderer>().sprite = Sprite.Create(backgroundTexture, new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), new Vector2(.5f, .5f));
		
		Board.centroid = Board.CalculateCentroid(new LevelSizeData((ushort) levelImage.width, (ushort) levelImage.height), new OffsetData(0, 0));
		Board.SetCameraPosition(Board.centroid);
		Board.guiManager.StartCoroutine(Board.guiManager.PressAnyKey());

		Resources.UnloadAsset(levelImage);
	}

	public static void GetLevelTextures(int level, out Texture2D[] walls, out Texture2D background) {
		walls = Resources.LoadAll<Texture2D>("LevelTextures/" + level.ToString() + "/Walls/"); // Loads the various textures for the blocks
		background = Resources.Load<Texture2D>("LevelTextures/" + level.ToString() + "/background");
	}

	public static GameObject AddPosition(GameObject g, bool isMainObject) {
		originalPositions.Add(new ObjectPosition(g.transform.localPosition + new Vector3(0, 1f, 0), g, isMainObject));
		return g;
	}

	public static void ResetPositions() {
		foreach (ObjectPosition o in originalPositions) {
			if (o.gameobject != null) o.Reset();
		}
	}

	public struct ObjectPosition {
		public Vector3 originalPosition;
		public GameObject gameobject;
		public bool isMainObject;

		public ObjectPosition(Vector3 pos, GameObject go, bool isMainObject) {
			originalPosition = pos;
			gameobject = go;
			this.isMainObject = isMainObject;
		}

		public void Reset() {
			gameobject.transform.position = originalPosition;
			gameobject.GetComponent<Renderer>().enabled = true;
			gameobject.GetComponent<Collider>().enabled = true;
		}
	}

	public struct Position {
		public int x;
		public int y;

		public Position(int x, int y) {
			this.x = x;
			this.y = y;
		}
	}

	public class TeleportPositions {
		public Vector3 a;
		public Vector3 b;

		public TeleportPositions() { }
	}
}
