using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Faction { Loyalists = 0, Rebels = 1 };

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
    public Faction _occupyingFaction;
    public int _occupyingForces = 10;
    public int _attackingForces = 0;

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


    // ---------------------------------------------------- <summary>
    // The update function.
    // ----------------------------------------------------
    void Update()
    {
        // For debug purposes
        foreach (City neighbour in _neighbourCities)
        {
            Debug.DrawLine(transform.position, 0.5f * transform.position + 0.5f * neighbour.transform.position, Color.blue);
        }

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
        _factionIndicator.color = (_occupyingFaction == Faction.Rebels) ? Color.red : Color.black;

        // Update occupying troops
        Sprite spriteUs = (_occupyingFaction == Faction.Loyalists) ? _loyalistAntSprite : _rebelAntSprite;
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
        Sprite spriteThem = (_occupyingFaction == Faction.Loyalists) ? _rebelAntSprite : _loyalistAntSprite;
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
            _occupyingForcesImages[Random.Range(0, _occupyingForces + 1)].sprite = (_occupyingFaction == Faction.Loyalists) ? _deadBlackAntSprite : _deadRedAntSprite;

            // Did the attacker win ?
            if (_occupyingForces == 0)
            {
                // Attacker takes the city
                _occupyingForces = _attackingForces;
                _attackingForces = 0;
                _occupyingFaction = (Faction)(1 - (int)_occupyingFaction);
            }
        }
        else
        {
            // Defender kills one attacker
            _attackingForces--;

            // Display dead ant symbol
            _attackingForcesImages[Random.Range(0, _attackingForces + 1)].sprite = (_occupyingFaction == Faction.Loyalists) ? _deadRedAntSprite : _deadBlackAntSprite;
        }
    }

    // For AI  
    // TODO: put into header
    private int _menacingForces;
    private int _distanceToEnemy;
    private int _weakestEnemyForces;
    private City _weakestEnemy;
    private int _weakestFriendNeededReinforcement;
    private City _weakestFriend;
    private int _indispensableForces;
    private int _dispensableForces;
    private const int MINIMAL_OCCUPATION_FORCE = 3;
    private const int ATTACK_TRESHOLD = 5;
    private const float MENACE_DEPTH_FACTOR = 0.3f;
    private const int MAX_TROOPS = 20;

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
                int slowMigrationToFront = (neighbour._distanceToEnemy < oldDistanceToEnemy) ? 1 : 0;
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

    private void SendTroopsTo(int amount, City city)
    {
        if (city._occupyingFaction == _occupyingFaction)
        {
            // Make sure that occupying forces + amount <= MAX_TROOPS
            amount = Mathf.Min(amount, MAX_TROOPS - city._occupyingForces);
        }
        else
        {
            // Make sure that attacking forces + amount <= MAX_TROOPS
            amount = Mathf.Min(amount, MAX_TROOPS - city._attackingForces);
        }

        // Deploy troops
        _occupyingForces -= amount;

        //TODO: not immediately
        city.ReceiveTroops(amount, _occupyingFaction);
    }

    private void ReceiveTroops(int amount, Faction faction )
    {
        //TODO: surplus troops look elsewhere
        if(faction == _occupyingFaction)
        {
            // Receiving reinforcements
            _occupyingForces = Mathf.Min(_occupyingForces + amount, MAX_TROOPS );
        }
        else
        {
            // Being attacked
            _attackingForces = Mathf.Min(_attackingForces + amount, MAX_TROOPS);
        }
    }


    public bool _enableMyDebugLog = false;
    private void MyDebugLog( string text )
    {
        if(_enableMyDebugLog)
        {
            Debug.Log(text);
        }
    }
}
