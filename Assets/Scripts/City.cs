using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Affiliation { Loyalists = 0, Rebels = 1 };

///=================================================================================================================
///                                                                                                       <summary>
///  City is a script that handles all about a city, including UI and AI. 
///  I know, it's kinda ugly to put everything in one class - but this is a GameJam.                     </summary>
///
///=================================================================================================================
public class City : MonoBehaviour
{
    [Header("Politics")]
    public int _rebelSupport = 0;     // de 0 à 3;
    public Affiliation _occupyingFaction;
    public int _occupyingForces = 10;
    public int _attackingForces = 0;
    public Waypoint _waypoint;

    [Header("UI")]
    public Sprite _loyalistAntSprite;
    public Sprite _rebelAntSprite;
    public Sprite _deadRedAntSprite;
    public Sprite _deadBlackAntSprite;
    public Image[] _flags;
    public Image[] _occupyingForcesImages;
    public Image[] _attackingForcesImages;
    public Image _factionIndicator;

    private const float TICK_TIME = 0.5f;
    private float _tickTimer = 0.0f;

    public City[] _neighbourCities;

    public SoldierGroup _soldierGroupPrefab;

    // For AI  
    private int _menacingForces;
    private int _distanceToEnemy;
    private int _weakestEnemyForces;
    private City _weakestEnemy;
    private int _weakestFriendNeededReinforcement;
    private City _weakestFriend;
    private int _indispensableForces;
    private int _dispensableForces;
    private int _loyalistTroopsOnTheirWayHere;
    private int _rebelTroopsOnTheirWayHere;
    private const int MINIMAL_OCCUPATION_FORCE = 3;
    private const int ATTACK_TRESHOLD = 5;
    private const float MENACE_DEPTH_FACTOR = 0.3f;
    private const int MAX_TROOPS = 20;

    private int _freezeMultiplicity = 0;

    // ---------------------------------------------------- <summary>
    // The start function.
    // ----------------------------------------------------
    void Start()
    {
        // Not optimal
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
        float minDistance = Mathf.Infinity;
        foreach( Waypoint waypoint in allWaypoints )
        {
            float distance = (waypoint.transform.position - transform.position).magnitude;
            if( distance < minDistance)
            {
                minDistance = distance;
                _waypoint = waypoint;
            }
        }
        _waypoint.name += name.Replace("City", "");
        _waypoint._node._city = this;

    }

    // ---------------------------------------------------- <summary>
    // The update function.
    // ----------------------------------------------------
    void Update()
    {
        if (IsAIFrozen() ) return;

        // Every once in a while, call TickUpdate 
        _tickTimer -= Time.deltaTime;
        if (_tickTimer <= 0.0f)
        {
            _tickTimer = TICK_TIME;
            TickUpdate();
        }

    }

    // ---------------------------------------------------- <summary>
    // Kind of a low-frequency update function.             </summary>
    // ----------------------------------------------------
    void TickUpdate()
    {
        // Update UI
        Populate();

        // Are we at war ?
        if (IsUnderAttack())
        {
            // handle combat
            UpdateAttack();
        }
        else
        {
            // TODO: Draft troops
            
            // TODO: Indoctrinate population 

            // AI stuff
            UpdateStrategicInfo();
            MakeStrategicDecisions();
        }
    }

    // ---------------------------------------------------- <summary>
    // is this city under siege ?                           </summary>
    // ----------------------------------------------------
    public bool IsUnderAttack()
    {
        return (_attackingForces > 0);
    }

