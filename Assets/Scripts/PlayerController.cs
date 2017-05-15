using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour {

	public GameController.PlayerTeam team;

	public float speed;

	public GameObject progressBarBase;
	public GameObject progressBar;

	private string horizontalAxis;
	private string verticalAxis;
	private string actionAxis;

	private Rigidbody2D rigidbody2d;

	private Collider2D currentCollision = null;
	private int captureProgress = 0;
	private GameController controller;

	// Use this for initialization
	void Start () {
		Assert.IsTrue (team == GameController.PlayerTeam.Blue || team == GameController.PlayerTeam.Red);

		string suffix;
		if (team == GameController.PlayerTeam.Red) {
			suffix = " P1";
		} else {
			suffix = " P2";
		}
		
		horizontalAxis = "Horizontal" + suffix;
		verticalAxis = "Vertical" + suffix;
		actionAxis = "Action" + suffix;

		rigidbody2d = GetComponent<Rigidbody2D> ();
		controller = GameController.FindController ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 motionVec = rigidbody2d.GetPointVelocity (this.transform.position);
		bool right = motionVec.normalized.x > 0;
		if (motionVec == Vector2.zero) return;
		float angle = Vector2.Angle (Vector2.up, motionVec);
		if (right) angle = -angle;
		transform.rotation = Quaternion.Euler (0, 0, angle);
	}

	// Called regularly. Do physics/movement/timed actions here.
	void FixedUpdate () {
		float horiz = Input.GetAxis (horizontalAxis);
		float vert = Input.GetAxis (verticalAxis);
		bool action = Input.GetButton (actionAxis);

		if (action && currentCollision != null) {
			if (currentCollision.gameObject.tag == "Emitter") {
				HandleEmitterAction (horiz, vert, currentCollision.gameObject.GetComponent<EmitterController> ());
			} else if (currentCollision.gameObject.tag == "Tower") {
				HandleTowerAction (horiz, vert, currentCollision.gameObject.GetComponent<TowerController> ());
			}
		} else {
			DoMovement (horiz, vert);
		}
	}

	void HandleEmitterAction (float horiz, float vert, EmitterController emitter) {
		// Only change own emitter.
		if (emitter.team == team) {
			controller.ChangeActiveEmitter(emitter.gridRow, team);
		}
		DoMovement (horiz, vert);
	}

	void HandleTowerAction (float horiz, float vert, TowerController tower) {
		if (controller.GetTowerOwner(tower) != team) {
			// Enemy/Neutral tower.
			captureProgress += 1;
			progressBarBase.SetActive (true);
			progressBarBase.transform.position = new Vector3(tower.transform.position.x, tower.transform.position.y - 2, 0);
			progressBar.GetComponent<ProgressBar> ().SetProgress((captureProgress / (float)controller.captureTime) * 100f);

			if (captureProgress >= controller.captureTime) {
				captureProgress = 0;
				controller.TowerCaptured (tower, team);
				progressBarBase.SetActive (false);
			}
		} else {
			// Friendly - change direction.
			// Can't change without direction.
			if (horiz == 0f && vert == 0f) return;

			GameController.LaserDirection dir;
			if (Mathf.Abs (horiz) > Mathf.Abs (vert)) {
				if (horiz > 0) dir = GameController.LaserDirection.RIGHT; else dir = GameController.LaserDirection.LEFT;
			} else {
				if (vert > 0) dir = GameController.LaserDirection.UP; else dir = GameController.LaserDirection.DOWN;
			}
			controller.RotateTower (tower, dir);
		}
	}

	void DoMovement(float horiz, float vert) {
		Vector2 motion = new Vector2 (horiz, vert);
		rigidbody2d.AddForce (motion * speed);
	}

	void OnTriggerEnter2D (Collider2D other) {
		currentCollision = other;
	}

	void OnTriggerExit2D (Collider2D other) {
		currentCollision = null;
		captureProgress = 0;
		progressBarBase.SetActive (false);
	}
}
