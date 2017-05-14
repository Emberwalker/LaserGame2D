using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Main game state controller.
 */
public class GameController : MonoBehaviour {

	// Team colours
	public static readonly Color RED_COLOR = new Color(182, 0, 0);
	public static readonly Color BLUE_COLOR = new Color(0, 181, 255);
	public static readonly Color NEUTRAL_COLOR = new Color(126, 126, 126);

	// Factions


	public enum PlayerTeam {
		Nobody = 0,
		Red = 1,
		Blue = 2,
	}

	public enum LaserDirection {
		UP = 0,
		RIGHT = 270,
		LEFT = 90,
		DOWN = 180,
		NONE = -1
	}

	// Because Unity inexplicably doesn't have .NET Tuples.
	private class PlayGridLoc {
		public TowerController tower;
		public LaserDirection direction;
		public PlayerTeam team;

		public PlayGridLoc(TowerController t, LaserDirection d, PlayerTeam te) {
			tower = t;
			direction = d;
			team = te;
		}
	}

	public int gridRows;
	public int gridCols;
	public int captureTime;

	private PlayGridLoc[,] playGrid;
	private EmitterController[] emittersRed;
	private EmitterController[] emittersBlue;
	private int activeEmitterRed;
	private int activeEmitterBlue;

	// Use this for initialization
	void Start () {
		int defaultEmitterRow = Mathf.RoundToInt (gridRows / 2f) - 1;
		Debug.Log ("Using row " + defaultEmitterRow + " as default emitter row.");
		activeEmitterRed = defaultEmitterRow;
		activeEmitterBlue = defaultEmitterRow;
	}

	void RecalculateGrid () {
		bool[,] redTrace = TraceRouteThroughGrid (PlayerTeam.Red);
		bool[,] blueTrace = TraceRouteThroughGrid (PlayerTeam.Blue);
		bool[,,] newLaserStates = MergeTraces (redTrace, blueTrace);
		UpdateTowerRenders(newLaserStates);
	}

	// Returns bool[row,col] = laser on/off, calls EndOfGame if someone wins.
	private bool[,] TraceRouteThroughGrid(PlayerTeam team) {
		int row, col;
		if (team == PlayerTeam.Red) {
			row = activeEmitterRed;
			col = 0;
		} else {
			row = activeEmitterBlue;
			col = gridCols - 1;
		}

		// Red and Blue active bools for each tower
		bool[,] laserStates = new bool[gridRows, gridCols];
		// Loop detection - store towers we've "seen"
		bool[,] seenTowers = new bool[gridRows, gridCols];

		while (true) {
			if (row < 0 || row >= gridRows) {
				Debug.Log ("Trace went off play grid - bailing.");
				break;
			}

			if (col < 0) {
				if (team == PlayerTeam.Blue) {
					EndOfGame (PlayerTeam.Blue);
				} else {
					Debug.Log ("Red team hit own emitter - bailing.");
				}
				break;
			}

			if (col >= gridCols) {
				if (team == PlayerTeam.Red) {
					EndOfGame (PlayerTeam.Red);
				} else {
					Debug.Log ("Blue team hit own emitter - bailing.");
				}
				break;
			}

			if (seenTowers[row, col]) {
				Debug.Log ("Loop detected in TraceRouteThroughGrid - bailing.");
				break; // Avoid infinite loop.
			}

			seenTowers[row, col] = true;
			PlayGridLoc loc = playGrid[row, col];
			Debug.Log ("TRACE: " + team.ToString () + " " + row + "/" + col + " " + loc.direction.ToString ());
			if (loc.direction == LaserDirection.NONE) break;
			laserStates[row, col] = true;

			switch (loc.direction) {
				case LaserDirection.UP:
					row -= 1;
					break;
				case LaserDirection.RIGHT:
					col += 1;
					break;
				case LaserDirection.DOWN:
					row += 1;
					break;
				case LaserDirection.LEFT:
					col -= 1;
					break;
				default:
					throw new ArgumentOutOfRangeException(); // Should be impossible.
			}
		}

		return laserStates;
	}

	private bool[,,] MergeTraces(bool[,] red, bool[,] blue) {
		bool[,,] res = new bool[gridRows, gridCols, 2];
		for (int i = 0; i < gridRows; i++) {
			for (int j = 0; j < gridCols; j++) {
				res[i,j,0] = red[i,j];
				res[i,j,1] = blue[i,j];
			}
		}
		return res;
	}

	private void UpdateTowerRenders(bool[,,] state) {
		for (int i = 0; i < gridRows; i++) {
			for (int j = 0; j < gridCols; j++) {
				PlayGridLoc loc = playGrid[i,j];
				loc.tower.UpdateLasers (state[i,j,0], state[i,j,1], loc.direction);
			}
		}
	}

	private void EndOfGame(PlayerTeam winner) {
		// TODO
		Debug.Log ("WIN: " + winner.ToString ());
	}

	public void RegisterTower (TowerController tower, int x, int y) {
		if (playGrid == null) playGrid = new PlayGridLoc[gridRows,gridCols];
		playGrid[x,y] = new PlayGridLoc(tower, LaserDirection.NONE, PlayerTeam.Nobody);
	}

	public void RegisterEmitter (EmitterController emitter, int row, PlayerTeam team) {
		if (emittersRed == null) emittersRed = new EmitterController[3];
		if (emittersBlue == null) emittersBlue = new EmitterController[3];

		if (team == PlayerTeam.Red) {
			emittersRed[row] = emitter;
		} else {
			emittersBlue[row] = emitter;
		}
	}

	public void ChangeActiveEmitter (int row, PlayerTeam team) {
		EmitterController[] emitters;
		int active;
		if (team == PlayerTeam.Red) {
			emitters = emittersRed;
			active = activeEmitterRed;
			activeEmitterRed = row;
		} else {
			emitters = emittersBlue;
			active = activeEmitterBlue;
			activeEmitterBlue = row;
		}

		// Nothing to do.
		if (active == row) return;

		// Change emitter and recalc.
		Debug.Log ("Changing emitter to row " + row + " (from row " + active + ") for team " + team.ToString());
		emitters[active].SetActive (false);
		emitters[row].SetActive (true);
		RecalculateGrid ();
	}

	public PlayerTeam GetTowerOwner (TowerController tower) {
		PlayGridLoc loc = playGrid[tower.gridRow, tower.gridCol];
		return loc.team;
	}

	public void TowerCaptured (TowerController tower, PlayerTeam team) {
		Debug.Log ("Tower " + tower + "captured! " + tower.gridRow + "/" + tower.gridCol + " by " + team.ToString ());
		PlayGridLoc loc = playGrid[tower.gridRow, tower.gridCol];
		loc.team = team;
		tower.OnCapture (team);
		RecalculateGrid ();
	}

	public void RotateTower (TowerController tower, LaserDirection direction) {
		PlayGridLoc loc = playGrid[tower.gridRow, tower.gridCol];
		if (loc.direction == direction) return; // Nothing to do. Skip recalculation.
		loc.direction = direction;
		RecalculateGrid ();
	}

	public static GameController FindController() {
		return GameObject.Find ("GameController").GetComponent<GameController>();
	}
}
