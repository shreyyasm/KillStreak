using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace EOSLobbyTest
{
    public class TeamPlayerNames : MonoBehaviour
    {
        public List<TextMeshProUGUI> myPlayersName;
        public List<GameObject> players;
        // Start is called before the first frame update
        void Start()
        {
            SetNames();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void SetNames()
        {
            if(PlayerManager.Instance.redTeamPlayer)
            {
                foreach (string i in PlayerManager.Instance.RedplayersList)
                {
                    for (int t = 0; t < PlayerManager.Instance.RedplayersList.Count; t++)
                    {
                        myPlayersName[t].text = (t + 1) + ". " + i.ToString();
                        players[t].SetActive(true);
                    }
                }
            }
            else
            {
                foreach (string i in PlayerManager.Instance.BlueplayersList)
                {
                    for (int t = 0; t < PlayerManager.Instance.BlueplayersList.Count; t++)
                    {
                        myPlayersName[t].text = (t + 1) + ".  " + i.ToString();
                        players[t].SetActive(true);
                    }
                }
            }
            
        }
    }
}
