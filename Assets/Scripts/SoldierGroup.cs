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
    public Sprite _loyalistSploutchSprite;
    public Sprite _rebelSploutchSprite;
    private Affiliation _affiliation;
    private int _size; // 1 or 2
    private List<Node> _path;
    private int _nextWaypointIndex;
    public static float SPEED = 0.25f;
    public static float SPRITE_DISTANCE = 0.2f;
    public bool? _freezeAI;
    public SoldierGroup _groupBehindMe;
    public SoldierGroup _groupBeforeMe;

    private bool _isDead = false;

    public void Deploy( Affiliation affiliation, int size, List<Node> path , bool? freezeAI, SoldierGroup groupInFront = null )
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

        // Register on first connection
        path[0].RegisterSoldierGroup(path[1], this);

        // Link groups
        if(groupInFront != null)
        {
            _groupBeforeMe = groupInFront;
            groupInFront._groupBehindMe = this;  // Needed to pass on lock in case of death
        }
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
        if (_isDead) return;

        TestIfPlayerNear();
        HandleEnemyEncounters();

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
            // Unregister on last connection
            _path[_nextWaypointIndex-1].UnregisterSoldierGroup(_path[_nextWaypointIndex], this);

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

                // Register on next connection
                _path[_nextWaypointIndex - 1].RegisterSoldierGroup(_path[_nextWaypointIndex], this);
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

    private IEnumerator Kill()
    {
        // Pass on freeze to group behind ?
        if( _freezeAI == true && _groupBehindMe != null )
        {
            if(_groupBehindMe._freezeAI == null)
            {
                // Pass on freeze
                _groupBehindMe._freezeAI = true;
            }
            else
            {
                // Last group remainaing - eliminate unfreeze
                _groupBehindMe._freezeAI = null;
            }

        }
        // Unregister from path segment
        _path[_nextWaypointIndex - 1].UnregisterSoldierGroup(_path[_nextWaypointIndex], this);

        // Un-announce my arrival at target city
        _path[_path.Count - 1]._city.OnGroupKilledOnTheirWayHere(_affiliation, _size);

        // Display sploutch
        Sprite sprite = (_affiliation == Affiliation.Loyalists) ? _loyalistSploutchSprite : _rebelSploutchSprite;
        _spriteRenderers[0].sprite = sprite;
        _spriteRenderers[1].sprite = sprite;

        // make sure sprites are displayed below living ants
        _spriteRenderers[0].sortingOrder = 0;
        _spriteRenderers[1].sortingOrder = 0;

        // Freeze AI
        _isDead = true;

        // fade out
        float timer = 1.0f;
        while (timer > 0 )
        {
            yield return new WaitForEndOfFrame();
            timer -= Time.deltaTime;
            Color alpha = new Color(1, 1, 1, timer);
            _spriteRenderers[0].color = alpha;
            _spriteRenderers[1].color = alpha;
        }

        // Die
        Destroy(gameObject);
    }


    private void HandleEnemyEncounters()
    {
        // Check all groups on the same segment, in opposite direction
        foreach( SoldierGroup enconteredGroup  in _path[_nextWaypointIndex].GroupsGoingTo(_path[_nextWaypointIndex-1]))
        {
            if(enconteredGroup._affiliation != _affiliation 
                && (transform.position - enconteredGroup.transform.position).magnitude <= SPRITE_DISTANCE )
            {
                // Duel to the death - what are the odds?
                float oddsToWin = (float)_size / (_size + enconteredGroup._size);
                oddsToWin *= oddsToWin;

                // Who wins?
                if ( Random.value < oddsToWin )
                {
                    // We win
                    enconteredGroup.StartCoroutine( Kill() );
                }
                else
                {
                    // They win
                    this.StartCoroutine(Kill());
                }
                return;
            }
        }
    }
}
