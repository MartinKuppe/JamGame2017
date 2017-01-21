using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwissArmyKnife;

public class Hero : Singleton<Hero> {

	// Use this for initialization
	void Awake () {
        //KeyWatcher.OnKeyAcknowledged += EvaluateMovement;
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    /*private void EvaluateMovement()
    {
        if (moveScript.isMoving) return;
        
        Direction dir = Waypoint.InterpretKeys();
        if (dir == Direction.None) return;

        Waypoint directWaypoint = moveScript.currentWaypoint.FindInDirection(dir);
        if(directWaypoint != null)
        {
            moveScript.GoToWaypoint(directWaypoint);
        }
    }*/

    private void DebugMethod()
    {
    }
}
