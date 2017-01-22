using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;

public class NodeConnection
{
    public List<SoldierGroup> _soldierGroups = new List<SoldierGroup>();
}

///=================================================================================================================
///                                                                                                       <summary>
///  Node is a waypoint in the pathfinding graph. It has been separated from Waypoint to avoid merge conflicts.	 </summary>
///
///=================================================================================================================
public class Node
{
    public Waypoint _waypoint;
    public Vector3 _position;
    public List<Node> _neighbours = new List<Node>();
    public bool _closed = false;
    public float _f;
    public float _g;
    public Node _cameFrom;
    public City _city;

    private int _index;
    private static int _nextFreeIndex = 0;
    private Dictionary<int, NodeConnection> _nodeConnectionsByTargetIndex = new Dictionary<int, NodeConnection>();

    public Node( Waypoint waypoint )
    {
        _position = waypoint.transform.position;
        _waypoint = waypoint;
        _waypoint._node = this;
        _index = _nextFreeIndex++;
        waypoint.name = "Waypoint " + _index;
    }

    public void LinkTo(Node otherNode)
    {
        if (_neighbours.Contains(otherNode)) return;

        // Link the nodes
        _neighbours.Add(otherNode);
        otherNode._neighbours.Add(this);

        // Initialize node connections
        NodeConnection to = new NodeConnection();
        NodeConnection fro = new NodeConnection();
        _nodeConnectionsByTargetIndex[otherNode._index] = to;
        otherNode._nodeConnectionsByTargetIndex[_index] = fro;
    }

    public float HeuristicDistanceTo( Node otherNode)
    {
        // Manhattan metrics
        return Mathf.Abs(_position.x - otherNode._position.x) + Mathf.Abs(_position.y - otherNode._position.y);
    }

    public void RegisterSoldierGroup( Node targetNode , SoldierGroup group )
    {
        _nodeConnectionsByTargetIndex[targetNode._index]._soldierGroups.Add(group);
    }

    public void UnregisterSoldierGroup(Node targetNode, SoldierGroup group)
    {
        _nodeConnectionsByTargetIndex[targetNode._index]._soldierGroups.Remove(group);
    }

    public List<SoldierGroup> GroupsGoingTo(Node targetNode)
    {
        return _nodeConnectionsByTargetIndex[targetNode._index]._soldierGroups;
    }
}


///=================================================================================================================
///                                                                                                       <summary>
///  Pathfinder is a class that implements A*, finding paths between nodes.								 </summary>
///
///=================================================================================================================
public class Pathfinder : Singleton<Pathfinder> 
{
    private Node[] _nodes;

    private void Awake()
    {
        CreateGraph();
    }

    private void CreateGraph()
    {
        Waypoint[] waypoints = FindObjectsOfType<Waypoint>();
        _nodes = new Node[waypoints.Length];

        // Create nodes
        for( int i = 0; i < _nodes.Length; i++ )
        {
            _nodes[i] = new Node(waypoints[i]);
        }

        // Create links
        for (int i = 0; i < _nodes.Length; i++)
        {
            foreach(Waypoint neighbour in waypoints[i].nextWaypoints)
            {
                waypoints[i]._node.LinkTo(neighbour._node);
            }
            foreach (Waypoint neighbour in waypoints[i].previousWaypoints)
            {
                waypoints[i]._node.LinkTo(neighbour._node);
            }
        }
    }

    private void Update()
    {
        //DebugDisplay();
    }

    private void DebugDisplay()
    {
        for (int i = 0; i < _nodes.Length; i++)
        {
            foreach (Node neighbour in _nodes[i]._neighbours)
            {
                Debug.DrawLine(_nodes[i]._position, 0.5f * _nodes[i]._position + 0.5f * neighbour._position, Color.cyan);
            }
        }
    }

    public List<Node> FindPath( Node start, Node destination, Affiliation affiliation )
    {
        // Prepare nodes
        List<Node> openNodes = new List<Node>();
        openNodes.Add(start);
        for (int i = 0; i < _nodes.Length; i++)
        {
            Node node = _nodes[i];
            node._closed = false;
            node._f = Mathf.Infinity;
            node._g = Mathf.Infinity;
            node._cameFrom = null;
        }
        start._g = 0.0f;
        start._f= (start._position - destination._position).magnitude;

        int securityCounter = 0;
        while(openNodes.Count > 0 && securityCounter++ < 10000)
        {
            // Find node with smallest _f
            Node currentNode = null;
            float bestF = Mathf.Infinity;
            foreach( Node node in openNodes)
            {
                if( node._f < bestF )
                {
                    bestF = node._f;
                    currentNode = node;
                }
            }

            // Are we at the destination ?
            if ( currentNode == destination)
            {
                return ReconstructPathTo(destination);
            }

            // Close current
            openNodes.Remove(currentNode);
            currentNode._closed = true;

            // Check neighbours
            foreach ( Node neighbour in currentNode._neighbours)
            {
                if (neighbour._closed) continue;

                if (neighbour._city != null 
                    && neighbour != destination
                    && neighbour._city._occupyingFaction != affiliation) continue; // Don't pass in front of enemy cities

                float tentativeG = currentNode._g + currentNode.HeuristicDistanceTo(neighbour);
                if( !openNodes.Contains(neighbour))
                {
                    openNodes.Add(neighbour);
                }
                else if( tentativeG >= neighbour._g)
                {
                    continue;
                }

                // This path is the best until now. Record it!
                neighbour._cameFrom = currentNode;
                neighbour._g = tentativeG;
                neighbour._f = neighbour._g + neighbour.HeuristicDistanceTo(destination);
            }

        }

        // Failure
        Debug.Break();
        return null;



    }

    private List<Node> ReconstructPathTo( Node node )
    {
        List<Node> list = new List<Node>();
        list.Add(node);
        while( node._cameFrom != null )
        {
            node = node._cameFrom;
            list.Insert(0, node);
        }
        return list;
    }

}

