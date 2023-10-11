using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Plugins.FishyEOS.Util;
using FishNet.Transporting.FishyEOSPlugin;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace EOSLobbyTest
{
    public class UIGameModeJoinPanel : UIPanel<UIGameModeJoinPanel>, IUIPanel
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public GameObject GameModeJoin;
        public void CloseGameModeJoin()
        {
            GameModeJoin.SetActive(false);
        }
    }
}
