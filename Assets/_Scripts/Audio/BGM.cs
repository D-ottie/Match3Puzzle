using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM : MonoBehaviour
{
    private AudioSource _BGM;

    private void Awake()
    {
        _BGM = GetComponent<AudioSource>(); 
    }

    private void Start()
    {
        _BGM.Play(); 
    }
}
