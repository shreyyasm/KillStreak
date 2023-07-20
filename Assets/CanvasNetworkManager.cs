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
        private void Awake()
        {
            uIPanel4V4Lobby = FindObjectOfType<UIPanel4V4Lobby>();
            
        }
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            StartCoroutine(DelayInstance());
        }
        IEnumerator DelayInstance()
        {
            yield return new WaitForSeconds(0.1f);
            myPlayer = GameObject.FindGameObjectWithTag("PlayerPrefab");
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
           
            if (uIPanel4V4Lobby.RedTeam.transform.childCount < 1)
                uIPanel4V4Lobby.players = uIPanel4V4Lobby.RedPlayers;
            else
                uIPanel4V4Lobby.players = uIPanel4V4Lobby.BluePlayers;
        }
        [ObserversRpc(BufferLast = true)]
        public void CheckIfTeamsFullObserver()
        {
            if (uIPanel4V4Lobby.RedTeam.transform.childCount < 1)
                uIPanel4V4Lobby.players = uIPanel4V4Lobby.RedPlayers;
            else
                uIPanel4V4Lobby.players = uIPanel4V4Lobby.BluePlayers;
        }

        public void ChangeTeamRedPosition()
        {
           // GameObject myPlayer = GameObject.FindGameObjectWithTag("PlayerPrefab");

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
