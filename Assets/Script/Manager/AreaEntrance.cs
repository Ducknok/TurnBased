using Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaEntrance : DucMonobehaviour
{
    [SerializeField] private string transitionName;

    protected override void Start()
    {
        StartCoroutine(SetPlayerPositionDelayed());
        PlayerController.Instance.LoadComponent();
        CombatController.Instance.LoadComponent();
        ItemInventoryController.Instance.LoadInventory();
        PartyManager.Instance.LoadPlayerMove();
        PartyManager.Instance.ClearLeaderPosition();
        UIFade.Instance.FadeToClear();
    }

    private IEnumerator SetPlayerPositionDelayed()
    {
        // Đợi đến khi CombatController và CBM sẵn sàng
        yield return new WaitUntil(() =>
            CombatController.Instance != null &&
            CombatController.Instance.CBM != null &&
            CombatController.Instance.CBM.playersInCombat != null
        );

       // Debug.LogError(transitionName);
        //Debug.LogError(SceneManagement.Instance.sceneTransitionName);
        if (this.transitionName == SceneManagement.Instance.sceneTransitionName)
        {

            foreach (var hero in CombatController.Instance.CBM.playersInCombat)
            {
                if (hero == null)
                {
                    continue;
                }

                HeroStateMachine hsm = hero.GetComponent<HeroStateMachine>();
                if (hsm != null)
                {
                    Transform body = hsm.transform.Find("Body");
                    if (body != null)
                    {
                        //Debug.LogError("am here");
                        body.position = this.transform.position;
                    }
                }
            }
        }
    }
}
