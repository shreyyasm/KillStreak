using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using EOSLobbyTest;
public class PlayerRespawn : NetworkBehaviour
{
    public static PlayerRespawn Instance;
    CameraFollow player;
    private NetworkConnection ownerConnection;

    [Tooltip("Transforms for each spawn location")]
    [SerializeField]
    public Transform[] RedTeamSpawnPoints;

    [Tooltip("Transforms for each spawn location")]
    [SerializeField]
    public Transform[] BlueTeamSpawnPoints;


    [SerializeField]
    private List<GameObject> AllPlayers;
    [SerializeField]
    private List<GameObject> RedPlayers;
    [SerializeField]
    private List<GameObject> BluePlayers;

    // which spawn point to use
    private int _nextSpawnPointIndex = 1;
    public int _nextSpawnPointIndexRed = 0;
    public int _nextSpawnPointIndexBlue = 0;

    public int playerRedPosIndex = 0;
    public int playerBluePosindex = 0;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
    }
 
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        //StartCoroutine(DelaySeparate());
    }

    private void Update()
    {
        //player = FindObjectOfType<CameraFollow>(); 
    }
    public void Respawn(GameObject player, GameObject playerCanvas, GameObject animatedCanvas)
    {
        StartCoroutine(StartSpawning(player, playerCanvas, animatedCanvas));
        
    }
    IEnumerator StartSpawning(GameObject player, GameObject playerCanvas,GameObject animatedCanvas)
    {
        NetworkObject playerObj = GetComponent<NetworkObject>();
        
        yield return new WaitForSeconds(10f);
        RespawnPlayer(player, playerCanvas, animatedCanvas);
        
        //s
        //player.SetActive(true);
        //InstanceFinder.ServerManager.Spawn(player,base.Owner);
        //playerObj.GiveOwnership(ownerConnection);
        //
        //player.SetActive(true);
        //if(playerCanvas!= null)
        //    playerCanvas.SetActive(true);     
        //if (animatedCanvas != null)
        //    animatedCanvas.SetActive(true);
        //player.GetComponent<PlayerHealth>().playerDead = false;
         

    }
    public void RespawnPlayer(GameObject player, GameObject playerCanvas, GameObject animatedCanvas)
    {

        if (base.IsServer)
            StartRespawnObserver(player,playerCanvas,animatedCanvas);
        else
            StartRespawnServer(player, playerCanvas, animatedCanvas);
    
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void StartRespawnServer(GameObject player, GameObject playerCanvas, GameObject animatedCanvas)
    {
        player.SetActive(true);
        if (playerCanvas != null)
            playerCanvas.SetActive(true);
        if (animatedCanvas != null)
            animatedCanvas.SetActive(true);
        player.GetComponent<PlayerHealth>().playerDead = false;
        player.GetComponent<PlayerHealth>().playerDeadConfirmed = false;
        //player.GetComponent<PlayerHealth>().RespawnAmmoLoadout();
        player.GetComponent<ThirdPersonController>().SetRigWeight();
        player.GetComponent<ThirdPersonController>().ResetPositionPlayer();
        PlayerGunSelector playerGunSelector = GetComponent<PlayerGunSelector>();
        //player.GetComponent<LoadOutManager>().PlayLoadoutSfX();


    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void StartRespawnObserver(GameObject player, GameObject playerCanvas, GameObject animatedCanvas)
    {
        player.SetActive(true);
        if (playerCanvas != null)
            playerCanvas.SetActive(true);
        if (animatedCanvas != null)
            animatedCanvas.SetActive(true);
        player.GetComponent<PlayerHealth>().playerDead = false;
        player.GetComponent<PlayerHealth>().playerDeadConfirmed = false;
        // player.GetComponent<PlayerHealth>().RespawnAmmoLoadout();
        player.GetComponent<ThirdPersonController>().SetRigWeight();
        player.GetComponent<ThirdPersonController>().ResetPositionPlayer();
        PlayerGunSelector playerGunSelector = GetComponent<PlayerGunSelector>();
        

    }
    public void AddPlayers(GameObject playerPrefab)
    {

        if (base.IsServer)
            AddPlayerObserver(playerPrefab);
        else
            AddPlayerServer(playerPrefab);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void AddPlayerServer(GameObject playerPrefab)
    {
        AllPlayers.Add(playerPrefab);
        //DelaySeparateTeam(playerPrefab);      
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void AddPlayerObserver(GameObject playerPrefab)
    {
        AllPlayers.Add(playerPrefab);
        DelaySeparateTeam(playerPrefab);   
    }
   
    public void DelaySeparateTeam(GameObject playerPrefab)
    {
        SeparateTeam(playerPrefab);
        StartCoroutine(AssignPositionDelay());
    }
    public void SeparateTeam(GameObject playerPrefab)
    {
        if (playerPrefab.GetComponent<PlayerGunSelector>().redTeamPlayer)
        {
            RedPlayers.Add(playerPrefab);
            _nextSpawnPointIndexRed++;

        }
        else
        {
            BluePlayers.Add(playerPrefab);
            _nextSpawnPointIndexBlue++;
        }
        //if (base.IsServer)
        //    SeparateTeamObserver(playerPrefab);
        //else
        //    SeparateTeamServer(playerPrefab);
    }
    
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void SeparateTeamServer(GameObject playerPrefab)
    {
        if (playerPrefab.GetComponent<PlayerGunSelector>().redTeamPlayer)
        {
            RedPlayers.Add(playerPrefab);
            _nextSpawnPointIndexRed++;

        }
        else
        {
            BluePlayers.Add(playerPrefab);
            _nextSpawnPointIndexBlue++;
        }
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void SeparateTeamObserver(GameObject playerPrefab)
    {
        if (playerPrefab.GetComponent<PlayerGunSelector>().redTeamPlayer)
        {
            RedPlayers.Add(playerPrefab);
            _nextSpawnPointIndexRed++;

        }
        else
        {
            BluePlayers.Add(playerPrefab);
            _nextSpawnPointIndexBlue++;
        }
    }
    IEnumerator AssignPositionDelay()
    {
        yield return new WaitForSeconds(0.1f);
        AssignPosition();
        yield return new WaitForSeconds(1f);
        SetResetFalse();
    }
    public void AssignPosition()
    {
        if (base.IsServer)
            AssignpositionObserver();
        else
            AssignpositionServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void AssignpositionServer()
    {
        foreach (GameObject r in RedPlayers)
        {
            //r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;

            r.GetComponent<ThirdPersonController>().ResetPosition = true;
            r.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 0;
            //_nextSpawnPointIndexRed++;//
        }
        foreach (GameObject b in BluePlayers)
        {
            //b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
            b.GetComponent<ThirdPersonController>().ResetPosition = true;
            b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
            //_nextSpawnPointIndexBlue++;
        }
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void AssignpositionObserver()
    {
        foreach (GameObject r in RedPlayers)
        {
            //r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;

            r.GetComponent<ThirdPersonController>().ResetPosition = true;
            r.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 0;
            //_nextSpawnPointIndexRed++;
        }
        foreach (GameObject b in BluePlayers)
        {
            //b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
            b.GetComponent<ThirdPersonController>().ResetPosition = true;
            b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
            //_nextSpawnPointIndexBlue++;
        }
    }

    public void ResetPosition()
    {
        _nextSpawnPointIndexRed = 0;
        _nextSpawnPointIndexBlue = 0;
        foreach (GameObject r in RedPlayers)
        {
            Debug.Log("Reset pos");
            //r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;
            r.GetComponent<ThirdPersonController>().ResetPosition = true;
            r.GetComponent<PlayerGunSelector>().PlayerRedPosIndex = playerRedPosIndex;
            r.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 0;
            LoadOutManager loadout = r.GetComponent<LoadOutManager>();
            loadout.GetLoadOutInput(loadout.loadNumber);
            loadout.PlayLoadoutSfX();
            _nextSpawnPointIndexRed++;
            playerRedPosIndex++;
        }
        foreach (GameObject b in BluePlayers)
        {
            // b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
            b.GetComponent<ThirdPersonController>().ResetPosition = true;
            b.GetComponent<PlayerGunSelector>().PlayerBluePosIndex = playerBluePosindex;
            b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
            LoadOutManager loadout = b.GetComponent<LoadOutManager>();
            loadout.GetLoadOutInput(loadout.loadNumber);
            loadout.PlayLoadoutSfX();
            _nextSpawnPointIndexBlue++;
            playerBluePosindex++;
        }
        StartCoroutine(SetRestFalseDelay());
        //if (base.IsServer)
        //    ResetPositionObserver();
        //else
        //    ResetPositionServer();

    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void ResetPositionServer()
    {
        _nextSpawnPointIndexRed = 0;
        _nextSpawnPointIndexBlue = 0;
        foreach (GameObject r in RedPlayers)
        {
            Debug.Log("Reset pos");
            //r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;
            r.GetComponent<ThirdPersonController>().ResetPosition = true;
            r.GetComponent<PlayerGunSelector>().PlayerRedPosIndex = playerRedPosIndex;
            r.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 0;
            LoadOutManager loadout = r.GetComponent<LoadOutManager>();
            loadout.GetLoadOutInput(loadout.loadNumber);
            loadout.PlayLoadoutSfX();
            _nextSpawnPointIndexRed++;
            playerRedPosIndex++;
        }
        foreach (GameObject b in BluePlayers)
        {
            // b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
            b.GetComponent<ThirdPersonController>().ResetPosition = true;
            b.GetComponent<PlayerGunSelector>().PlayerBluePosIndex = playerBluePosindex;
            b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
            LoadOutManager loadout = b.GetComponent<LoadOutManager>();
            loadout.GetLoadOutInput(loadout.loadNumber);
            loadout.PlayLoadoutSfX();
            _nextSpawnPointIndexBlue++;
            playerBluePosindex++;
        }
        StartCoroutine(SetRestFalseDelay());
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void ResetPositionObserver()
    {
        _nextSpawnPointIndexRed = 0;
        _nextSpawnPointIndexBlue = 0;
        foreach (GameObject r in RedPlayers)
        {
            Debug.Log("Reset pos");
            //r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;
            r.GetComponent<ThirdPersonController>().ResetPosition = true;
            r.GetComponent<PlayerGunSelector>().PlayerRedPosIndex = playerRedPosIndex;
            r.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 0;
            LoadOutManager loadout = r.GetComponent<LoadOutManager>();
            loadout.GetLoadOutInput(loadout.loadNumber);
            loadout.PlayLoadoutSfX();
            _nextSpawnPointIndexRed++;
            playerRedPosIndex++;
        }
        foreach (GameObject b in BluePlayers)
        {
            // b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
            b.GetComponent<ThirdPersonController>().ResetPosition = true;
            b.GetComponent<PlayerGunSelector>().PlayerBluePosIndex = playerBluePosindex;
            b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
            LoadOutManager loadout = b.GetComponent<LoadOutManager>();
            loadout.GetLoadOutInput(loadout.loadNumber);
            loadout.PlayLoadoutSfX();
            _nextSpawnPointIndexBlue++;
            playerBluePosindex++;
        }
        StartCoroutine(SetRestFalseDelay());
    }
    public void SetResetFalse()
    {
        if (base.IsServer)
            SetResetFalseObserver();
        else
            SetResetFalseServer();

    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void SetResetFalseServer()
    {
        foreach (GameObject r in RedPlayers)
        {
            //r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;
            r.GetComponent<ThirdPersonController>().ResetPosition = false;
        }
        foreach (GameObject b in BluePlayers)
        {
            //b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
            b.GetComponent<ThirdPersonController>().ResetPosition = false;

        }
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void SetResetFalseObserver()
    {
        foreach (GameObject r in RedPlayers)
        {
            //r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;
            r.GetComponent<ThirdPersonController>().ResetPosition = false;
        }
        foreach (GameObject b in BluePlayers)
        {
            //b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
            b.GetComponent<ThirdPersonController>().ResetPosition = false;

        }
    }
    IEnumerator SetRestFalseDelay()
    {
        yield return new WaitForSeconds(0.1f);
        SetResetFalse();
    }
    public void HideLoadOutButton()
    {
        StartCoroutine(DelayButtonHide());
    }
    IEnumerator DelayButtonHide()
    {
        yield return new WaitForSeconds(10f);
        foreach (GameObject a in AllPlayers)
        {
            if(a.GetComponent<ThirdPersonController>().loadOutButton != null)
                a.GetComponent<ThirdPersonController>().loadOutButton.SetActive(false);
        }
    }
}
