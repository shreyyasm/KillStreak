using FishNet.Object;
using FishNet.Object.Synchronizing;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCustomization : NetworkBehaviour
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

    }

    public List<Character> Characters = new List<Character>();

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public List<CharacterIndex> characterIndex { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public int GenderIndex { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }


    private PlayerCustomizationData playerData = new JasonDataService();
    private bool EncryptionEnabled;


    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (base.Owner.IsLocalClient)
        {
            List<CharacterIndex> data = playerData.LoadData<List<CharacterIndex>>("/player-CustomizationNew.json", EncryptionEnabled);
            characterIndex = data;
            GenderIndex = characterIndex[0].ActiveGenderIndex;
            ChangeGender();
            LoadPlayerData();
        }
    }

    private void Update()
    {

    }
    public void LoadPlayerData()
    {
        if (base.IsServer)
            LoadPlayerDataObserver();
        else
            LoadPlayerDataServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void LoadPlayerDataServer()
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
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void LoadPlayerDataObserver()
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
        if (base.IsServer)
            ChangeGenderObserver();
        else
            ChangeGenderServer();

    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void ChangeGenderServer()
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
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void ChangeGenderObserver()
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



}
