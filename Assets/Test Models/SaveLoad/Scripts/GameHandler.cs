/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameHandler : MonoBehaviour {

    [SerializeField] private GameObject unitGameObject;
    private IUnit unit;

    private void Awake() {
        unit = unitGameObject.GetComponent<IUnit>();
        SaveSystem.Init();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            Load();
        }
    }

    private void Save() {
        // Save
        Vector3 playerPosition = unit.GetPosition();
        int goldAmount = unit.GetGoldAmount();

        SaveObject saveObject = new SaveObject { 
            goldAmount = goldAmount,
            playerPosition = playerPosition
        };
        string json = JsonUtility.ToJson(saveObject);
        SaveSystem.Save(json);

        //CMDebug.TextPopupMouse("Saved!");
    }

    private void Load() {
        // Load
        string saveString = SaveSystem.Load();
        if (saveString != null) {
            //CMDebug.TextPopupMouse("Loaded: " + saveString);

            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);

            unit.SetPosition(saveObject.playerPosition);
            unit.SetGoldAmount(saveObject.goldAmount);
        } 
    }


    private class SaveObject {
        public int goldAmount;
        public Vector3 playerPosition;
    }
}