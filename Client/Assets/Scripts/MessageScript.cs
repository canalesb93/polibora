using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MessageScript : MonoBehaviour {

	public string SceneName;

	void OnMouseDown() {
		HideMessage ();
	}

	public void ShowMessage(string text) {
		GetComponent<Renderer> ().enabled = true;
		GetComponent<CanvasRenderer> ().SetAlpha (1.0f);
		GetComponent<Text> ().text = text + "\n Presiona para continuar...";
	}

	public void HideMessage() {
		GetComponent<Renderer> ().enabled = false;
		GetComponent<CanvasRenderer> ().SetAlpha (0.0f);
	}

	public bool IsShown() {
		return GetComponent<Renderer> ().enabled;
	}
}
