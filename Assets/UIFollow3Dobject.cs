using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIFollow3Dobject : NetworkBehaviour
{
    public Transform mainCam;
    public Transform target;
    public Transform worldSpaceCanvas;
    public Vector3 offset;

    public TextMeshProUGUI playerNum;

    
    private void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        mainCam = Camera.main.transform;
        transform.SetParent(worldSpaceCanvas);
        SetPlayerNumber();

    }
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (base.Owner.IsLocalClient)
        {
            HideNumber();
        }
       
    }
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position); // look at camera

        transform.position = target.position + offset;
        

    }
    public GameObject canvas;
    public void HideNumber()
    {
        
        canvas.SetActive(false);
        
            
    }
    public PlayerGunSelector playerGunSelector;
    public void SetPlayerNumber()
    {
        playerNum.text = playerGunSelector.playerNumber.ToString();
    }

}