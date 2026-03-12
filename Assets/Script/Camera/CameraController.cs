using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : Singleton<CameraController>
{
    //Camera
    public CinemachineVirtualCamera virtualCamera;


    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoaded(scene, mode);
        RefreshCamera();
    }
    protected override void Start()
    {
        base.Start();
        RefreshCamera();
    }
    public void RefreshCamera()
    {
        StopAllCoroutines();
        StartCoroutine(WaitToSetCamera());
    }
    IEnumerator WaitToSetCamera()
    {
        if (this.virtualCamera == null)
        {
            this.virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        }

        yield return new WaitUntil(() => PartyManager.Instance != null && PartyManager.Instance.currentLeader != null);

        this.SetCameraFollowCurrentLeader();
    }

    public void SetCameraFollowCurrentLeader()
    {
        if (PartyManager.Instance == null || PartyManager.Instance.currentLeader == null)
        {
            return;
        }

        HeroStateMachine leaderHero = PartyManager.Instance.currentLeader.transform.parent.parent.GetComponent<HeroStateMachine>();

        if (leaderHero != null)
        {
            this.SetCameraFollowHero(leaderHero);
        }
    }

    public void SetCameraFollowHero(HeroStateMachine heroSelected)
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                return;
            }
        }

        this.ResetCamera();

        if (heroSelected == null)
        {
            Debug.LogWarning("Hero is null. Cannot set camera.");
            return;
        }

        Transform body = heroSelected.transform.Find("Body");
        if (body == null)
        {
            Debug.LogWarning($"Body transform not found on {heroSelected.name}");
            return;
        }

        virtualCamera.Follow = body;

        virtualCamera.OnTargetObjectWarped(body, body.position);
    }

    public void SetCameraForCombat(Transform obj)
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogWarning("Virtual Camera is not assigned and could not be found.");
                return;
            }
        }

        this.ResetCamera();

        if (obj == null)
        {
            Debug.LogWarning("Object is null. Cannot set camera.");
            return;
        }

        virtualCamera.Follow = obj;
        virtualCamera.OnTargetObjectWarped(obj, obj.position);
    }

    public void ResetCamera()
    {
        if (this.virtualCamera != null)
        {
            this.virtualCamera.Follow = null;
        }
    }
}