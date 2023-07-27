using FishNet.Object;
using FishNet.Object.Synchronizing;
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

        [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
        public bool spawned { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

        void Update()
        {
            uIPanel4V4Lobby = FindObjectOfType<UIPanel4V4Lobby>();
            Debug.Log(uIPanel4V4Lobby.RedTeam.transform.childCount);
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
            if(!spawned)
            {
                if (base.IsServer)
                    CheckIfTeamsFullObserver();

                else
                    CheckIfTeamsFullServer();
            }           
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void CheckIfTeamsFullServer()
        {
            
            if (uIPanel4V4Lobby.RedTeam.transform.childCount <= 1)
            {
                PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();
                playerInfo.RedPlayer = true;
                playerInfo.BluePlayer = false;     
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedPlayers.container);
            }
                
            else
            {
                PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();
                playerInfo.RedPlayer = false;
                playerInfo.BluePlayer = true;
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BluePlayers.container);
            }

            spawned = true;
            PlayerManager.Instance.SetPlayerTeam();
        }
        [ObserversRpc(BufferLast = true)]
        public void CheckIfTeamsFullObserver()
        {

            if (uIPanel4V4Lobby.RedTeam.transform.childCount <= 1)
            {
                PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();
                playerInfo.RedPlayer = true;
                playerInfo.BluePlayer = false;
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedPlayers.container);
            }

            else
            {
                PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();
                playerInfo.RedPlayer = false;
                playerInfo.BluePlayer = true;
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BluePlayers.container);
            }
            PlayerManager.Instance.SetPlayerTeam();
            spawned = true;
        }
       
        public void CheckIfTeamsAfterSpawn()
        {
            
            if (base.IsServer)
                CheckIfTeamsAfterSpawnObserver();

            else
                CheckIfTeamsAfterSpawnServer();
        }
        [ServerRpc(RequireOwnership = false, RunLocally = false)]
        public void CheckIfTeamsAfterSpawnServer()
        {
            PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();
            if (playerInfo.RedPlayer)
            {
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedPlayers.container);
            }
            else
            {
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BluePlayers.container);
            }         
        }
        [ObserversRpc(BufferLast = true, RunLocally = false)]
        public void CheckIfTeamsAfterSpawnObserver()
        {
            PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();
            if (playerInfo.RedPlayer)
            {
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedPlayers.container);
            }
            else
            {
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BluePlayers.container);
            }
        }

        public void ChangeTeamRedPosition()
        {

            if (base.IsServer)
           
                ChangeTeamRedPositionObserver();

            else
                ChangeTeamRedPositionServer();
        }
        
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void ChangeTeamRedPositionServer()
        {
           
           PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();

            if (uIPanel4V4Lobby.RedTeam.transform.childCount < 1)
            {
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedTeam);

                playerInfo.RedPlayer = true;
                playerInfo.BluePlayer = false;
            }
            PlayerManager.Instance.SetPlayerTeam();
        }
        [ObserversRpc(BufferLast = true)]
        public void ChangeTeamRedPositionObserver()
        {

            PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();
            if (uIPanel4V4Lobby.RedTeam.transform.childCount < 1)
            {
                myPlayer.transform.SetParent(uIPanel4V4Lobby.RedTeam);

                playerInfo.RedPlayer = true;
                playerInfo.BluePlayer = false;
            }
            PlayerManager.Instance.SetPlayerTeam();
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

            PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();

            if (uIPanel4V4Lobby.BlueTeam.transform.childCount < 1)
            {
                
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BlueTeam);

                playerInfo.RedPlayer = false;
                playerInfo.BluePlayer = true;
            }
            PlayerManager.Instance.SetPlayerTeam();
        }
        [ObserversRpc(BufferLast = true)]
        public void ChangeTeamBluePositionObserver()
        {
            PlayerInfo playerInfo = gameObject.GetComponent<PlayerInfo>();

            if (uIPanel4V4Lobby.BlueTeam.transform.childCount < 1)
            {
                container = uIPanel4V4Lobby.BlueTeam;
                myPlayer.transform.SetParent(uIPanel4V4Lobby.BlueTeam);

                playerInfo.RedPlayer = false;
                playerInfo.BluePlayer = true;
            }
            PlayerManager.Instance.SetPlayerTeam();
        }
      
    }
}
