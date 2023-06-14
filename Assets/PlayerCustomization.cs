using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [System.Serializable]
    public class Character
    {
        public string Gender;
        public List<GameObject> MainBody;
        public List<GameObject> Hairs;
        public List<GameObject> HeadGear;
        public List<GameObject> FaceGear;
        public List<GameObject> Beard;
        public List<GameObject> Vest;
    }
    
    public List<Character> Characters = new List<Character>();

    public int GenderIndex;
    public int MainBodyIndex;
    public int HairsIndex;
    public int HeadGearIndex;
    public int FaceGearIndex;
    public int BeardIndex;
    public int VestIndex;

    public void SelectGender(int index)
    {
        GenderIndex = index;
    }
  
    public void SelectMainBody(int index)
    {

        if (MainBodyIndex < Characters[GenderIndex].MainBody.Count)
        {
            MainBodyIndex += index;

            if (MainBodyIndex == -1)
                MainBodyIndex = Characters[GenderIndex].MainBody.Count;
           
        }         
        else
        {
            if(index == 1)
                MainBodyIndex = 0;
            else
                MainBodyIndex += index;
        }
    }
}
