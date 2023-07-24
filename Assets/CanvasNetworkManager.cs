using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EOSLobbyTest
{
    public class CanvasNetworkManager : NetworkBehaviour
    {
        [SerializeField] UIPanel4V4Lobby uIPanel4V4Lobby;
        public GameObject myPlayer;
        public Transform container;

        public bool spawned;
     
        void Update()
        {
            uIPanel4V4Lobby = FindObjectOfType<UIPanel4V4Lobby>();
            SetPlayerPrefab();
        }
        public void SetPlayerPrefab()
        {
            foreach(GameObject i in uIPanel4V4Lobby.playersList)
            {
                if (i.GetComponent<UIPlayerItem>().PlayerName == gameObject.GetComponent<PlayerInfo>().PlayerName)
                {
                    myPlayer = i;
                }
            }
        }
        
        public void CheckIfTeamsFull()
        {
            if (base.IsServer)
                CheckIfTeamsFullObserver();

            else
                CheckIfTeamsFullServer();
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void CheckIfTeamsFullServer()
        {

            if (uIPanel4V4Lobby.RedTeam.transform.childCount <= 1)
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedPlayers.container);
            else
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BluePlayers.container);

            spawned = true;
        }
        [ObserversRpc(BufferLast = true)]
        public void CheckIfTeamsFullObserver()
        {
            if (uIPanel4V4Lobby.RedTeam.transform.childCount <= 1)
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedPlayers.container);
            else
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BluePlayers.container);
            spawned = true;
        }

        public void ChangeTeamRedPosition()
        {
            // GameObject myPlayer = GameObject.FindGameObjectWithTag("PlayerPrefab");
            //SetPlayer();
            myPlayer = GameObject.FindGameObjectWithTag("PlayerPrefab");
            if (base.IsServer)
           
                ChangeTeamRedPositionObserver();

            else
                ChangeTeamRedPositionServer();
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void ChangeTeamRedPositionServer()
        {
           
            PlayerInfo playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();
            

            if (uIPanel4V4Lobby.RedTeam.transform.childCount < 2)
            {
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedTeam);

                playerInfo.RedPlayer = true;
                playerInfo.BluePlayer = false;
            }
        }
        [ObserversRpc(BufferLast = true)]
        public void ChangeTeamRedPositionObserver()
        {
           
            PlayerInfo playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();
            

            if (uIPanel4V4Lobby.RedTeam.transform.childCount < 2)
            {
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedTeam);

                playerInfo.RedPlayer = true;
                playerInfo.BluePlayer = false;
            }
        }
        
        public void ChangeTeamBluePosition()
        {

            myPlayer = GameObject.FindGameObjectWithTag("PlayerPrefab");
            // SetPlayer();
            if (base.IsServer)
                ChangeTeamBluePositionObserver();

            else
                ChangeTeamBluePositionServer();
        }

        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void ChangeTeamBluePositionServer()
        {
           
            PlayerInfo playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();

           
            if (uIPanel4V4Lobby.BlueTeam.transform.childCount < 2)
            {
                
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BlueTeam);

                playerInfo.RedPlayer = false;
                playerInfo.BluePlayer = true;
            }
        }
        [ObserversRpc(BufferLast = true)]
        public void ChangeTeamBluePositionObserver()
        {         
            PlayerInfo playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();

            if (uIPanel4V4Lobby.BlueTeam.transform.childCount < 2)
            {
                container = uIPanel4V4Lobby.BlueTeam;
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BlueTeam);

                playerInfo.RedPlayer = false;
                playerInfo.BluePlayer = true;
            }
        }
      
    }
}
