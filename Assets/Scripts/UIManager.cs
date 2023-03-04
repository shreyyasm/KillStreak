using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public NetworkManager _networkManager;
    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }

    public void OnClick_Server()
    {
        if (_networkManager == null)
            return;

        if (_serverState != LocalConnectionState.Stopped)
            _networkManager.ServerManager.StopConnection(true);
        else
            _networkManager.ServerManager.StartConnection();

        DeselectButtons();
    }


    public void OnClick_Client()
    {
        if (_networkManager == null)
            return;

        if (_clientState != LocalConnectionState.Stopped)
            _networkManager.ClientManager.StopConnection();
        else
            _networkManager.ClientManager.StartConnection();

        DeselectButtons();
    }
    private void DeselectButtons()
    {
#if !ENABLE_INPUT_SYSTEM
        SetEventSystem();
        _eventSystem?.SetSelectedGameObject(null);
#endif
    }


}
