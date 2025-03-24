using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animated : MonoBehaviour
{
    public GameObject breakObj;

    protected void Start()
    {
        this.breakObj = this.transform.GetComponent<GameObject>();
    }
    public void DisableAnimation()
    {
        this.gameObject.SetActive(false);
    }
}
