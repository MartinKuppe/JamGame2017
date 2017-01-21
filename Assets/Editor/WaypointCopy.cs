using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaypointCopy : MonoBehaviour {
    
    // Add a menu item named "Do Something with a Shortcut Key" to MyMenu in the menu bar
    // and give it a shortcut (ctrl-g on Windows, cmd-g on macOS).
    [MenuItem("Pathfinding/Duplicate Waypoint %g")]
    static void DoSomethingWithAShortcutKey()
    {
        GameObject activeObject = Selection.activeGameObject;
        if (activeObject == null || activeObject.GetComponent<Waypoint>() == null) return;

        Waypoint currentWaypoint = activeObject.GetComponent<Waypoint>();

        Object prefabRoot = PrefabUtility.GetPrefabParent(Selection.activeGameObject);
        GameObject newOb = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;

        newOb.name = "WP";
        newOb.transform.SetParent(activeObject.transform.parent);
        newOb.transform.position = activeObject.transform.position;
        
        Waypoint newWP = newOb.GetComponent<Waypoint>();

        currentWaypoint.nextWaypoints.Add(newWP);

        newWP.previousWaypoints.Clear();
        newWP.nextWaypoints.Clear();
        newWP.previousWaypoints.Add(currentWaypoint);
        
        Selection.activeObject = newOb;
    }
}
