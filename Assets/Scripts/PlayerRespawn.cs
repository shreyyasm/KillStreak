using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using EOSLobbyTest;
using System.Linq;
using FishNet.Object.Synchronizing;

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


    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    [SerializeField] public List<GameObject> AllPlayers { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    [SerializeField] public List<GameObject> RedPlayers { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    [SerializeField] public List<GameObject> BluePlayers { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    // which spawn point to use
    private int _nextSpawnPointIndex = 1;
    public int _nextSpawnPointIndexRed = 0;
    public int _nextSpawnPointIndexBlue = 0;

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public int playerRedPosIndex { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public int playerBluePosindex { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        //Invoke("LoadPlayerNames", 1f);
    }
 
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        StartCoroutine(SavePlayerNames());
        //StartCoroutine(DelaySeparate());
    }

    private void Start()
    {
        
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
        player.GetComponent<ThirdPersonController>().SeeInvincibilty();
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
        player.GetComponent<ThirdPersonController>().SeeInvincibilty();


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
        Debug.Log("workAdd");
        DelaySeparateTeam(playerPrefab);
        SetPlayerNum();
        
        //SetPlayerNum();
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void AddPlayerObserver(GameObject playerPrefab)
    {
        AllPlayers.Add(playerPrefab);
       // Debug.Log("workAdd");
        DelaySeparateTeam(playerPrefab);
        SetPlayerNum();
       
    }
   
    public void DelaySeparateTeam(GameObject playerPrefab)
    {
        SeparateTeam(playerPrefab);
        StartCoroutine(AssignPositionDelay());
    }
    public void SeparateTeam(GameObject playerPrefab)
    {

        if (base.IsServer)
            SeparateTeamObserver(playerPrefab);
        else
            SeparateTeamServer(playerPrefab);
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void SeparateTeamServer(GameObject playerPrefab)
    {

        if (playerPrefab.GetComponent<PlayerGunSelector>().redTeamPlayer)
        {
            RedPlayers.Add(playerPrefab);            
            RedPlayers = RedPlayers.Distinct().ToList();
            //redPlayersName.Add(playerPrefab.GetComponent<ThirdPersonController>().PlayerName);
            //_nextSpawnPointIndexRed++;
            //Debug.Log("work");
            //LoadPlayerNames(playerPrefab);
        }
        if(playerPrefab.GetComponent<PlayerGunSelector>().blueTeamPlayer)
        {
            BluePlayers.Add(playerPrefab);
            BluePlayers = BluePlayers.Distinct().ToList();
            //bluePlayersName.Add(playerPrefab.GetComponent<ThirdPersonController>().PlayerName);
            //_nextSpawnPointIndexBlue++;
            //LoadPlayerNames(playerPrefab);
        }
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void SeparateTeamObserver(GameObject playerPrefab)
    {

        if (playerPrefab.GetComponent<PlayerGunSelector>().redTeamPlayer)
        {
            RedPlayers.Add(playerPrefab);
            RedPlayers = RedPlayers.Distinct().ToList();
        }
        if(playerPrefab.GetComponent<PlayerGunSelector>().blueTeamPlayer)
        {
            BluePlayers.Add(playerPrefab);
            BluePlayers = BluePlayers.Distinct().ToList();
        }
    }
    public void SetPositionIndex()
    {

        //for(int i = 0; i < RedPlayers.Count; i++)
        //{
        //    RedPlayers[i].GetComponent<PlayerGunSelector>().PlayerRedPosIndex = playerRedPosIndex;
        //    playerRedPosIndex++;
        //    _nextSpawnPointIndexRed++;
        //}
        //for (int i = 0; i < BluePlayers.Count; i++)
        //{
        //    BluePlayers[i].GetComponent<PlayerGunSelector>().PlayerBluePosIndex = playerBluePosindex;
        //    playerBluePosindex++;
        //    _nextSpawnPointIndexBlue++;
        //}
        
    }
    IEnumerator AssignPositionDelay()
    {
        yield return new WaitForSeconds(0.1f);
        AssignPosition();
        SetPositionIndex();
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
            r.GetComponent<ThirdPersonController>().ResetPosition = true;
            r.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 0;
            r.GetComponent<PlayerGunSelector>().SetPlayerPositionIndex();
            //r.GetComponent<PlayerGunSelector>().PlayerRedPosIndex = playerRedPosIndex;
            //playerRedPosIndex++;
            //_nextSpawnPointIndexRed++;
        }
        foreach (GameObject b in BluePlayers)
        {
            b.GetComponent<ThirdPersonController>().ResetPosition = true;
            b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
            b.GetComponent<PlayerGunSelector>().SetPlayerPositionIndex();
            //b.GetComponent<PlayerGunSelector>().PlayerBluePosIndex = playerBluePosindex;
            //playerBluePosindex++;
            //_nextSpawnPointIndexBlue++;
        }

    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void AssignpositionObserver()
    {      
        foreach (GameObject r in RedPlayers)
        {
            r.GetComponent<ThirdPersonController>().ResetPosition = true;
            r.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 0;
            r.GetComponent<PlayerGunSelector>().SetPlayerPositionIndex();
            //r.GetComponent<PlayerGunSelector>().PlayerRedPosIndex = playerRedPosIndex;
            //playerRedPosIndex++;
            //_nextSpawnPointIndexRed++;
        }
        foreach (GameObject b in BluePlayers)
        {
            b.GetComponent<ThirdPersonController>().ResetPosition = true;
            b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
            b.GetComponent<PlayerGunSelector>().SetPlayerPositionIndex();
            //b.GetComponent<PlayerGunSelector>().PlayerBluePosIndex = playerBluePosindex;
            //playerBluePosindex++;
            //_nextSpawnPointIndexBlue++;
        }
    }

    public void ResetPosition()
    {
        //_nextSpawnPointIndexRed = 0;
        //_nextSpawnPointIndexBlue = 0;
        foreach (GameObject r in RedPlayers)
        {
            //Debug.Log("Reset pos");
            //r.transform.position = RedTeamSpawnPoints[_nextSpawnPointIndexRed].transform.position;
            r.GetComponent<ThirdPersonController>().ResetPosition = true;
            //r.GetComponent<PlayerGunSelector>().PlayerRedPosIndex = playerRedPosIndex;
            r.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 0;
            r.GetComponent<ThirdPersonController>().SeeInvincibilty();
            LoadOutManager loadout = r.GetComponent<LoadOutManager>();
            loadout.GetLoadOutInput(loadout.loadNumber);
            loadout.PlayLoadoutSfX();
            //_nextSpawnPointIndexRed++;
            //playerRedPosIndex++;
        }
        foreach (GameObject b in BluePlayers)
        {
            // b.transform.position = BlueTeamSpawnPoints[_nextSpawnPointIndexBlue].transform.position;
            b.GetComponent<ThirdPersonController>().ResetPosition = true;
            //b.GetComponent<PlayerGunSelector>().PlayerBluePosIndex = playerBluePosindex;
            b.GetComponent<ThirdPersonController>()._cinemachineTargetYaw = 180;
            b.GetComponent<ThirdPersonController>().SeeInvincibilty();
            LoadOutManager loadout = b.GetComponent<LoadOutManager>();
            loadout.GetLoadOutInput(loadout.loadNumber);
            loadout.PlayLoadoutSfX();
            //_nextSpawnPointIndexBlue++;
            //playerBluePosindex++;
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
    public void SetPlayerNum()
    {
        foreach(GameObject i in RedPlayers)
        {
            for(int t = 1; t <= RedPlayers.Count; t++)
            {
                
                i.GetComponent<PlayerGunSelector>().playerNumber = t;
            }
        }
        foreach (GameObject i in BluePlayers)
        {
            for (int t = 1; t <= BluePlayers.Count; t++)
            {
                //Debug.Log(t);
                i.GetComponent<PlayerGunSelector>().playerNumber = t;
            }
        }
    }
   public void DisableOutlineRedPlayers()
   {
        foreach(GameObject i in RedPlayers)
        {
            i.GetComponent<ThirdPersonController>().playerMainBody.GetComponent<Outline>().enabled = false;
        }
   }
    public void DisableOutlineBluePlayers()
    {
        foreach (GameObject i in BluePlayers)
        {
            i.GetComponent<ThirdPersonController>().playerMainBody.GetComponent<Outline>().enabled = false;
        }
    }

    public List<string> redPlayersName;
    public List<string> bluePlayersName;

    public void LoadPlayerNames()
    {
        if (base.IsServer)
            LoadPlayerNamesObserver();
        else
            LoadPlayerNamesServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void LoadPlayerNamesServer()
    {
        foreach (GameObject i in AllPlayers)
        {
            if (i.GetComponent<PlayerGunSelector>().redTeamPlayer)
            {
                redPlayersName.Add(i.GetComponent<ThirdPersonController>().PlayerName);
                redPlayersName = redPlayersName.Distinct().ToList();
                //Debug.Log("work");
            }

            if (i.GetComponent<PlayerGunSelector>().blueTeamPlayer)
            {
                bluePlayersName.Add(i.GetComponent<ThirdPersonController>().PlayerName);
                bluePlayersName = bluePlayersName.Distinct().ToList();
            }
        }
        TeamPlayerNames.Instance.SetNames();
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void LoadPlayerNamesObserver()
    {
        foreach (GameObject i in AllPlayers)
        {
            if (i.GetComponent<PlayerGunSelector>().redTeamPlayer)
            {
                redPlayersName.Add(i.GetComponent<ThirdPersonController>().PlayerName);
                redPlayersName = redPlayersName.Distinct().ToList();
                //Debug.Log("work");
            }

            if (i.GetComponent<PlayerGunSelector>().blueTeamPlayer)
            {
                bluePlayersName.Add(i.GetComponent<ThirdPersonController>().PlayerName);
                bluePlayersName = bluePlayersName.Distinct().ToList();
            }         
        }
        TeamPlayerNames.Instance.SetNames();
    }
    IEnumerator SavePlayerNames()
    {
        yield return new WaitForSeconds(1f);
        LoadPlayerNames();
    }
}
