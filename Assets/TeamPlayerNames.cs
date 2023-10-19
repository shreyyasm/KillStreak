using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Object;

namespace EOSLobbyTest
{
    public class TeamPlayerNames : NetworkBehaviour
    {
       
        public List<TextMeshProUGUI> myPlayersName;
        public List<GameObject> players;
        // Start is called before the first frame update
        void Start()
        {
            Invoke("SetNames",1.5f);

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void SetNames()
        {
            if (PlayerManager.Instance.redTeamPlayer)
            {
                foreach (string i in PlayerRespawn.Instance.redPlayersName)
                {
                    for (int t = 0; t < PlayerRespawn.Instance.redPlayersName.Count; t++)
                    {
                        myPlayersName[t].text = (t + 1) + ". " + i.ToString();
                        players[t].SetActive(true);
                    }
                }
            }
            else
            {
                foreach (string i in PlayerRespawn.Instance.bluePlayersName)
                {
                    for (int t = 0; t < PlayerRespawn.Instance.bluePlayersName.Count; t++)
                    {
                        myPlayersName[t].text = (t + 1) + ".  " + i.ToString();
                        players[t].SetActive(true);
                    }
                }
            }
            //SetNamesServer();
            //SetNamesObserver();
        }
        [ServerRpc(RequireOwnership = false, RunLocally = true)]
        public void SetNamesServer()
        {
            if(PlayerManager.Instance.redTeamPlayer)
            {
                foreach (string i in PlayerRespawn.Instance.redPlayersName)
                {
                    for (int t = 0; t < PlayerRespawn.Instance.redPlayersName.Count; t++)
                    {
                        myPlayersName[t].text = (t + 1) + ". " + i.ToString();
                        players[t].SetActive(true);
                    }
                }
            }
            else
            {
                foreach (string i in PlayerRespawn.Instance.bluePlayersName)
                {
                    for (int t = 0; t < PlayerRespawn.Instance.bluePlayersName.Count; t++)
                    {
                        myPlayersName[t].text = (t + 1) + ".  " + i.ToString();
                        players[t].SetActive(true);
                    }
                }
            }
            
        }
        [ObserversRpc(BufferLast = true, RunLocally = true)]       
        public void SetNamesObserver()
        {
            if (PlayerManager.Instance.redTeamPlayer)
            {
                foreach (string i in PlayerRespawn.Instance.redPlayersName)
                {
                    for (int t = 0; t < PlayerRespawn.Instance.redPlayersName.Count; t++)
                    {
                        myPlayersName[t].text = (t + 1) + ". " + i.ToString();
                        players[t].SetActive(true);
                    }
                }
            }
            else
            {
                foreach (string i in PlayerRespawn.Instance.bluePlayersName)
                {
                    for (int t = 0; t < PlayerRespawn.Instance.bluePlayersName.Count; t++)
                    {
                        myPlayersName[t].text = (t + 1) + ".  " + i.ToString();
                        players[t].SetActive(true);
                    }
                }
            }
        }
    }
}
