using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace EOSLobbyTest
{
    public class TeamPlayerNames : MonoBehaviour
    {
        public static TeamPlayerNames Instance;
        public List<TextMeshProUGUI> myPlayersName;
        public List<GameObject> players;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void SetNames()
        {
            if (PlayerManager.Instance.redTeamPlayer)
            {
                for (int t = 0; t < PlayerRespawn.Instance.redPlayersName.Count; t++)
                {
                    myPlayersName[t].text = (t + 1) + ". " + PlayerRespawn.Instance.redPlayersName[t];
                    players[t].SetActive(true);
                    //Debug.Log(PlayerRespawn.Instance.redPlayersName.Count);
                }

            }
            else
            {
                for (int t = 0; t < PlayerRespawn.Instance.bluePlayersName.Count; t++)
                {
                    myPlayersName[t].text = (t + 1) + ". " + PlayerRespawn.Instance.bluePlayersName[t];
                    players[t].SetActive(true);
                }
            }
        }
    }
}
