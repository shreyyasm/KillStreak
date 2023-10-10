using FishNet.Object;
using FishNet.Object.Synchronizing;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerCustomizationNonNetwork : MonoBehaviour
{
    [Serializable]
    public class Character
    {
        public string Gender;
        public List<GameObject> MainBody;
        public List<GameObject> Hairs;
        public List<GameObject> HeadGear;
        public List<GameObject> Beard;
        public List<GameObject> Vest;
        public List<GameObject> Bag;
        public List<GameObject> MemeCharacter;

    }

 
    public List<Character> Characters = new List<Character>();


    public List<CharacterIndex> characterIndex = new List<CharacterIndex>();

    public List<TextMeshProUGUI> CustomizationNumbers = new List<TextMeshProUGUI>();

    public int GenderIndex;

    private PlayerCustomizationData playerData = new JasonDataService();
    private bool EncryptionEnabled;



    private void Awake()
    {

        List<CharacterIndex> data = playerData.LoadData<List<CharacterIndex>>("/player-CustomizationNew.json", EncryptionEnabled);
        characterIndex = data;
        GenderIndex = characterIndex[0].ActiveGenderIndex;
        ChangeGender();
        LoadPlayerData();
    }
    private void Update()
    {

    }

    public void LoadPlayerData()
    {

        foreach (GameObject body in Characters[GenderIndex].MainBody) //   <--- go back to here --------+
        {

            if (body == Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex])
            {
                Characters[GenderIndex].MainBody[characterIndex[GenderIndex].MainBodyIndex].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }
            // do work
            body.SetActive(false);
        }
        ShowCustomizationNumbers();
    }


    public void SaveData()
    {
        playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);
        
    }
    public void SelectGender(int index)
    {
        GenderIndex = index;
        characterIndex[0].ActiveGenderIndex = GenderIndex;
        characterIndex[1].ActiveGenderIndex = GenderIndex;
        ChangeGender();
        playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);
    }



    public void ChangeGender()
    {
        GenderIndex = characterIndex[0].ActiveGenderIndex;
        if (GenderIndex == 0)
        {
            Characters[0].MainBody[0].SetActive(false);

            //MainBody
            Characters[0].MainBody[characterIndex[0].MainBodyIndex].SetActive(true);
            Characters[1].MainBody[characterIndex[1].MainBodyIndex].SetActive(false);

            //Hairs
            Characters[0].Hairs[characterIndex[0].HairsIndex].SetActive(true);
            Characters[1].Hairs[characterIndex[1].HairsIndex].SetActive(false);

            //HeadGear
            Characters[0].HeadGear[characterIndex[0].HeadGearIndex].SetActive(true);
            Characters[1].HeadGear[characterIndex[1].HeadGearIndex].SetActive(false);

            //Beard
            Characters[0].Beard[characterIndex[0].BeardIndex].SetActive(true);
            Characters[1].Beard[characterIndex[1].BeardIndex].SetActive(false);

            //Vest
            Characters[0].Vest[characterIndex[0].VestIndex].SetActive(true);
            Characters[1].Vest[characterIndex[1].VestIndex].SetActive(false);

            //Bag
            Characters[0].Bag[characterIndex[0].BagIndex].SetActive(true);
            Characters[1].Bag[characterIndex[1].BagIndex].SetActive(false);
        }


        else
        {
            Characters[0].MainBody[0].SetActive(false);

            //MainBody
            Characters[0].MainBody[characterIndex[0].MainBodyIndex].SetActive(false);
            Characters[1].MainBody[characterIndex[1].MainBodyIndex].SetActive(true);

            //Hairs
            Characters[0].Hairs[characterIndex[0].HairsIndex].SetActive(false);
            Characters[1].Hairs[characterIndex[1].HairsIndex].SetActive(true);

            //HeadGear
            Characters[0].HeadGear[characterIndex[0].HeadGearIndex].SetActive(false);
            Characters[1].HeadGear[characterIndex[1].HeadGearIndex].SetActive(true);

            //Beard
            Characters[0].Beard[characterIndex[0].BeardIndex].SetActive(false);
            Characters[1].Beard[characterIndex[1].BeardIndex].SetActive(true);

            //Vest
            Characters[0].Vest[characterIndex[0].VestIndex].SetActive(false);
            Characters[1].Vest[characterIndex[1].VestIndex].SetActive(true);

            //Bag
            Characters[0].Bag[characterIndex[0].BagIndex].SetActive(false);
            Characters[1].Bag[characterIndex[1].BagIndex].SetActive(true);
        }

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
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            body.SetActive(false);

            //SavingData
            playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);

        }
        CustomizationNumbers[0].text = characterIndex[GenderIndex].MainBodyIndex + " / " + (Characters[GenderIndex].MainBody.Count -1);
    }
    public void SelectHairs(int index)
    {
        if (characterIndex[GenderIndex].HairsIndex < Characters[GenderIndex].Hairs.Count - 1)
        {
            characterIndex[GenderIndex].HairsIndex += index;

            if (characterIndex[GenderIndex].HairsIndex == -1)
                characterIndex[GenderIndex].HairsIndex = Characters[GenderIndex].Hairs.Count - 1;
        }
        else
        {
            if (index == 1)
                characterIndex[GenderIndex].HairsIndex = 0;
            else
                characterIndex[GenderIndex].HairsIndex += index;
        }
        foreach (GameObject body in Characters[GenderIndex].Hairs) //   <--- go back to here --------+
        {
            if (body == Characters[GenderIndex].Hairs[characterIndex[GenderIndex].HairsIndex])
            {
                Characters[GenderIndex].Hairs[characterIndex[GenderIndex].HairsIndex].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            body.SetActive(false);

            //SavingData
            playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);

        }
        if (characterIndex[GenderIndex].HairsIndex == 0)
        {

            Characters[GenderIndex].HeadGear[characterIndex[GenderIndex].HeadGearIndex].SetActive(true);

        }
        else
        {

            Characters[GenderIndex].HeadGear[characterIndex[GenderIndex].HeadGearIndex].SetActive(false);
            characterIndex[GenderIndex].HeadGearIndex = 0;
        }
        CustomizationNumbers[1].text = characterIndex[GenderIndex].HairsIndex + " / " + (Characters[GenderIndex].Hairs.Count - 1);
    }
    public void SelectHeadGear(int index)
    {
        if (characterIndex[GenderIndex].HeadGearIndex < Characters[GenderIndex].HeadGear.Count - 1)
        {
            characterIndex[GenderIndex].HeadGearIndex += index;

            if (characterIndex[GenderIndex].HeadGearIndex == -1)
                characterIndex[GenderIndex].HeadGearIndex = Characters[GenderIndex].HeadGear.Count - 1;
        }
        else
        {
            if (index == 1)
                characterIndex[GenderIndex].HeadGearIndex = 0;
            else
                characterIndex[GenderIndex].HeadGearIndex += index;
        }
        foreach (GameObject body in Characters[GenderIndex].HeadGear) //   <--- go back to here --------+
        {
            if (body == Characters[GenderIndex].HeadGear[characterIndex[GenderIndex].HeadGearIndex])
            {
                Characters[GenderIndex].HeadGear[characterIndex[GenderIndex].HeadGearIndex].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            body.SetActive(false);

            //SavingData
            playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);

        }    
        if(characterIndex[GenderIndex].HeadGearIndex == 0)
        {
            
            Characters[GenderIndex].Hairs[characterIndex[GenderIndex].HairsIndex].SetActive(true);
  
        }
        else
        {

            Characters[GenderIndex].Hairs[characterIndex[GenderIndex].HairsIndex].SetActive(false);
            characterIndex[GenderIndex].HairsIndex = 0;
        }
        CustomizationNumbers[2].text = characterIndex[GenderIndex].HeadGearIndex + " / " + (Characters[GenderIndex].HeadGear.Count - 1);
    }
    public void SelectBeard(int index)
    {
        if (characterIndex[GenderIndex].BeardIndex < Characters[GenderIndex].Beard.Count - 1)
        {
            characterIndex[GenderIndex].BeardIndex += index;

            if (characterIndex[GenderIndex].BeardIndex == -1)
                characterIndex[GenderIndex].BeardIndex = Characters[GenderIndex].Beard.Count - 1;
        }
        else
        {
            if (index == 1)
                characterIndex[GenderIndex].BeardIndex = 0;
            else
                characterIndex[GenderIndex].BeardIndex += index;
        }
        foreach (GameObject body in Characters[GenderIndex].Beard) //   <--- go back to here --------+
        {
            if (body == Characters[GenderIndex].Beard[characterIndex[GenderIndex].BeardIndex])
            {
                Characters[GenderIndex].Beard[characterIndex[GenderIndex].BeardIndex].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            body.SetActive(false);

            //SavingData
            playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);

        }
        if (characterIndex[GenderIndex].BeardIndex == 0)
        {

            Characters[GenderIndex].MemeCharacter[characterIndex[GenderIndex].MemeCharacterIndex].SetActive(true);

        }
        else
        {

            Characters[GenderIndex].MemeCharacter[characterIndex[GenderIndex].MemeCharacterIndex].SetActive(false);
            characterIndex[GenderIndex].MemeCharacterIndex = 0;
        }
        CustomizationNumbers[3].text = characterIndex[GenderIndex].BeardIndex + " / " + (Characters[GenderIndex].Beard.Count - 1);
    }
    public void SelectVest(int index)
    {
        if (characterIndex[GenderIndex].VestIndex < Characters[GenderIndex].Vest.Count - 1)
        {
            characterIndex[GenderIndex].VestIndex += index;

            if (characterIndex[GenderIndex].VestIndex == -1)
                characterIndex[GenderIndex].VestIndex = Characters[GenderIndex].Vest.Count - 1;
        }
        else
        {
            if (index == 1)
                characterIndex[GenderIndex].VestIndex = 0;
            else
                characterIndex[GenderIndex].VestIndex += index;
        }
        foreach (GameObject body in Characters[GenderIndex].Vest) //   <--- go back to here --------+
        {
            if (body == Characters[GenderIndex].Vest[characterIndex[GenderIndex].VestIndex])
            {
                Characters[GenderIndex].Vest[characterIndex[GenderIndex].VestIndex].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            body.SetActive(false);

            //SavingData
            playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);

        }
        CustomizationNumbers[4].text = characterIndex[GenderIndex].VestIndex + " / " + (Characters[GenderIndex].Vest.Count - 1);
    }
    public void SelectBag(int index)
    {
        if (characterIndex[GenderIndex].BagIndex < Characters[GenderIndex].Bag.Count - 1)
        {
            characterIndex[GenderIndex].BagIndex += index;

            if (characterIndex[GenderIndex].BagIndex == -1)
                characterIndex[GenderIndex].BagIndex = Characters[GenderIndex].Bag.Count - 1;
        }
        else
        {
            if (index == 1)
                characterIndex[GenderIndex].BagIndex = 0;
            else
                characterIndex[GenderIndex].BagIndex += index;
        }
        foreach (GameObject body in Characters[GenderIndex].Bag) //   <--- go back to here --------+
        {
            if (body == Characters[GenderIndex].Bag[characterIndex[GenderIndex].BagIndex])
            {
                Characters[GenderIndex].Bag[characterIndex[GenderIndex].BagIndex].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            body.SetActive(false);

            //SavingData
            playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);

        }
        CustomizationNumbers[5].text = characterIndex[GenderIndex].BagIndex + " / " + (Characters[GenderIndex].Bag.Count - 1);
    }
    public void SelectMemeCharacter(int index)
    {
        if (characterIndex[GenderIndex].MemeCharacterIndex < Characters[GenderIndex].MemeCharacter.Count - 1)
        {
            characterIndex[GenderIndex].MemeCharacterIndex += index;

            if (characterIndex[GenderIndex].MemeCharacterIndex == -1)
                characterIndex[GenderIndex].MemeCharacterIndex = Characters[GenderIndex].MemeCharacter.Count - 1;
        }
        else
        {
            if (index == 1)
                characterIndex[GenderIndex].MemeCharacterIndex = 0;
            else
                characterIndex[GenderIndex].MemeCharacterIndex += index;
        }
        foreach (GameObject body in Characters[GenderIndex].MemeCharacter) //   <--- go back to here --------+
        {
            if (body == Characters[GenderIndex].MemeCharacter[characterIndex[GenderIndex].MemeCharacterIndex])
            {
                Characters[GenderIndex].MemeCharacter[characterIndex[GenderIndex].MemeCharacterIndex].SetActive(true);
                continue;   // Skip the remainder of this iteration. -----+
            }

            // do work
            body.SetActive(false);

            //SavingData
            playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled);

        }
        if (characterIndex[GenderIndex].MemeCharacterIndex == 0)
        {

            Characters[GenderIndex].Beard[characterIndex[GenderIndex].BeardIndex].SetActive(true);

        }
        else
        {

            Characters[GenderIndex].Beard[characterIndex[GenderIndex].BeardIndex].SetActive(false);
            characterIndex[GenderIndex].BeardIndex = 0;
        }
        CustomizationNumbers[6].text = characterIndex[GenderIndex].MemeCharacterIndex + " / " + (Characters[GenderIndex].MemeCharacter.Count - 1);
    }
    public void ShowCustomizationNumbers()
    {
        if(GenderIndex == 0)
        {
            CustomizationNumbers[0].text = characterIndex[0].MainBodyIndex + " / " + (Characters[0].MainBody.Count - 1);
            CustomizationNumbers[1].text = characterIndex[0].HairsIndex + " / " + (Characters[0].Hairs.Count - 1);
            CustomizationNumbers[2].text = characterIndex[0].HeadGearIndex + " / " + (Characters[0].HeadGear.Count - 1);
            CustomizationNumbers[3].text = characterIndex[0].BeardIndex + " / " + (Characters[0].Beard.Count - 1);
            CustomizationNumbers[4].text = characterIndex[0].VestIndex + " / " + (Characters[0].Vest.Count - 1);
            CustomizationNumbers[5].text = characterIndex[0].BagIndex + " / " + (Characters[0].Bag.Count - 1);
            CustomizationNumbers[6].text = characterIndex[0].MemeCharacterIndex + " / " + (Characters[0].MemeCharacter.Count - 1);
        }
        else
        {
            CustomizationNumbers[0].text = characterIndex[1].MainBodyIndex + " / " + (Characters[1].MainBody.Count - 1);
            CustomizationNumbers[1].text = characterIndex[1].HairsIndex + " / " + (Characters[1].Hairs.Count - 1);
            CustomizationNumbers[2].text = characterIndex[1].HeadGearIndex + " / " + (Characters[1].HeadGear.Count - 1);
            CustomizationNumbers[3].text = characterIndex[1].BeardIndex + " / " + (Characters[1].Beard.Count - 1);
            CustomizationNumbers[4].text = characterIndex[1].VestIndex + " / " + (Characters[1].Vest.Count - 1);
            CustomizationNumbers[5].text = characterIndex[1].BagIndex + " / " + (Characters[1].Bag.Count - 1);
            CustomizationNumbers[6].text = characterIndex[1].MemeCharacterIndex + " / " + (Characters[1].MemeCharacter.Count - 1);
        }
    }
    public void SerializeJson()
    {
        long startTime = DateTime.Now.Ticks;
        if (playerData.SaveData("/player-CustomizationNew.json", characterIndex, EncryptionEnabled))
        {
            //SaveTime = DateTime.Now.Ticks - startTime;
            //SaveTimeText.SetText($"Save Time: {(SaveTime / TimeSpan.TicksPerMillisecond):N4}ms");

            startTime = DateTime.Now.Ticks;
            try
            {

                CharacterIndex data = playerData.LoadData<CharacterIndex>("/player-CustomizationNew.json", EncryptionEnabled);
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
