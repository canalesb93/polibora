using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SnakeBehaviourScript : MonoBehaviour {

	private const float SPEED = 1f; // moves 1 tile per step.
	private const int MINIMUM_SCORE = 0;

	private float stepRate = 0.1f; // step rate
	private float timeToStep = 0.35f;
	private const float fireRate = 3.5f;
	private float timeToFire = 0.5f;

	public int currentLevel;
	public MessageScript message;
	// foodobjects start
	public GameObject polieteno;
	public GameObject poliestireno;
	public GameObject poliproileno;
	public GameObject policloruro;
	public GameObject teflon;
	public GameObject polibutadieno;
	public GameObject reaccion;
	// foodobjects end
	public GameObject fire;
	public Transform northWall, westWall, eastWall, southWall;

	private int levelMonomer;

	private Vector3 newDirection = new Vector3 (0, 0, 0);
	private List<GameObject> snakeParts = new List<GameObject>();
	private bool eat = false;
	private bool lost = false;
	private bool exit = false;
	private bool firstTimeFire = true;
	private bool firstTimeMonomer = true;
	private bool showEducation1 = true;
	private bool showEducation2 = false;
	private bool showEducation3 = false;

	private Text scoreText;
	private GameObject currentFire;
	private GameObject currentReaction;
	private List<GameObject> monomers = new List<GameObject>();
	private int score = 0;

	// MESSAGES
	string firstFireMessage = "Ouch!\n Recuerda que el fuego degrada a un polimero. Ten cuidado con el calor!";
	string firstMonomerMessage = "Recuerda que los polimeros de adición solo funcionan con monomeros del mismo tipo.";

	// Initializes Application
	void Start () {
		levelMonomer = currentLevel - 1;
		ShowIntroMessage ();
		SpawnFood ();
		SpawnFire ();
		scoreText = GameObject.FindWithTag ("Score").GetComponent<Text>();
		scoreText.text = "Puntaje: " + score;
		UpdateBorders ();

		Vector2 ta = new Vector2 (transform.position.x, transform.position.y - 1);
		GameObject g =(GameObject)Instantiate(getMonomer(levelMonomer), ta, Quaternion.identity);
		g.transform.eulerAngles = transform.eulerAngles;
		g.tag = "Tail";
		snakeParts.Insert(0, g);
	}

	void UpdateBorders() {
		Vector3 stageDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height,0));
		eastWall.transform.position = new Vector3 (-Mathf.Round(stageDimensions.x) - 0.5f, 0, 0);
		westWall.transform.position = new Vector3 (Mathf.Round(stageDimensions.x) + 0.5f, 0, 0);
		northWall.transform.position = new Vector3 (0, Mathf.Round(stageDimensions.y) - 0.5f, 0);
		southWall.transform.position = new Vector3 (0, -Mathf.Round(stageDimensions.y) - 0.5f, 0);
	}

	// Update is called once per frame.
	// Handles controls.
	void Update () {


		// if message is not shown, execute game...
		if (!message.IsShown()) {
			if (currentReaction) {
				Destroy(currentReaction);
			}

			if (exit) {
				SceneManager.LoadScene ("Menu", LoadSceneMode.Single);
			}

			if (showEducation1) {
				ShowEducation1 ();
			} else if (showEducation2) {
				ShowEducation2 ();
			} else if (showEducation3) {
				ShowEducation3 ();
			}


			// Update Steps
			if (timeToStep <= 0f) {
				timeToStep = stepRate;
				Step ();
			}
			timeToStep -= Time.deltaTime;

			// Update Fires
			if (timeToFire <= 0f) {
				timeToFire = fireRate;
				randomizePosition (currentFire);
			}
			timeToFire -= Time.deltaTime;

			// Check input keys, can't move in opposite direction.
			if (Input.GetKeyDown (KeyCode.RightArrow) && transform.eulerAngles.z != 90) {
				newDirection = new Vector3 (0, 0, 270);
				forceStep();
			} else if (Input.GetKeyDown (KeyCode.UpArrow) && transform.eulerAngles.z != 180) {
				newDirection = new Vector3 (0, 0, 0);
				forceStep();
			} else if (Input.GetKeyDown (KeyCode.DownArrow) && transform.eulerAngles.z != 0) {
				newDirection = new Vector3 (0, 0, 180);
				forceStep();
			} else if (Input.GetKeyDown (KeyCode.LeftArrow) && transform.eulerAngles.z != 270) {
				newDirection = new Vector3 (0, 0, 90);
				forceStep();
			}

			if (Input.GetKeyDown (KeyCode.Escape)) {
				exit = true;
			} else if (Input.GetKeyDown (KeyCode.P)) {
				message.ShowMessage ("Pausa\n");
			}


			if (lost || score < MINIMUM_SCORE) {
				lost = false;
				ShowOutroMessage ();
				exit = true;
			}
			
		}
	}

	void forceStep(){
		timeToStep = stepRate;
		Step();
	}

	// ========== Game Logic ==========

	// Updates the location of the snake.
	void Step() {
		Vector2 ta = transform.position;
		if (eat) {
			GameObject g =(GameObject)Instantiate(getMonomer(levelMonomer), ta, Quaternion.identity);
			g.transform.eulerAngles = transform.eulerAngles;
			g.tag = "Tail";
			snakeParts.Insert(0, g);
			eat = false;
		}
		else if (snakeParts.Count > 0) {
			snakeParts.Last().transform.position = ta;
			snakeParts.Last().transform.eulerAngles = transform.eulerAngles;
			snakeParts.Insert(0, snakeParts.Last());
			snakeParts.RemoveAt(snakeParts.Count-1);
		}
		transform.eulerAngles = newDirection;
		transform.Translate(Vector2.up * SPEED);
	}

	// Spawns the food that the snake will eat.
	public void SpawnFood() {
		//clear all foods
		foreach (GameObject m in monomers) {
			Destroy (m);
		}
		monomers.Clear ();

		for (int i = 0; i < currentLevel + 1; i++) {
			int m = (int)Random.Range (0, 6);
			if (m == levelMonomer) {
				i--;
				continue;
			} else {
				SpawnMonomer (m);
			}
		}
		SpawnMonomer (levelMonomer);
	}

	public void SpawnMonomer(int m) {
		int x = (int)Random.Range (eastWall.position.x + 1, westWall.position.x - 1);
		int y = (int)Random.Range (southWall.position.y + 1, northWall.position.y - 2);
		if (positionTaken (x, y)) {
			SpawnMonomer (m);
			return;
		}

		GameObject g = (GameObject)Instantiate (getMonomer(m), new Vector2 (x, y), Quaternion.identity);
		monomers.Insert(0, g);
	}

	public void SpawnFire() {
		int x = (int)Random.Range (eastWall.position.x + 1, westWall.position.x - 1);
		int y = (int)Random.Range (southWall.position.y + 1, northWall.position.y - 2);
		if (positionTaken (x, y)) {
			SpawnFire ();
			return;
		}
		currentFire = (GameObject)Instantiate (fire, new Vector2 (x, y), Quaternion.identity);
	}

	public void SpawnReaction(){
		int x = 0;
		int y = 5;

		currentReaction = (GameObject)Instantiate (reaccion, new Vector2 (x, y), Quaternion.identity);
	}

	// ========== Events ==========

	// Handles collisions.
	void OnTriggerEnter2D(Collider2D c) {
		if (c.tag == "Monomer") {
			if (stepRate > 0.05f) {
				stepRate -= 0.001f;
			}
			int monomerID = getMonomerIDByName (c.name);
			if (monomerID == levelMonomer) {
				eat = true;
				SpawnFood ();
				increaseScore ();
			} else {
				if (firstTimeMonomer) {
					message.ShowMessage (firstMonomerMessage);
					firstTimeMonomer = false;
				}
				decreaseScore ();
			}
			Destroy (c.gameObject);
		} else if (c.name.StartsWith("fire")) {
			if (firstTimeFire) {
				message.ShowMessage (firstFireMessage);
				firstTimeFire = false;
			}
			for (int i = 0; i < 5; i++) {
				GameObject g = snakeParts.Last ();
				if (g) {
					snakeParts.RemoveAt (snakeParts.Count - 1);
					Destroy (g);
					decreaseScore ();
				}
			}

			Destroy(c.gameObject);
			SpawnFire ();
		} else if (c.name.EndsWith("wall") || c.tag == "Tail") {
			lost = true;
		}

		if (lost) {
		}
	}
		
	// ========== GUI ==========

	public void increaseScore() {
		score+=10;
		scoreText.text = "Puntaje: " + score;
	}

	public void decreaseScore() {
		score-=15;
		scoreText.text = "Puntaje: " + score;
	}

	private void ShowIntroMessage() {
		message.ShowMessage (getMonomerNameByMonomer(levelMonomer) + "\n"+getIntroMessageByMonomer(levelMonomer));
		SpawnReaction();
	}

	private void ShowOutroMessage() {
		string victoryMessage = "Game Over!\nObtuviste " + snakeParts.Count + " monomeros, haz creado " + (snakeParts.Count / 10);
		message.ShowMessage (victoryMessage + " " + getProductNameByMonomer(levelMonomer) + "s\n" + getOutroMessageByMonomer(levelMonomer));
		SpawnReaction();
	}

	private void ShowEducation1(){
		showEducation1=false;
		message.ShowMessage ("¡Prepárate para sintetizar polímeros de adición!\n\nLa reacción de adición tiene 3 pasos:\nIniciación, Crecimiento y Terminación\n");
		showEducation2 = true;
	}

	private void ShowEducation2(){
		showEducation2=false;
		message.ShowMessage ("Iniciación: CH2=CH2 + catalizador ⇒ •CH2–CH2• (radicales libres)\n\nCrecimiento: 2 •CH2–CH2• ⇒ •CH2–CH2–CH2–CH2•\n");
		showEducation3 = true;
	}

	private void ShowEducation3(){
		showEducation3=false;
		message.ShowMessage ("Terminación: Los radicales libres de los extremos se unen a impurezas o bien se unen dos cadenas con un terminal neutralizado.\n");
	}
	// ========== Utils ==========

	private bool positionTaken(float x, float y) {
		// head using space
		if (transform.position.x == x && transform.position.y == y) {
			return true;
		}

		// fire using space
		if (fire.transform.position.x == x && fire.transform.position.y == y) {
			return true;
		}

		// body part using space
		foreach (GameObject part in snakeParts) {
			if (part.transform.position.x == x && part.transform.position.y == y) {
				return true;
			}
		}

		return false;
	}

	private void randomizePosition(GameObject g) {
		int x = (int)Random.Range (eastWall.position.x + 1, westWall.position.x - 1);
		int y = (int)Random.Range (southWall.position.y + 1, northWall.position.y - 2);
		if (positionTaken (x, y)) {
			randomizePosition (g);
			return;
		}
		g.transform.position = new Vector2 (x, y);
	}

	public GameObject getMonomer(int m) {
		switch(m) {
		case 0:
			return polieteno;
		case 1:
			return poliestireno;
		case 2:
			return poliproileno;
		case 3:
			return policloruro;
		case 4:
			return teflon;
		case 5:
			return polibutadieno;
		default:
			return polieteno;
		}
	}

	public int getMonomerIDByName(string m) {
		if (m.StartsWith ("polieteno")) {
			return 0;
		} else if (m.StartsWith ("poliestireno")) {
			return 1;
		} else if (m.StartsWith ("polipropileno")) {
			return 2;
		} else if (m.StartsWith ("policloruro")) {
			return 3;
		} else if (m.StartsWith ("teflon")) {
			return 4;
		} else if (m.StartsWith ("polibutadieno")) {
			return 5;
		}
		return 0;
	}

	private string getMonomerNameByMonomer(int m) {
		switch(m) {
		case 0:
			return "Polieteno";
		case 1:
			return "Poliestireno";
		case 2:
			return "Polipropileno";
			break;
		case 3:
			return "Policloruro";
		case 4:
			return "Teflón";
		case 5:
			return "Polibutadieno";
		default:
			return "-";
		}
	}

	private string getProductNameByMonomer(int m) {
		switch(m) {
		case 0:
			return "Botella";
		case 1:
			return "Hielera";
		case 2:
			return "Cubeta";
		case 3:
			return "PVC";
		case 4:
			return "Sarten";
		case 5:
			return "Llanta";
		default:
			return "-";
		}
	}

	private string getIntroMessageByMonomer(int m) {
		return getOutroMessageByMonomer(m);
	}

	private string getOutroMessageByMonomer(int m) {
		switch(m) {
		case 0:
			return "Las botellas de plástico son increíblemente usadas en empaque y distribución de toda clase de productos cotidianos.";
		case 1:
			return "Industrial y domésticamente, el poliestireno se utiliza por su capacidad aislante.";
		case 2:
			return "Un plástico de polipropileno se utiliza como contenedor por su dureza por encima de uno de polietileno.";
		case 3:
			return "Los tubos de PVC son famosos por su resistencia y su aislamiento térmico.";
		case 4:
			return "El teflón está siempre presente en utensilios de cocina por dejar nada pegado.";
		case 5:
			return "El caucho derivado del polibutadieno se encuentra cualquier neumático.";
		default:
			return "-";
		}
	}
}
