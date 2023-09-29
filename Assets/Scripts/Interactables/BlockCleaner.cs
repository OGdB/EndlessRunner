using System;
using UnityEngine;

/// <summary>
/// Enqueues blocks back into their queues after the player passed them.
/// Invokees action that the player passed the block.
/// </summary>
public class BlockCleaner : MonoBehaviour
{
    public static Action PassedObstacleLine;

    private static int _lastRegisteredLane = -1;

    private void OnTriggerEnter(Collider other)
    {
        InteractableBlock interactableBlock = other.GetComponent<InteractableBlock>();
        LevelGenerator.EnqueueInteractable(interactableBlock);

        // _lastRegisteredLane == set to the last passed lane.
        // Using thi
        if (_lastRegisteredLane < interactableBlock.RowNumber)
        {
            // If this interactable isn't part of the lane of the previous
            _lastRegisteredLane = interactableBlock.RowNumber;
            PassedObstacleLine?.Invoke();
        }
    }
}
