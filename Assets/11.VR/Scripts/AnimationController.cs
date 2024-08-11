using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    public InputActionReference move;
    public Animator animator;

    void OnEnable()
    {
        move.action.started += OnMoveStarted;
        move.action.canceled += OnMoveCanceled;
    }

    void OnDisable()
    {
        move.action.started -= OnMoveStarted;
        move.action.canceled -= OnMoveCanceled;
    }

    void OnMoveStarted(InputAction.CallbackContext context)
    {
        float moveInputY = context.ReadValue<Vector2>().y;

        bool isMovingForward = moveInputY > 0;
        float animationSpeed = isMovingForward ? 1 : -1;

        SetAnimationParameters(true, animationSpeed);
    }

    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        SetAnimationParameters(false, 0);
    }

    void SetAnimationParameters(bool isWalking, float animSpeed)
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetFloat("animSpeed", animSpeed);
    }
}