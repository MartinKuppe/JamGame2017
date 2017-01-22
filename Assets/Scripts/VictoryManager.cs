using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



///=================================================================================================================
///                                                                                                       <summary>
///  VictoryManager manages victory and defeat.              											 </summary>
///
///=================================================================================================================
public class VictoryManager : Singleton<VictoryManager> 
{
    public GameObject _defeatPanel;
    public Text _defeatText;
    public GameObject _victoryPanel;
    private City[] _cities;
    private float _checkVictoryTimer = 0.0f;

    public void Awake()
    {
        _cities = FindObjectsOfType<City>();
        Time.timeScale = 1.0f;
    }

    public void Update()
    {
        _checkVictoryTimer -= Time.deltaTime;
        if(_checkVictoryTimer < 0.0f)
        {
            _checkVictoryTimer = 1.0f;
            CheckVictory();
        }
    }

    public void GameOver( string message )
    {
        Time.timeScale = 0.0f;
        _defeatPanel.SetActive(true);
        _defeatText.text = message;
    }

    public void Victory()
    {
        Time.timeScale = 0.0f;
        _victoryPanel.SetActive(true);
    }

    private void CheckVictory()
    {
        int rebels = 0;
        int loyalists = 0;
        foreach( City city in _cities )
        {
            if( city.IsRebelCity)
            {
                rebels++;
            }
            else
            {
                loyalists++;
            }
        }

        if(rebels == 0)
        {
            GameOver("You lost the last city.");
        }
        else if(loyalists == 0 )
        {
            Victory();
        }
    }
}
