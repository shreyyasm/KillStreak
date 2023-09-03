using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
namespace EOSLobbyTest
{
    public class GameManagerEOS : MonoBehaviourSingletonForScene<GameManagerEOS>
    {
        [Tooltip("Prefab object for player controlled object")]
        [SerializeField]
        private GameObject playerPrefab;

        [Tooltip("Transforms for each spawn location")]
        [SerializeField]
        private Transform[] RedTeamSpawnPoints;

        [Tooltip("Transforms for each spawn location")]
        [SerializeField]
        private Transform[] BlueTeamSpawnPoints;


        [SerializeField]
        private List<GameObject> AllPlayers;
        [SerializeField]
        private List<GameObject> RedPlayers;
        [SerializeField]
        private List<GameObject> BluePlayers;

        // which spawn point to use
        private int _nextSpawnPointIndex = 1;
        private int _nextSpawnPointIndexRed = 0;
        private int _nextSpawnPointIndexBlue = 0;

        private void Start()
        {
           
            if (InstanceFinder.NetworkManager != null && InstanceFinder.NetworkManager.IsHost)
            {
                InstanceFinder.SceneManager.OnClientPresenceChangeEnd += SceneManager_OnClientPresenceChangeEnd;
            }

            // if we are testing there will be no vivox instance - this is start in the lobby
            VivoxManager.Instance?.JoinChannel(PlayerManager.Instance?.ActiveLobbyId + "_game", VivoxUnity.ChannelType.Positional, VivoxManager.ChatCapability.AudioOnly, true, null,
               () =>
               {
                   Debug.Log("Connected to vivox positional audio channel");
               });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (InstanceFinder.NetworkManager != null)
            {
                InstanceFinder.SceneManager.OnClientPresenceChangeEnd -= SceneManager_OnClientPresenceChangeEnd;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (UIPanelManager.Instance.PanelIsVisible<UIPanelGameSettings>())
                {
                    UIPanelManager.Instance.HidePanel<UIPanelGameSettings>();
                }
                else if (UIPanelManager.Instance.PanelIsVisible<UIPanelGame>())
                {
                    UIPanelManager.Instance.HidePanel<UIPanelGame>();
                }
                else
                {
                    UIPanelManager.Instance.ShowPanel<UIPanelGame>();
                }
            }
        }
        
        private void SceneManager_OnClientPresenceChangeEnd(FishNet.Managing.Scened.ClientPresenceChangeEventArgs obj)
        {
          
            if (RedTeamSpawnPoints != null && RedTeamSpawnPoints.Length > 0)
            {
                var spawnPointRedTeam = RedTeamSpawnPoints[_nextSpawnPointIndex % RedTeamSpawnPoints.Length];
                var spawnPointBlueTeam = BlueTeamSpawnPoints[_nextSpawnPointIndex % BlueTeamSpawnPoints.Length];

                InstanceFinder.SceneManager.AddConnectionToScene(obj.Connection, SceneManager.GetActiveScene());
                var playerPrefab = Instantiate(this.playerPrefab, spawnPointRedTeam.position, spawnPointRedTeam.rotation);

                InstanceFinder.ServerManager.Spawn(playerPrefab, obj.Connection);
                AllPlayers.Add(playerPrefab);
                StartCoroutine(DelaySeparate());
                //if(PlayerManager.Instance.redTeamPlayer)
                //{

                //   playerVehicle.GetComponent<PlayerGunSelector>().blueTeamPlayer = false;
                //   playerVehicle.GetComponent<PlayerGunSelector>().redTeamPlayer = true;

                //}
                //else
                //{
                //    playerVehicle.GetComponent<PlayerGunSelector>().blueTeamPlayer = true;
                //    playerVehicle.GetComponent<PlayerGunSelector>().redTeamPlayer = false;
                //}

                //if (playerPrefab.GetComponent<PlayerGunSelector>().blueTeamPlayer)
                //{
                //    BluePlayers.Add(playerPrefab);
                //}
                //else
                //{
                //    RedPlayers.Add(playerPrefab);
                //}
                _nextSpawnPointIndex++;
            }
        }
        IEnumerator DelaySeparate()
        {
            yield return new WaitForSeconds(0.1f);
            SeparateTeam();
        }
       public void SeparateTeam()
       {
            foreach (GameObject i in AllPlayers)
            {
                if(i.GetComponent<PlayerGunSelector>().redTeamPlayer)
                {
                    RedPlayers.Add(i);
                }
                if (i.GetComponent<PlayerGunSelector>().blueTeamPlayer)
                {
                    BluePlayers.Add(i);
                }
            }
            //var spawnPointRedTeam = RedTeamSpawnPoints[_nextSpawnPointIndexRed % RedTeamSpawnPoints.Length];
            foreach(GameObject r in RedPlayers)
            {
                r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;
                _nextSpawnPointIndexRed++;
            }
            foreach (GameObject b in BluePlayers)
            {
                b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
                b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
                _nextSpawnPointIndexBlue++;
            }
        }
    }
}