    // ---------------------------------------------------- <summary>
    // Refresh the UI                                     </summary>
    // ----------------------------------------------------
    public void Populate()
    {
        // Colorize flags and faction indicator
        for (int i = 0; i < _flags.Length; i++)
        {
            _flags[i].color = (i < _rebelSupport) ? Color.red : Color.black;
        }
        _factionIndicator.color = (_occupyingFaction == Affiliation.Rebels) ? Color.red : Color.black;

        // Update occupying troops
        Sprite spriteUs = (_occupyingFaction == Affiliation.Loyalists) ? _loyalistAntSprite : _rebelAntSprite;
        for (int i = 0; i < _occupyingForcesImages.Length; i++)
        {
            if (i < _occupyingForces)
            {
                _occupyingForcesImages[i].gameObject.SetActive(true);
                _occupyingForcesImages[i].sprite = spriteUs;
            }
            else
            {
                _occupyingForcesImages[i].gameObject.SetActive(false);
            }
        }

        // Update attacking troops
        Sprite spriteThem = (_occupyingFaction == Affiliation.Loyalists) ? _rebelAntSprite : _loyalistAntSprite;
        for (int i = 0; i < _attackingForcesImages.Length; i++)
        {
            if (i < _attackingForces)
            {
                _attackingForcesImages[i].gameObject.SetActive(true);
                _attackingForcesImages[i].sprite = spriteThem;
            }
            else
            {
                _attackingForcesImages[i].gameObject.SetActive(false);
            }
        }
    }

    // ---------------------------------------------------- <summary>
    //Called every tick when besieged                                    </summary>
    // ----------------------------------------------------
    private void UpdateAttack()
    {
        float attackerOdds = Mathf.Clamp((float)_attackingForces / (float)(_attackingForces / _occupyingForces), 0.4f, 0.6f);

        if (Random.value <= attackerOdds)
        {
            // Attacker kills one defender
            _occupyingForces--;

            // Display dead ant symbol
            _occupyingForcesImages[Random.Range(0, _occupyingForces + 1)].sprite = (_occupyingFaction == Affiliation.Loyalists) ? _deadBlackAntSprite : _deadRedAntSprite;

            // Did the attacker win ?
            if (_occupyingForces == 0)
            {
                // Attacker takes the city
                _occupyingForces = _attackingForces;
                _attackingForces = 0;
                _occupyingFaction = (Affiliation)(1 - (int)_occupyingFaction);
            }
        }
        else
        {
            // Defender kills one attacker
            _attackingForces--;

            // Display dead ant symbol
            _attackingForcesImages[Random.Range(0, _attackingForces + 1)].sprite = (_occupyingFaction == Affiliation.Loyalists) ? _deadRedAntSprite : _deadBlackAntSprite;
        }
    }



    // ---------------------------------------------------- <summary>
    // Gather information for AI                                   </summary>
    // ----------------------------------------------------
    private void UpdateStrategicInfo()
    {
        // Count menacing forces
        _menacingForces = 0;
        int oldDistanceToEnemy = _distanceToEnemy;
        _distanceToEnemy = 99999;
        _weakestEnemyForces = 9999999;
        _weakestFriendNeededReinforcement = 0;
        _weakestFriend = null;
        DEBUG_INFO = "";
        MyDebugLog("---Updating strategy");
        foreach (City neighbour in _neighbourCities)
        {
            if (neighbour.IsUnderAttack())
            {
                //TODO: Send reinfocements ?
            }
            else if (neighbour._occupyingFaction != _occupyingFaction)
            {
                // Enemy city - consider forces as menace (minus MINIMAL_OCCUPATION_FORCE)
                _distanceToEnemy = 1;
                int menace = neighbour._occupyingForces - MINIMAL_OCCUPATION_FORCE;
                int defense = neighbour._occupyingForces;
                _menacingForces += Mathf.Max(0, menace);

                // Check if this enemy is worth attacking
                if (defense < _weakestEnemyForces)
                {
                    _weakestEnemyForces = defense;
                    _weakestEnemy = neighbour;
                }

            }
            else
            {
                MyDebugLog("   ---Checking " + neighbour.name);

                // Allied city - forces that menace that city might possibly come here
                _distanceToEnemy = Mathf.Min(_distanceToEnemy, neighbour._distanceToEnemy + 1);
                if (neighbour._distanceToEnemy < _distanceToEnemy)
                {
                    _menacingForces += Mathf.RoundToInt(neighbour._menacingForces * MENACE_DEPTH_FACTOR);
                }

                // Check if this friend needs reinforcments
                int slowMigrationToFront = (neighbour._distanceToEnemy < oldDistanceToEnemy) ? 2 : 0;
                int neededReinforcements = Mathf.Max(slowMigrationToFront, neighbour._menacingForces - neighbour._occupyingForces);

                MyDebugLog("neighbour._distanceToEnemy = " + neighbour._distanceToEnemy);
                MyDebugLog("_distanceToEnemy = " + oldDistanceToEnemy);
                MyDebugLog("slowMigrationToFront = " + slowMigrationToFront);
                MyDebugLog("neededReinforcements = " + neededReinforcements);

                if (neededReinforcements > _weakestFriendNeededReinforcement)
                {
                    _weakestFriendNeededReinforcement = neededReinforcements;
                    _weakestFriend = neighbour;

                    MyDebugLog("_weakestFriendNeededReinforcement = neededReinforcements");
                }
            }
        }
    }

