using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static Vector2 Movement;

    private PlayerInput playerInput;
    private InputAction moveAction;

    private void Awake()
    {
        this.playerInput = GetComponent<PlayerInput>();
        this.moveAction = this.playerInput.actions["Move"];
    }

    private void Update()
    {
        Movement = this.moveAction.ReadValue<Vector2>();
    }

}
