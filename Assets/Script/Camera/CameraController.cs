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
        // Đợi 1 frame để đảm bảo các Manager khác (như PartyManager) đã khởi tạo xong
        yield return null;

        this.virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();

        // Gọi hàm setup riêng biệt và an toàn
        this.SetCameraFollowCurrentLeader();
    }

    // --- HÀM MỚI: Tự động tìm và set Camera theo Đội trưởng hiện tại ---
    public void SetCameraFollowCurrentLeader()
    {
        if (PartyManager.Instance == null || PartyManager.Instance.currentLeader == null)
        {
            Debug.LogWarning("[CameraController] PartyManager hoặc currentLeader đang rỗng. Chưa thể bám theo Đội trưởng!");
            return;
        }

        // Lấy HeroStateMachine từ cấu trúc object Đội trưởng
        HeroStateMachine leaderHero = PartyManager.Instance.currentLeader.transform.parent.parent.GetComponent<HeroStateMachine>();

        if (leaderHero != null)
        {
            this.SetCameraFollowHero(leaderHero);
        }
    }

    public void SetCameraFollowHero(HeroStateMachine heroSelected)
    {
        // 1. Tự động tìm lại Camera nếu bị null (do load map chậm hoặc rớt tham chiếu)
        if (virtualCamera == null)
        {
            virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogWarning("Virtual Camera is not assigned and could not be found.");
                return;
            }
        }

        // 2. An toàn rồi mới gọi Reset
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

        // Focus ngay lập tức (Xóa độ trễ lúc chuyển map hoặc đổi đội trưởng)
        virtualCamera.OnTargetObjectWarped(body, body.position);
    }

    public void SetCameraForCombat(Transform obj)
    {
        // Tự động tìm lại Camera nếu bị null
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
        // Focus ngay lập tức
        virtualCamera.OnTargetObjectWarped(obj, obj.position);
    }

    public void ResetCamera()
    {
        // 3. THÊM ĐIỀU KIỆN IF NÀY: Để đảm bảo không bao giờ bị NullReferenceException
        if (this.virtualCamera != null)
        {
            this.virtualCamera.Follow = null;
        }
    }
}