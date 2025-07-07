using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CameraShakeManager : DucMonobehaviour
{
    public static CameraShakeManager instance;

    [SerializeField] private float shakeForce = 0.1f;

    protected override void Awake()
    {
        base.Awake();
        if (instance == null) instance = this;
    }
    public void CameraShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(shakeForce);
    }

    internal void CameraShake(object impulseSource)
    {
        throw new NotImplementedException();
    }

    
}
