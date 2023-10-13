using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillSystem : NetworkBehaviour
{
    public PlayerGunSelector playerGunSelector;

    [field: SyncVar(ReadPermissions = ReadPermission.ExcludeOwner)]
    public int playerKills { get; [ServerRpc(RequireOwnership = false, RunLocally = true)] set; }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void playerKilled()
    {
        if (base.IsServer)
            PlayerKilledObserver();
        else
            PlayerKilledServer();
    }
    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void PlayerKilledServer()
    {
        playerKills++;
        AddScoreToLeaderboard();
    }
    [ObserversRpc(BufferLast = true, RunLocally = true)]
    public void PlayerKilledObserver()
    {
        playerKills++;
        AddScoreToLeaderboard();
        
    }
    public void AddScoreToLeaderboard()
    {
        if(base.IsOwner)
        {
            if (playerGunSelector.redTeamPlayer)
                PointSystem.Instance.AddScoreToRedTeam();
            else
                PointSystem.Instance.AddScoreToBlueTeam();
        }
        
    }

}
