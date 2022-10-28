using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reshuffle : MonoBehaviour
{
    private Reshuffle instance;
    public static Reshuffle Singleton;

    [SerializeField] private float secondsToCheck = 10f;
    public float currentTime { get; set; }
    public bool _alreadyChecked { get; set; }
    public bool InCombo { get; set; }
    public int ComboCounter { get; set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        else { Singleton = this; }
        //DontDestroyOnLoad(Singleton);
    }
    private void Start()
    {
        currentTime = 0;
        ComboCounter = 0; 
        InCombo = false; 
    }
    private void Update()
    {
        if (currentTime >= secondsToCheck + 1)
        {
            // if the time is greater than 10, just don't do anything. 
            // don't proceed with the rest of the code. 
            return;
        }

        if (_alreadyChecked)
        {
            return; 
        }
        currentTime += Time.deltaTime; // add every second. 
                                       //print(Mathf.Round(currentTime) + "In script");

        //print(InCombo + "Currently combo status");

        // if you ever go beyond 7 seconds without resetting(making a clear), we reset the combo bool
        if (currentTime > 7)
        {
            InCombo = false;
            ComboCounter = 0; 
        }
        if (currentTime > secondsToCheck)
        {
            if (FindObjectOfType<GameGrid>())
            {
                FindObjectOfType<GameGrid>().KickOfCheck();
            }
        }
        else
        {
            return;
        }
    }
}
