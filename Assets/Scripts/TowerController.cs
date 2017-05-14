using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour {

	private SpriteRenderer spRender;

	public int gridRow;
	public int gridCol;
	public GameObject reflector;
	public GameObject redLaser;
	public GameObject blueLaser;

	private GameController controller;

	// Use this for initialization
	void Start () {
		spRender = GetComponent<SpriteRenderer> ();
		controller = GameController.FindController ();
		controller.RegisterTower(this, gridRow, gridCol);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnCapture (GameController.PlayerTeam team) {
		switch (team) {
			case GameController.PlayerTeam.Red:
				spRender.color = GameController.RED_COLOR;
				break;
			case GameController.PlayerTeam.Blue:
				spRender.color = GameController.BLUE_COLOR;
				break;
			default:
				spRender.color = GameController.NEUTRAL_COLOR;
				break;
		}
	}

	public void UpdateLasers (bool redLaser, bool blueLaser, GameController.LaserDirection direction) {
		//Debug.Log("Tower update: " + redLaser + "/" + blueLaser + "/" + direction.ToString());
		reflector.transform.rotation = Quaternion.Euler(0, 0, (float)direction);
		this.redLaser.SetActive(redLaser);
		this.blueLaser.SetActive(blueLaser);
	}
}
