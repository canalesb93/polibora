using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class LevelLoader : MonoBehaviour {

	public string SceneName;

	void OnMouseDown() {
		SceneManager.LoadScene (SceneName, LoadSceneMode.Single);
	}
}
