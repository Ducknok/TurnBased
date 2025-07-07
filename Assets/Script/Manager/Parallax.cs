using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : DucMonobehaviour
{
    [SerializeField] private float parallaxOffset = -0.15f;
    private Camera cam;
    private Vector2 startPos;
    private Vector2 travel => (Vector2)cam.transform.position - startPos;

    protected override void Awake()
    {
        this.cam = Camera.main;
    }
    protected override void Start()
    {
        this.startPos = this.transform.position;
    }
    protected override void FixedUpdate()
    {
        this.transform.position = this.startPos + travel * parallaxOffset;
    }
}
