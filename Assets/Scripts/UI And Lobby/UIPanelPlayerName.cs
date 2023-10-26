using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace EOSLobbyTest
{
    public class UIPanelPlayerName : UIPanel<UIPanelPlayerName>, IUIPanel
    {
        [SerializeField]
        public TMP_InputField inputFieldPlayerName;

        [SerializeField]
        public TMP_InputField PlayerNameText;

        [SerializeField]
        private Button buttonSave;
         
        private void Awake()
        {
            //string data = playerData.LoadData<string>("/player-Info.json", EncryptionEnabled);
            //Settings.Instance.CurrentPlayerName = data;
            //Debug.Log(Settings.Instance.CurrentPlayerName);

        }
        private void Start()
        {
            inputFieldPlayerName.onValueChanged.AddListener(delegate
            {
                UpdateControlState();
            });
           

        }
        private void Update()
        {
            PlayerName = inputFieldPlayerName.ToString();

        }
        private void UpdateControlState()
        {
            buttonSave.interactable = !String.IsNullOrEmpty(inputFieldPlayerName.text);
            PlayerName = inputFieldPlayerName.ToString();
            Settings.Instance.CurrentPlayerName = inputFieldPlayerName.text;

        }

        protected override void OnShowing()
        {
            UpdateControlState();

            //inputFieldPlayerName.text = Settings.Instance.CurrentPlayerName;
        }

        protected override void OnShown()
        {
            inputFieldPlayerName.ActivateInputField();
        }
        private PlayerInfoData playerData = new JasonDataService();
        private string PlayerName;
        private bool EncryptionEnabled;
        public void Save()
        {
            Debug.Log(inputFieldPlayerName.text);
            PlayerName = inputFieldPlayerName.text;
            Settings.Instance.CurrentPlayerName = PlayerName;
            PlayerPrefs.SetString("PlayerName", PlayerName);
            PlayerNameText.text = PlayerPrefs.GetString("PlayerName");
            UIPanelManager.Instance.HidePanel<UIPanelPlayerName>(true);
        }
        private class SaveObject
        {
            public string PlayerName;
            
        }
        public void Cancel()
        {
            UIPanelManager.Instance.HidePanel<UIPanelPlayerName>(false);
        }
    }
}
