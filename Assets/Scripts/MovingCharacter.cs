using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCharacter : MonoBehaviour {

    public Waypoint currentWaypoint;
    public bool isMoving = false;
    private const float MOVE_DURATION = 1.0f;

	// Use this for initialization
	void Awake () {
        if(currentWaypoint != null) transform.position = currentWaypoint.Position;
	}

    public void GoToWaypoint(Waypoint destination)
    {
        StopAllCoroutines();
        StartCoroutine(MovementRoutine(destination));
    }

    public void InitiateProgression(KeyCode advanceKey, KeyCode backKey, Waypoint destination)
    {

    }
    
	
	private IEnumerator MovementRoutine(Waypoint destination)
    {
        float time = 0;
        Vector3 oriPos = transform.position;
        Vector3 destPos = destination.Position;
        isMoving = true;

        while(time <= MOVE_DURATION)
        {
            Vector3 newPos = Vector3.Lerp(oriPos, destPos, time / MOVE_DURATION);
            transform.position = newPos;

            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        transform.position = destPos;
        isMoving = false;
        currentWaypoint = destination;

        if(GetComponent<Hero>() != null)
        {
            KeyWatcher.ForceAck();
        }
    }
}
