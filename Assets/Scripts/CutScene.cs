using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class CutScene : MonoBehaviour
{
    public PlayableDirector director;
 
    void Awake()
    {
      
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            director.Play();
        }
    }

}
