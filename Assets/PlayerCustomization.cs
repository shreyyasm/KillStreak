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

    public List<GameObject> MaleCustoms = new List<GameObject>();
    public List<GameObject> FemaleCustoms = new List<GameObject>();

    public int GenderIndex;
    public int MainBodyIndex;
    public int HairsIndex;
    public int HeadGearIndex;
    public int FaceGearIndex;
    public int BeardIndex;
    public int VestIndex;

    private void Awake()
    {
        ChangeGender();
    }
    public void SelectGender(int index)
    {
        GenderIndex = index;
        ChangeGender();
    }
    public void ChangeGender()
    {
        if (GenderIndex == 0)
        {
            foreach (GameObject body in MaleCustoms) //   <--- go back to here --------+
            {
                // do work
                body.SetActive(true);
            }
            foreach (GameObject body in FemaleCustoms) //   <--- go back to here --------+
            {
                // do work
                body.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject body in MaleCustoms) //   <--- go back to here --------+
            {
                // do work
                body.SetActive(false);
            }
            foreach (GameObject body in FemaleCustoms) //   <--- go back to here --------+
            {
                // do work
                body.SetActive(true);
            }
        }

    }
    public void SelectMainBody(int index)
    {
       
        if (MainBodyIndex < Characters[GenderIndex].MainBody.Count - 1)
        {
            MainBodyIndex += index;

            if (MainBodyIndex == -1)
                MainBodyIndex = Characters[GenderIndex].MainBody.Count - 1;     
        }         
        else
        {
            if(index == 1)
                MainBodyIndex = 0;
            else
                MainBodyIndex += index;
        }
        foreach (GameObject body in Characters[GenderIndex].MainBody) //   <--- go back to here --------+
        {                        
            if (body == Characters[GenderIndex].MainBody[MainBodyIndex])                                  
            {
                Characters[GenderIndex].MainBody[MainBodyIndex].SetActive(true);

                if(GenderIndex == 0)
                    MaleCustoms.Add(Characters[GenderIndex].MainBody[MainBodyIndex]);
                else
                    FemaleCustoms.Add(Characters[GenderIndex].MainBody[MainBodyIndex]);
                continue;   // Skip the remainder of this iteration. -----+
            }

            if (GenderIndex == 0)
                MaleCustoms.Remove(body);
            else
                FemaleCustoms.Remove(body);
            // do work
            body.SetActive(false);
        }
    }
}
