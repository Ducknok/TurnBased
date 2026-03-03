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
        StartCoroutine(WaitToSetCamera());
    }

    IEnumerator WaitToSetCamera()
    {
        yield return null;
        this.virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
        this.SetCameraFollowHero(PartyManager.Instance.currentLeader.transform.parent.parent.GetComponent<HeroStateMachine>());
    }
    public void SetCameraFollowHero(HeroStateMachine heroSelected)
    {
        //Debug.LogError(heroSelected);
        this.ResetCamera();
        if (virtualCamera == null)
        {
            Debug.LogWarning("Virtual Camera is not assigned.");
            return;
        }

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
        // Optional: focus immediately
        virtualCamera.OnTargetObjectWarped(body, body.position);
    }
    public void SetCameraForCombat(Transform obj)
    {
        this.ResetCamera();
        if (virtualCamera == null)
        {
            Debug.LogWarning("Virtual Camera is not assigned.");
            return;
        }

        if (obj == null)
        {
            Debug.LogWarning("Hero is null. Cannot set camera.");
            return;
        }

        virtualCamera.Follow = obj;
        // Optional: focus immediately
        virtualCamera.OnTargetObjectWarped(obj, obj.position);
    }
    public void ResetCamera()
    {
        this.virtualCamera.Follow = null;
    }

}
