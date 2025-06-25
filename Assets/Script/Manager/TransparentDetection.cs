using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TransparentDetection : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] private float transparencyAmount = 0.8f;
    [SerializeField] private float fadeTime = .4f;

    private SpriteRenderer spriteRenderer;
    private Tilemap tileMap;

    private void Awake()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.tileMap = this.GetComponent<Tilemap>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Body"))
        {
            if (spriteRenderer)
            {
                //Debug.LogError("da va vao hero");
                StartCoroutine(FadeRoutine(spriteRenderer, fadeTime, spriteRenderer.color.a, transparencyAmount));
            }
            else if (tileMap)
            {
                //Debug.LogError("da va cham");
                StartCoroutine(FadeRoutine(tileMap, fadeTime, tileMap.color.a, transparencyAmount));
            }
            else return;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Body"))
        {
            if (spriteRenderer)
            {
                StartCoroutine(FadeRoutine(spriteRenderer, fadeTime, spriteRenderer.color.a, 1f));
            }
            else if (tileMap)
            {
                //Debug.LogError("het va cham");
                StartCoroutine(FadeRoutine(tileMap, fadeTime, tileMap.color.a, 1f));
            }
            else return;
        }
    }
    private IEnumerator FadeRoutine(SpriteRenderer spriteRenderer, float fadeTime, float startValue, float targetTransparency)
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, targetTransparency, elapsedTime / fadeTime);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }
    }

    private IEnumerator FadeRoutine(Tilemap tilemap, float fadeTime, float startValue, float targetTransparency)
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, targetTransparency, elapsedTime / fadeTime);
            tilemap.color = new Color(tilemap.color.r, tilemap.color.g, tilemap.color.b, newAlpha);
            yield return null;
        }
    }
}
