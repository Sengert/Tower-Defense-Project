using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GUIManager : MonoBehaviour {
	GameObject options;
	GameObject confirmExit;

	void Start()
	{
		options = GameObject.Find ("Options Box");
		options.SetActive (false);

		confirmExit = GameObject.Find ("Exit Confirmation Box");
		confirmExit.SetActive (false);
	}

	public void ToggleOptionsMenu(bool isActive)
	{
		//GameObject.Find ("Options Box").SetActive (isActive);
		options.SetActive(isActive);
	}

	public void ToggleExitConfirmationBox(bool isActive)
	{
		//GameObject.Find ("Options Box").SetActive (isActive);
		confirmExit.SetActive(isActive);
	}

	public void ToggleControlsMenu(bool isActive)
	{
		Debug.Log ("Controls menu coming soon!");
	}

	public void StartGame()
	{
		SceneManager.LoadScene ("gameScene");
	}

	public void ExitGame()
	{
		Application.Quit ();
	}
}
