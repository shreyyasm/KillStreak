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
            //OpenGameOverCanvas();
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
        public List<TextMeshProUGUI> RedPlayers;
        public List<TextMeshProUGUI> BluePlayers;

        public List<TextMeshProUGUI> RedPlayersKills;
        public List<TextMeshProUGUI> BluePlayersKills;
        public void SetFinalNamesNScore()
        {
            for (int t = 0; t < PlayerRespawn.Instance.RedPlayers.Count; t++)
            {
                RedPlayers[t].text = (t) + ". " + PlayerRespawn.Instance.redPlayersName[t];
                RedPlayers[t].enabled = true;

                RedPlayersKills[t].text = "Kills: " + PlayerRespawn.Instance.RedPlayers[t].GetComponent<KillSystem>().playerKills;
                RedPlayersKills[t].enabled = true;
            }
      
            for (int t = 0; t < PlayerRespawn.Instance.BluePlayers.Count; t++)
            {
                BluePlayers[t].text = (t) + ". " + PlayerRespawn.Instance.bluePlayersName[t];
                BluePlayers[t].enabled = true;

                BluePlayersKills[t].text = "Kills: " + PlayerRespawn.Instance.BluePlayers[t].GetComponent<KillSystem>().playerKills;
                BluePlayersKills[t].enabled = true;
            }      
        }
        public GameObject GameOverCanvas;
        public TextMeshProUGUI Victory;
        public TextMeshProUGUI Defeat;
        public void ShowConclusion()
        {

            if (PointSystem.Instance.RedTeamScore > PointSystem.Instance.BlueTeamScore)
            {
                if (PlayerManager.Instance.redTeamPlayer)
                {
                    Victory.enabled = true;
                    Defeat.enabled = false;
                }
                else
                {
                    Victory.enabled = false;
                    Defeat.enabled = true;
                }
            }
            if (PointSystem.Instance.RedTeamScore < PointSystem.Instance.BlueTeamScore)
            {
                if (PlayerManager.Instance.redTeamPlayer)
                {
                    Victory.enabled = false;
                    Defeat.enabled = true;

                }
                else
                {
                    Victory.enabled = true;
                    Defeat.enabled = false;
                }
            }
        }
        public void OpenGameOverCanvas()
        {
            StartCoroutine(ShowGameOver());
        }
        IEnumerator ShowGameOver()
        {
            yield return new WaitForSeconds(10f);
            GameOverCanvas.SetActive(true);
            ShowConclusion();
            SetFinalNamesNScore();
            yield return new WaitForSeconds(10f);
            PointSystem.Instance.LeaveLobby();
        }

    }
}
