using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
   
    public int BoardSizeX = 5;
    public int BoardSizeY = 5;
    public int Layers = 3; 

    public int MaxTraySize = 5; 
    public float ActionDelay = 0.5f;
}

