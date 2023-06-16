using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCustomization : MonoBehaviour
{
    [Serializable]
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
    [Serializable]
    public class CharacterIndex
    {

        public string Gender;
        public int ActiveGenderIndex;
        public int MainBodyIndex;
        public int HairsIndex;
        public int HeadGearIndex;
        public int FaceGearIndex;
        public int BeardIndex;
        public int VestIndex;
    }

    public List<Character> Characters = new List<Character>();
    public List<CharacterIndex> characterIndex = new List<CharacterIndex>();

    public List<GameObject> MaleCustoms = new List<GameObject>();
    public List<GameObject> FemaleCustoms = new List<GameObject>();

    public int GenderIndex;


    private PlayerCustomizationData playerData = new JasonDataService();
    private bool EncryptionEnabled;


    private void Awake()
    {
        //Loading Data
        List<CharacterIndex> data = playerData.LoadData<List<CharacterIndex>>("/player-Customization.json", EncryptionEnabled);
        characterIndex = data;
        GenderIndex = characterIndex[0].ActiveGenderIndex;
        
        ChangeGender();
        LoadPlayerData();
    }
    public void LoadPlayerData()
    {
        foreach (GameObject body in Characters[GenderIndex].MainBody) //   <--- go back to here --------+
        {

            if (body == Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex])
            {
                Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex].SetActive(true);

                if (GenderIndex == 0)
                    MaleCustoms.Add(Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex]);
                else
                    FemaleCustoms.Add(Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex]);

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
    public void SelectGender(int index)
    {
        GenderIndex = index;
        characterIndex[0].ActiveGenderIndex = GenderIndex;
        characterIndex[1].ActiveGenderIndex = GenderIndex;
        ChangeGender();
        playerData.SaveData("/player-Customization.json", characterIndex, EncryptionEnabled);
    }

    public void ChangeGender()
    {
        if (GenderIndex == 0)
        {
            Characters[0].MainBody[0].SetActive(false);
            Characters[0].MainBody[characterIndex[0].MainBodyIndex].SetActive(true);           
            Characters[1].MainBody[characterIndex[1].MainBodyIndex].SetActive(false);
        }


        else
        {
            Characters[0].MainBody[0].SetActive(false);
            Characters[0].MainBody[characterIndex[0].MainBodyIndex].SetActive(false);
            Characters[1].MainBody[characterIndex[1].MainBodyIndex].SetActive(true);
        }
        //if (GenderIndex == 0)
        //{
        //    foreach (GameObject body in MaleCustoms) //   <--- go back to here --------+
        //    {
        //        // do work
        //        body.SetActive(true);
        //    }
        //    foreach (GameObject body in FemaleCustoms) //   <--- go back to here --------+
        //    {
        //        // do work
        //        body.SetActive(false);
        //    }
        //}
        //else
        //{
        //    foreach (GameObject body in MaleCustoms) //   <--- go back to here --------+
        //    {
        //        // do work
        //        body.SetActive(false);
        //    }
        //    foreach (GameObject body in FemaleCustoms) //   <--- go back to here --------+
        //    {
        //        // do work
        //        body.SetActive(true);
        //    }
        //}


    }

    public void SelectMainBody(int index)
    {
        if (characterIndex[GenderIndex].MainBodyIndex < Characters[GenderIndex].MainBody.Count - 1)
        {
            characterIndex[GenderIndex].MainBodyIndex += index;

            if (characterIndex[GenderIndex].MainBodyIndex == -1)
                characterIndex[GenderIndex].MainBodyIndex = Characters[GenderIndex].MainBody.Count - 1;
        }
        else
        {
            if (index == 1)
                characterIndex[GenderIndex].MainBodyIndex = 0;
            else
                characterIndex[GenderIndex].MainBodyIndex += index;
        }
        foreach (GameObject body in Characters[GenderIndex].MainBody) //   <--- go back to here --------+
        {
            if (body == Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex])
            {
                Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex].SetActive(true);

                if (GenderIndex == 0)
                    MaleCustoms.Add(Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex]);
                else
                    FemaleCustoms.Add(Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex]);

                continue;   // Skip the remainder of this iteration. -----+
            }

            if (GenderIndex == 0)
                MaleCustoms.Remove(body);
            else
                FemaleCustoms.Remove(body);
            // do work
            body.SetActive(false);
            //characterIndex[GenderIndex].MainBodyIndex = 4;

            //SavingData
            playerData.SaveData("/player-Customization.json", characterIndex, EncryptionEnabled);

        }

    }
    public void SerializeJson()
    {
        long startTime = DateTime.Now.Ticks;
        if (playerData.SaveData("/player-Customization.json", characterIndex, EncryptionEnabled))
        {
            //SaveTime = DateTime.Now.Ticks - startTime;
            //SaveTimeText.SetText($"Save Time: {(SaveTime / TimeSpan.TicksPerMillisecond):N4}ms");

            startTime = DateTime.Now.Ticks;
            try
            {

                CharacterIndex data = playerData.LoadData<CharacterIndex>("/player-Customization.json", EncryptionEnabled);
                Debug.Log("Work");
                //LoadTime = DateTime.Now.Ticks - startTime;
                //InputField.text = "Loaded from file:\r\n" + JsonConvert.SerializeObject(data, Formatting.Indented);
                // LoadTimeText.SetText($"Load Time: {(LoadTime / TimeSpan.TicksPerMillisecond):N4}ms");
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not read file! Show something on the UI here!");
                //InputField.text = "<color=#ff0000>Error reading save file!</color>";
            }
        }
        else
        {
            Debug.LogError("Could not save file! Show something on the UI about it!");
            //InputField.text = "<color=#ff0000>Error saving data!</color>";
        }
    }
}
