using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitterController : MonoBehaviour {
	public int gridRow;
	public GameController.PlayerTeam team;

	private GameController controller;

	// Use this for initialization
	void Start () {
		// Emitters must be owned.
		UnityEngine.Assertions.Assert.AreNotEqual (GameController.PlayerTeam.Nobody, team);
		controller = GameController.FindController ();
		controller.RegisterEmitter (this, gridRow, team);
	}

	public void SetActive (bool active) {
		// This is an ugly hack because Unity's GetComponentsInChildren also includes the parent for some reason.
		// Using the transform's children on the other hand works fine.
		foreach (Transform tChild in transform) {
			tChild.gameObject.GetComponent<SpriteRenderer> ().enabled = active;
		}
	}
}
