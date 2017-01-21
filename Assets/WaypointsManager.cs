using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwissArmyKnife;

public class WaypointsManager : Singleton<WaypointsManager> {

    public List<Waypoint> allWaypoints = new List<Waypoint>();

    public Transform wallsParent;
    public GameObject wallPrefab;

    private void Awake()
    {
        allWaypoints = new List<Waypoint>(Object.FindObjectsOfType<Waypoint>());
        //BuildWalls();
    }
    
    private void BuildWalls()
    {
        foreach(Waypoint w in allWaypoints)
        {
            if(w.nextWaypoints.Count > 0)
            {
                for (int i = 0; i < w.nextWaypoints.Count; i++)
                {
                    Waypoint next = w.nextWaypoints[i];
                    Vector3 midPos = Vector3.Lerp(w.transform.position, next.transform.position, 0.5f);
                    float distance = Vector3.Distance(w.transform.position, next.transform.position);

                    GameObject newWall = Instantiate(wallPrefab) as GameObject;

                    Quaternion lookRot = Quaternion.LookRotation(w.transform.position, next.transform.position);

                    newWall.transform.rotation = lookRot;
                    newWall.transform.localScale = new Vector3(distance - 0.5f, 1, 1);

                    newWall.transform.position = midPos;
                    newWall.transform.SetParent(wallsParent);
                }
            }
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
