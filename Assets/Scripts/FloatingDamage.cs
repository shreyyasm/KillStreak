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

        anim = GetComponent<Animator>();
      
    }

    // Update is called once per frame
    void Update()
    {
   
        //if (playerAction.IsReloading)
        //{
        //    anim.SetBool("FloatDamage", true  );
        //    return;
        //}
            
        //if (!playerAction.IsShooting)
        //{ 
        //        anim.SetBool("FloatDamage", true);
        //}
        
        //else
        //{
        //        anim.SetBool("FloatDamage", false);
        //}
        
    }
    public void StopFloatDamage()
    {
        anim.SetBool("FloatDamage", false);
        text.SetActive(false);
    }
    public void GetPosition(float damage)
    {
       
        text.SetActive(true);
        floatingText.text = damage.ToString();       
    }
    public void StartFloatDamage()
    {
        StartCoroutine(CheckState());
    }
    IEnumerator CheckState()
    {
        yield return new WaitForSeconds(0.02f);
        anim.SetBool("FloatDamage", true);
    }
    public void SetFloat()
    {
        anim.SetBool("FloatDamage", false);
    }
}
