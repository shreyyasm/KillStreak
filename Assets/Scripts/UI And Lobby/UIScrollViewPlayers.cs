using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EOSLobbyTest
{
    public class UIScrollViewPlayers : MonoBehaviour
    {
        [Tooltip("Prefab for player item")]
        [SerializeField]
        private GameObject playerItemPrefab;

        [Tooltip("Container for the player items")]
        [SerializeField]
        public Transform container;


        public GameObject SpawnedPlayer;

        [SerializeField] UIScrollViewPlayers RedLobby;
        [SerializeField] UIScrollViewPlayers BlueLobby;
        public UIPanel4V4Lobby uIPanel4V4Lobby;

        private void Update()
        {
            ChangePlayerPrefab();
            
        }
        public UIPlayerItem[] GetPlayers()
        {
            return container.GetComponentsInChildren<UIPlayerItem>();
        }
     
        public void ClearPlayers()
        {
            for (var i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
            
        }
        public void ClearPlayersDelay()
        {
            StartCoroutine(DelayClearPlayers());
        }
        IEnumerator DelayClearPlayers()
        {
            yield return new WaitForSeconds(.5f);
            
            for (var i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        public UIPlayerItem AddPlayer(string playerId, string playerName, bool canKick)
        {
            var playerGameObject = GameObject.Instantiate(playerItemPrefab, container);
            //InstanceFinder.ServerManager.Spawn(playerGameObject);
            
            SpawnedPlayer = playerGameObject;
      
            var playerItem = playerGameObject.GetComponent<UIPlayerItem>();
            playerItem.PlayerId = playerId;
            playerItem.PlayerName = playerName;
            playerItem.CanKick = canKick;
            return playerItem;
        }
        public void ChangePlayerPrefab()
        {
            if (SpawnedPlayer == null)
            {
                SpawnedPlayer = RedLobby.SpawnedPlayer;
            }
        }
    }
}
