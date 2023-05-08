 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FloatingDamage : MonoBehaviour
{
    public TextMeshProUGUI floatingText;
    public GameObject text;
    public static FloatingDamage Instance;
    Transform mainCam;
    Transform unit;
    public Transform worldSpaceCanvas;
    public Vector3 offset;
    Vector2 PosPlayer;
    public Animator anim;
    public PlayerAction playerAction;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main.transform;
        unit = transform.parent;
        anim = GetComponent<Animator>();
        transform.SetParent(worldSpaceCanvas);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        //transform.position = unit.position + offset;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("FloatDamage") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
        {
            text.SetActive(false);
        }
        if (playerAction.IsReloading)
        {
            anim.SetBool("FloatDamage", true  );
            return;
        }
            
        if (!playerAction.IsShooting)
        { 
                anim.SetBool("FloatDamage", true);
        }
        
        else
        {

            //anim.StopPlayback();
           
                anim.SetBool("FloatDamage", false);
            //anim.StopPlayback();
        }
        
    }
    public void GetPosition(Transform Pos, float damage)
    {
        //transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        //PosPlayer.x = Pos.position.x + offset.x;
        //PosPlayer.y = Pos.position.y + offset.y;
        //transform.localPosition = Pos.transform.localPosition + offset;
        text.SetActive(true);
        
        
        floatingText.text = damage.ToString();       
        //anim.StartPlayback();
        
    }
    public IEnumerator StartAnimation()
    {
        //anim.StopPlayback();
        yield return new WaitForSeconds(0.8f);
        
    }
    public void CallStartAnimation()
    {
        StartCoroutine(StartAnimation());
    }
}