    public string DEBUG_INFO = "";

    // ---------------------------------------------------- <summary>
    // Decide where to send troops                                   </summary>
    // ----------------------------------------------------
    private void MakeStrategicDecisions()
    {
         _indispensableForces = Mathf.Max(MINIMAL_OCCUPATION_FORCE, _menacingForces);
         _dispensableForces = Mathf.Max(0, _occupyingForces - _indispensableForces);


        // Check attack opportunities
        if ( _dispensableForces >= _weakestEnemyForces + ATTACK_TRESHOLD)
        {
            DEBUG_INFO = " Attacking " + _weakestEnemy.name;
            SendTroopsTo(_dispensableForces, _weakestEnemy);
            return;
        }

        // Check neighbours in need
        if (_weakestFriend != null && _dispensableForces >= 0)
        {
            DEBUG_INFO = " Reinforcing "+ _weakestFriend.name;
            SendTroopsTo(Mathf.Min(_dispensableForces, _weakestFriendNeededReinforcement), _weakestFriend);
            return;
        }
    }

    // ---------------------------------------------------- <summary>
    // Send troops to a city                                  </summary>
    // ----------------------------------------------------
    private void SendTroopsTo(int amount, City city)
    {
        // How many troops are already on their way to that city ?
        int troopsOnTheirWay = (_occupyingFaction == Affiliation.Loyalists) ? city._loyalistTroopsOnTheirWayHere : city._rebelTroopsOnTheirWayHere;

        // Make sure there's enough space in the target city for the troops
        if (city._occupyingFaction == _occupyingFaction)
        {
            // Make sure that occupying forces + amount + troops on their way <= MAX_TROOPS
            amount = Mathf.Min(amount, MAX_TROOPS - city._occupyingForces - troopsOnTheirWay);
        }
        else
        {
            // Make sure that attacking forces + amount + troops on their way <= MAX_TROOPS
            amount = Mathf.Min(amount, MAX_TROOPS - city._attackingForces - troopsOnTheirWay);
        }

        // Just to be sure we don't send a negative amount
        amount = Mathf.Max(0, amount);

        // Announce troops to target city
        if ( _occupyingFaction == Affiliation.Loyalists )
        {
            // Reserve space for loyalist troops
            city._loyalistTroopsOnTheirWayHere += amount;
        }
        else
        {
            // Reserve space for rebel troops
            city._rebelTroopsOnTheirWayHere += amount;
        }

        // Deploy troops
        List<Node> path = Pathfinder.Instance.FindPath(_waypoint._node, city._waypoint._node);
        StartCoroutine(DeployTroops(path, amount));

        // For debug purposes
        //DebugDisplayPathTo(city);
    }

