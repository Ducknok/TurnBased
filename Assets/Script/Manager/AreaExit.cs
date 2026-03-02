using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaExit : DucMonobehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string sceneTransitionName;

    private float wairToLoadTime = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Body"))
        {
            SceneManagement.Instance.SetTransitionName(sceneTransitionName);
            UIFade.Instance.FadeToBlack();
            StartCoroutine(this.LoadSceneCoroutine());
        }
    }
    private IEnumerator LoadSceneCoroutine()
    {
        while(this.wairToLoadTime >= 0)
        {
            this.wairToLoadTime -= Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene(this.sceneToLoad);
    }
    
}
