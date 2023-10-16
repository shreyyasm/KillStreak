using FishNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EOSLobbyTest
{
    public class PlayerManager : MonoBehaviourSingletonPersistent<PlayerManager>
    {
        public UIPanel4V4Lobby uIPanel4V4Lobby;
        [SerializeField] CanvasNetworkManager canvasNetworkManager;
        public List<PlayerInfo> _players = new List<PlayerInfo>();

        //public List<PlayerTeamInfo> PlayerInfos = new List<PlayerTeamInfo>();
        //[Serializable]
        //public class PlayerTeamInfo
        //{
        //    public bool RedPlayer;
        //    public bool BluePlayer;
        //}
        // EOS lobby id we are currently in
        public string ActiveLobbyId { get; set; } = "testing";

        // triggered when ever any changes are done to the players
        public event Action PlayersChanged;

        // the player info for the server
        public PlayerInfo ServerPlayer => _players.FirstOrDefault(x => x.IsServer);

        // the player info for the active client
        public PlayerInfo ActivePlayer => _players.FirstOrDefault(x => x.IsOwner);

        public List<PlayerInfo> GetPlayers()
        {
            return _players;
        }

        public void PlayerUpdated(string userId)
        {
            PlayersChanged?.Invoke();
        }

        public void AddPlayer(PlayerInfo info)
        {
            _players.Add(info);
            
            PlayersChanged?.Invoke();

            StartCoroutine(DelayCheck());
        }
        public GameObject myPlayer;
       
        IEnumerator DelayCheck()
        {
            
            myPlayer = GameObject.FindGameObjectWithTag("Player");
            yield return new WaitForSeconds(0.5f);
            myPlayer.GetComponent<CanvasNetworkManager>().CheckIfTeamsAfterSpawn();
            

            yield return new WaitForSeconds(0.5f);
            myPlayer.GetComponent<CanvasNetworkManager>().CheckIfTeamsFull();


            //SaveNames();

            //uIPanel4V4Lobby.RedPlayers.ChangeContainer();
        }
        public void RemovePlayer(string userId)
        {
            _players.RemoveAll(x => x.UserId == userId);
            PlayersChanged?.Invoke();
        }
        public bool redTeamPlayer;
        public bool blueTeamPlayer;
        
        
        public void SetPlayerTeam()
        {
            PlayerInfo playerInfo = myPlayer.GetComponent<PlayerInfo>();
            redTeamPlayer = playerInfo.RedPlayer;
            blueTeamPlayer = playerInfo.BluePlayer;
        }
        public List<string> RedplayersList;
        public List<string> BlueplayersList;

        public void SaveNames()
        {
            foreach(PlayerInfo i in _players)
            {
                if(i.RedPlayer)
                {
                    string playerName = i.GetComponent<CanvasNetworkManager>().myPlayer.GetComponent<UIPlayerItem>().textPlayerName.text;
                    RedplayersList.Add(playerName);
                }
                if (i.BluePlayer)
                {
                    string playerName = i.GetComponent<CanvasNetworkManager>().myPlayer.GetComponent<UIPlayerItem>().textPlayerName.text;
                    BlueplayersList.Add(playerName);
                }
            }
        }
    }
}