    // ---------------------------------------------------- <summary>
    // Put troops on the map by groups of 2                 </summary>
    // ----------------------------------------------------
    private IEnumerator DeployTroops(List<Node> path, int totalAmount)
    {
        // Freeze AI of this city
        FreezeAI();

        int remainingAmount = totalAmount;
        while (remainingAmount > 0 )
        {
            // Determine of next group will freeze or unfreeze the AI
            bool? freezeAI = null;
            if (totalAmount > 2)
            {
                // First group will freeze AI of target city when it arrives 
                if (remainingAmount == totalAmount) freezeAI = true;

                // Last group will unfreeze AI of target city when it arrives
                if (remainingAmount <= 2) freezeAI = false;
            }

            // Deploy one or two soldiers
            int deployedNow = Mathf.Min(2, remainingAmount);
            SoldierGroup group = Instantiate<SoldierGroup>(_soldierGroupPrefab);
            group.Deploy(_occupyingFaction, deployedNow, path, freezeAI);

            // Update remainoing forces
            remainingAmount -= deployedNow;
            _occupyingForces -= deployedNow;

            // populate the UI (not done by UpdateTick because frozen)
            Populate();

            // Wait till the group is at a distance of SoldierGroup.SPRITE_DISTANCE
            yield return new WaitForSeconds( SoldierGroup.SPRITE_DISTANCE / SoldierGroup.SPEED);
        }

        // Unfreeze AI of this city
        UnfreezeAI();
    }

    // ---------------------------------------------------- <summary>
    // Some soldiers just entered the city.               </summary>
    // ----------------------------------------------------
    public void ReceiveTroops(int amount, Affiliation faction, bool? freezeAI )
    {
        // Reduce troops on their way
        if (faction == Affiliation.Loyalists)
        {
            _loyalistTroopsOnTheirWayHere -= amount;
        }
        else
        {
            _rebelTroopsOnTheirWayHere -= amount;
        }

        // freeze / unfreeze AI
        if(freezeAI == true)
        {
            FreezeAI();
        }
        else if( freezeAI == false)
        {
            UnfreezeAI();
        }

        //TODO: surplus troops look elsewhere
        if (faction == _occupyingFaction)
        {
            // Receiving reinforcements
            _occupyingForces = Mathf.Min(_occupyingForces + amount, MAX_TROOPS );
        }
        else
        {
            // Being attacked
            _attackingForces = Mathf.Min(_attackingForces + amount, MAX_TROOPS);
        }
        Populate();
    }

    // ---------------------------------------------------- <summary>
    // Freeze the AI.                                        </summary>
    // ----------------------------------------------------
    public void FreezeAI()
    {
        _freezeMultiplicity++;
    }

    // ---------------------------------------------------- <summary>
    // Unfreeze the AI (except if it was frozen several times)  </summary>
    // ----------------------------------------------------
    public void UnfreezeAI()
    {
        _freezeMultiplicity--;
    }

    // ---------------------------------------------------- <summary>
    // is the AI frozen ?                                     </summary>
    // ----------------------------------------------------
    public bool IsAIFrozen()
    {
        return (_freezeMultiplicity>0);
    }

    // ---------------------------------------------------- <summary>
    // Draw debug stuff                                    </summary>
    // ----------------------------------------------------
    private void DebugDisplayPathTo(City otherCity)
    {
        List<Node> path = Pathfinder.Instance.FindPath(_waypoint._node, otherCity._waypoint._node);
        if (path == null)
        {
            Debug.LogWarning("Couldn't find path between " + name + " and " + otherCity.name);
            return;
        }
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i]._position, path[i + 1]._position, Color.red, 0.5f);
        }
    }

    // ---------------------------------------------------- <summary>
    // Write stuff to Debug.Log iff "enable My Debug Log" is checked      </summary>
    // ----------------------------------------------------
    public bool _enableMyDebugLog = false;
    private void MyDebugLog( string text )
    {
        if(_enableMyDebugLog)
        {
            Debug.Log(text);
        }
    }
    // ---------------------------------------------------- <summary>
    // Draw lines to neighbour cities                     </summary>
    // ----------------------------------------------------
    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        // For debug purposes
        foreach (City neighbour in _neighbourCities)
        {
            Debug.DrawLine(transform.position, 0.5f * transform.position + 0.5f * neighbour.transform.position, Color.blue);
        }
    }
}
