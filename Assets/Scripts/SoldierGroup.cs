using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



///=================================================================================================================
///                                                                                                       <summary>
///  SoldierGroup is a group of one or two soldiers, moving along a road.  								 </summary>
///
///=================================================================================================================
public class SoldierGroup : MonoBehaviour 
{
    public SpriteRenderer[] _spriteRenderers;
    public SpriteRenderer[] _minimapSpriteRenderers;
    public Sprite _loyalistAntSprite;
    public Sprite _rebelAntSprite;
    private Affiliation _affiliation;
    private int _size; // 1 or 2
    private List<Node> _path;
    private int _nextWaypointIndex;
    public static float SPEED = 0.25f;
    public static float SPRITE_DISTANCE = 0.2f;
    public bool? _freezeAI;

    public void Deploy( Affiliation affiliation, int size, List<Node> path , bool? freezeAI )
    {
        _affiliation = affiliation;
        _size = size;
        _path = path;
        _freezeAI = freezeAI;

        // Display
        Sprite sprite = (_affiliation == Affiliation.Loyalists) ? _loyalistAntSprite : _rebelAntSprite;
        for ( int i = 0; i < _spriteRenderers.Length; i++ )
        {
            _spriteRenderers[i].gameObject.SetActive(i < _size);
            _spriteRenderers[i].sprite = sprite;
            _minimapSpriteRenderers[i].sprite = sprite;
        }

        // Position on path
        _nextWaypointIndex = 1;
        transform.position = path[0]._position;
        PlaceSprites();
    }

    private void PlaceSprites()
    {
        switch( _size )
        {
            case 1:
                _spriteRenderers[0].transform.position = transform.position;
                break;

            case 2:
                Vector3 forward = (_path[_nextWaypointIndex]._position - transform.position).normalized;
                Vector3 sidewards = new Vector3(forward.y, -forward.x) * SPRITE_DISTANCE * 0.5f;
                _spriteRenderers[0].transform.position = transform.position + sidewards;
                _spriteRenderers[1].transform.position = transform.position - sidewards;
                break;
        }
    }

    public void Update()
    {
        TestIfPlayerNear();

        if ( Move() )
        {
            // We have arrived
            City city = _path[_path.Count - 1]._waypoint._node._city;
            city.ReceiveTroops(_size, _affiliation, _freezeAI);

            Destroy(gameObject);
        }
    }

    private bool Move()
    {
        // How fare do we move this time ?
        float fDistanceThisFrame = Time.deltaTime * SPEED;

        if (fDistanceThisFrame == 0)
        {
            // We won't move this frame.
            return false;
        }

        Vector3 vNextWaypoint = _path[_nextWaypointIndex]._position;

        //Debug.DrawLine(transform.position, vNextWaypoint, Color.red, 10.0f);

        float fDistance = (vNextWaypoint - transform.position).magnitude;

        bool arrived = false;
        while (!arrived && fDistanceThisFrame > fDistance)
        {

            transform.position = vNextWaypoint;
            _nextWaypointIndex++;

            if (_nextWaypointIndex >= _path.Count)
            {
                // We have arrived
                arrived = true;
            }
            else
            {
                // Procede to next waypoint
                fDistanceThisFrame -= fDistance;
                vNextWaypoint = _path[_nextWaypointIndex]._position;
                fDistance = (vNextWaypoint - transform.position).magnitude;
            }
        }

        if( !arrived )
        {
            transform.position += fDistanceThisFrame * (vNextWaypoint - transform.position).normalized;
            PlaceSprites();
        }
        return arrived;
    }

    private const float ARREST_PLAYER_DISTANCE = 0.5f;

    private void TestIfPlayerNear()
    {
        if( _affiliation == Affiliation.Loyalists && (transform.position - Hero.Instance.transform.position).magnitude < ARREST_PLAYER_DISTANCE )
        {
            VictoryManager.Instance.GameOver("You were caught by an enemy patrol.");
        }
    }
}
