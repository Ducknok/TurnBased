using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animated : DucMonobehaviour
{
    public GameObject breakObj;

    protected override void Start()
    {
        this.breakObj = this.transform.GetComponent<GameObject>();
    }
    public void DisableAnimation()
    {
        this.gameObject.SetActive(false);
    }
}
