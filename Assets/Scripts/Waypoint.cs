using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    N = 0,
    E = 2,
    S = 3,
    W = 1,
    NE = 5,
    NW = 4,
    SW = 7,
    SE = 8,
    None = 10
}

public class Waypoint : MonoBehaviour {

    public List<Waypoint> previousWaypoints = new List<Waypoint>();
    public List<Waypoint> nextWaypoints = new List<Waypoint>();

    public Node _node;

    public List<Waypoint> LinkedWaypoints
    {
        get
        {
            List<Waypoint> res = previousWaypoints;
            res.AddRange(nextWaypoints);
            return res;
        }
    }

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    private void Awake()
    {
        foreach(Waypoint prev in previousWaypoints)
        {
            if (!prev.nextWaypoints.Contains(this)) prev.nextWaypoints.Add(this);
        }
    }

    public Waypoint FindInDirection(Direction dir)
    {
        Vector3 vecDir = InterpretDirection(dir);

        Ray manta = new Ray(transform.position, vecDir);
        RaycastHit hit;

        Debug.DrawLine(transform.position, transform.position + (vecDir * 1000f), Color.red, 10f);

        if (Physics.Raycast(manta, out hit, 1000.0f))
        {
            if(hit.collider.gameObject.GetComponent<Waypoint>() != null && LinkedWaypoints.Contains(hit.collider.gameObject.GetComponent<Waypoint>())) 
            {
                return hit.collider.gameObject.GetComponent<Waypoint>();
            }
        }

        return null;
    }

    public static Direction InterpretKeys()
    {
        //up, left, right, down
        bool[] arrowsInput = new bool[4];
        arrowsInput[0] = Input.GetKey(KeyCode.UpArrow);
        arrowsInput[1] = Input.GetKey(KeyCode.LeftArrow);
        arrowsInput[2] = Input.GetKey(KeyCode.RightArrow);
        arrowsInput[3] = Input.GetKey(KeyCode.DownArrow);

        int count = 0;
        foreach(bool b in arrowsInput)
        {
            if (b) count++;
        }

        if(count == 1)
        {
            for (int i = 0; i < arrowsInput.Length; i++)
            {
                if(arrowsInput[i])
                {
                    return (Direction)i;
                }
            }
        }
        else if (count == 2)
        {
            List<int> values = new List<int>();
            for (int i = 0; i < arrowsInput.Length; i++)
            {
                if (arrowsInput[i]) values.Add(i);
            }

            return (Direction)(values[0] + values[1] + 3);
        }

        return Direction.None;
    }

    public static Vector2 InterpretDirection(Direction dir)
    {
        Vector2 res = Vector2.zero;

        switch(dir)
        {
            case Direction.N:
                res = new Vector2(0, 1);
                break;
            case Direction.S:
                res = new Vector2(0, -1);
                break;
            case Direction.E:
                res = new Vector2(1, 0);
                break;
            case Direction.W:
                res = new Vector2(-1, 0);
                break;
            case Direction.NE:
                res = new Vector2(1, 1);
                break;
            case Direction.NW:
                res = new Vector2(-1, 1);
                break;
            case Direction.SE:
                res = new Vector2(1, -1);
                break;
            case Direction.SW:
                res = new Vector2(-1, -1);
                break;
        }

        return res;
    }

    /*
    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        // For debug purposes
        foreach (Waypoint waypoint in nextWaypoints)
        {
            Debug.DrawLine(transform.position, 0.5f * transform.position + 0.5f * waypoint.transform.position, Color.magenta);
        }
        foreach (Waypoint waypoint in previousWaypoints)
        {
            Debug.DrawLine(transform.position, 0.5f * transform.position + 0.5f * waypoint.transform.position, Color.red);
        }
    }
    */
}
