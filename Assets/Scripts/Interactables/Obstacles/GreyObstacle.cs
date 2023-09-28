using System.Collections;
using UnityEngine;

/// <summary>
/// Grey Obstacle: a 'neutral' obstacle: static in 1 lane, hurts the player when he hits it.
/// This obstacle has formed the basis for the over obstacles, inheriting from this block.
/// </summary>
public class GreyObstacle : InteractableBlock
{
    protected static WaitForFixedUpdate FUpdate;

    private void Awake() => FUpdate = new();

    protected override void OnEnable()
    {
        base.OnEnable();

        RedPowerup.OnRedPowerup += OnRedPowerup;
    }
    protected override void OnDisable()
    {
        RedPowerup.OnRedPowerup -= OnRedPowerup;
    }

    /// <summary>
    /// Invoked when the player picks up the Red powerup. Destroys this block if it's on the same lane as the player.
    /// </summary>
    protected void OnRedPowerup()
    {
        // If on the same lane as the player, destroy (if within X distance?)
        if (CurrentLaneInt == PlayerController.CurrentLaneInt)
        {
            LevelGenerator.EnqueueInteractable(this);
        }
    }

    /// <summary>
    /// Smoothly moves the block to a specified target position.
    /// </summary>
    /// <param name="targetLanePos">The target position to move the block to.</param>
    /// <param name="movementSpeed">The speed at which the block moves towards the target.</param>
    /// <returns>An IEnumerator for coroutine usage.</returns>
    protected IEnumerator MoveToTargetPos(Vector3 targetLanePos, float movementSpeed)
    {
        // Move towards the target position as long as it's not on that position.
        while (transform.position != targetLanePos)
        {
            Vector3 nextPos = Vector3.MoveTowards(transform.position, targetLanePos, movementSpeed * Time.deltaTime);
            transform.position = nextPos;

            yield return FUpdate;
        }

        transform.position = targetLanePos;
    }
}
