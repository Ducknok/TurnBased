using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float parallaxOffset = -0.15f;
    private Camera cam;
    private Vector2 startPos;
    private Vector2 travel => (Vector2)cam.transform.position - startPos;

    private void Awake()
    {
        this.cam = Camera.main;
    }
    private void Start()
    {
        this.startPos = this.transform.position;
    }
    private void FixedUpdate()
    {
        this.transform.position = this.startPos + travel * parallaxOffset;
    }
}
