using FishNet.Plugins.FishyEOS.Util;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Lobby;
using FishNet.Managing;
using System.Linq;
using FishNet.Transporting.FishyEOSPlugin;
using FishNet;

namespace EOSLobbyTest
{
    public class UIPanelMain : UIPanel<UIPanelMain>, IUIPanel
    {

        [Tooltip("Input for changing player name directly")]
        [SerializeField]
        private InputField inputFieldPlayerName;

        [Tooltip("Button for hosting game")]
        [SerializeField]
        private Button buttonHost;

        [Tooltip("Button for joining game")]
        [SerializeField]
        private Button buttonJoin;

        [Tooltip("Button for adjusting settings")]
        [SerializeField]
        private Button buttonSettings;


        private void Start()
        {
            Settings.Instance.Load();
            Debug.Log(Settings.Instance.CurrentPlayerName);
            inputFieldPlayerName.onValueChanged.AddListener(delegate
            {
                Settings.Instance.CurrentPlayerName = inputFieldPlayerName.text;
                Debug.Log(Settings.Instance.CurrentPlayerName);
            });

            VivoxManager.Instance.OnInitialized += Vivox_OnAuthenticated;

            UpdateControlState();

            if (EOS.LocalProductUserId == null)
            {
                StartCoroutine(ConnectToEOS());
            }
        }

        private void OnDestroy()
        {
            if (VivoxManager.Instance != null)
            {
                VivoxManager.Instance.OnInitialized -= Vivox_OnAuthenticated;
            }
        }

        private void Vivox_OnAuthenticated()
        {
            UpdateControlState();
        }

        private IEnumerator ConnectToEOS()
        {
            var authData = new AuthData();

            // do the EOS login
            authData.loginCredentialType = Epic.OnlineServices.Auth.LoginCredentialType.DeviceCode;
            yield return authData.Connect(out var authDataLogin);
            if (authDataLogin.loginCallbackInfo?.ResultCode != Result.Success)
            {
                // failed
                Debug.LogError($"[ServerPeer] Failed to authenticate with EOS Connect. {authDataLogin.loginCallbackInfo?.ResultCode}");
                UpdateControlState();
                yield break;
            }

            // force EOS relay always on - hides IP
            var relayControlOptions = new SetRelayControlOptions { RelayControl = RelayControl.ForceRelays };
            EOS.GetCachedP2PInterface().SetRelayControl(ref relayControlOptions);
            UpdateControlState();
        }

        private void UpdateControlState()
        {
            inputFieldPlayerName.text = Settings.Instance.CurrentPlayerName;

            // only allow host/join if all logged in ok
            buttonHost.interactable = EOS.LocalProductUserId != null;
            buttonJoin.interactable = EOS.LocalProductUserId != null;

            // as the voice device is not initialised until we logged in we have to wait on this - not ideal...
            buttonSettings.interactable = VivoxManager.Instance.Initialized;
        }

        protected override void OnShowing()
        {
            UpdateControlState();
        }

        private IEnumerator DoDeathmatchHostGame()
        {
            // is player name blank ?
            if (String.IsNullOrEmpty(Settings.Instance.CurrentPlayerName))
            {
                // show panel that gets player name
                yield return UIPanelManager.Instance.ShowPanelAndWaitTillHidden<UIPanelPlayerName>();

                // update player name on this panel
                UpdateControlState();
            }

            // only allow host if we have player name
            if (!String.IsNullOrEmpty(Settings.Instance.CurrentPlayerName))
            {
                yield return UIPanelManager.Instance.ShowPanelAndWaitTillHidden<UIPanelHostDetails>();

                // did we create a valid room name ?
                if (UIPanelHostDetails.Instance.UIResult)
                {
                    UIPanelLobby.Instance.IsHost = true;
                    UIPanelManager.Instance.ShowPanel<UIPanelLobby>();
                }
            }
        }
        private IEnumerator DoTeamDeathmatchHostGame()
        {
            // is player name blank ?
            if (String.IsNullOrEmpty(Settings.Instance.CurrentPlayerName))
            {
                // show panel that gets player name
                yield return UIPanelManager.Instance.ShowPanelAndWaitTillHidden<UIPanelPlayerName>();

                // update player name on this panel
                UpdateControlState();
            }

            // only allow host if we have player name
            if (!String.IsNullOrEmpty(Settings.Instance.CurrentPlayerName))
            {
                yield return UIPanelManager.Instance.ShowPanelAndWaitTillHidden<UIPanelHostDetails>();

                // did we create a valid room name ?
                if (UIPanelHostDetails.Instance.UIResult)
                {
                    UIPanel4V4Lobby.Instance.IsHost = true;
                    UIPanelManager.Instance.ShowPanel<UIPanel4V4Lobby>();
                }
            }
        }

        private IEnumerator DoJoinGame()
        {
            // is player name blank ?
            if (String.IsNullOrEmpty(Settings.Instance.CurrentPlayerName))
            {
                // show panel that gets player name
                yield return UIPanelManager.Instance.ShowPanelAndWaitTillHidden<UIPanelPlayerName>();

                // update player name on this panel
                UpdateControlState();
            }

            // only allow join if we have player name
            if (!String.IsNullOrEmpty(Settings.Instance.CurrentPlayerName))
            {
                // show panel showing list of lobbies
                UIPanelManager.Instance.ShowPanel<UIPanelLobbies>();
                UIPanelManager.Instance.HidePanel<UIGameModeJoinPanel>(true);
            }
        }

        public void DeathmatchHostGame()
        {
            StartCoroutine(DoDeathmatchHostGame());
        }
        public void TeamDeathmatchHostGame()
        {
            StartCoroutine(DoTeamDeathmatchHostGame());
        }
        [SerializeField] UIPanelLobbies uIPanelLobbies;
        public void JoinGame(bool deathMatch)
        {
            StartCoroutine(DoJoinGame());
            uIPanelLobbies.deathMatch = deathMatch;


        }

        public void ShowSettings()
        {
            UIPanelManager.Instance.ShowPanel<UIPanelMenuSettings>();
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}
