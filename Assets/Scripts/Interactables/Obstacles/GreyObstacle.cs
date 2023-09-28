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

        RedPowerup.OnRedPowerup += RedPowerup_OnRedPowerup;
    }
    protected override void OnDisable()
    {
        RedPowerup.OnRedPowerup -= RedPowerup_OnRedPowerup;
    }

    /// <summary>
    /// Invoked when the player picks up the Red powerup. Destroys this block if it's on the same lane as the player.
    /// </summary>
    protected void RedPowerup_OnRedPowerup()
    {
        // If on the same lane as the player, destroy (if within X distance?)
        if (CurrentLaneInt == PlayerController.CurrentLaneInt)
        {
            LevelGenerator.EnqueueInteractable(this);
        }
    }
}
