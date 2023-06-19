using FishNet.Object;
using FishNet.Object.Synchronizing;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CharacterIndex
{

    public string Gender;
    public int ActiveGenderIndex;
    public int MainBodyIndex;
    public int HairsIndex;
    public int HeadGearIndex;
    public int BeardIndex;
    public int VestIndex;
    public int BagIndex;
}