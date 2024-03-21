using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcLookAt : MonoBehaviour
{
    public Transform player;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if(player != null)
        {
            transform.LookAt(player);
        }
    }
}
