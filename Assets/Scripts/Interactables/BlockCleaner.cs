using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Enqueues blocks back into their queues after the player passed them.
/// Invokees action that the player passed the block.
/// </summary>
public class BlockCleaner : MonoBehaviour
{
    public static Action PassedObstacleLine;
    private static WaitForSeconds EventCooldownTimer;
    private static bool _cooldown = false;

    private void Awake() => EventCooldownTimer = new(0.5f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            InteractableBlock interactableBlock = other.GetComponent<InteractableBlock>();
            LevelGenerator.EnqueueInteractable(interactableBlock);

            if (!_cooldown)
                StartCoroutine(EventCooldown());
        }
    }

    private IEnumerator EventCooldown()
    {
        PassedObstacleLine?.Invoke();
        _cooldown = true;
        yield return EventCooldownTimer;
        _cooldown = false;
    }
}
