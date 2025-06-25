using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaExit : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string sceneTransitionName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Body"))
        {
            
            SceneManager.LoadScene(this.sceneToLoad);
            //Debug.LogError("chua bi load ne");
            
            SceneManagement.Instance.SetTransitionName(sceneTransitionName);
            PlayerController.Instance.LoadComponent();
            CombatController.Instance.LoadComponent();
            PartyManager.Instance.LoadPlayerMove();
        }
    }
}
