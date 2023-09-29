using System.Collections;
using UnityEngine;

/// <summary>
/// Orang Obstacle: Starts moving towards an adjacent lane at the moment the player switches lanes.
/// </summary>
public class OrangeObstacle : GreyObstacle
{
    [SerializeField]
    private float movementSpeed = 1f;

    private int lastLaneInt = 1;
    private Vector3 _targetPosition;

    private bool _isMoving = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        lastLaneInt = CurrentLaneInt;
        PlayerController.OnLaneSwitch += PlayerController_OnLaneSwitch;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        PlayerController.OnLaneSwitch -= PlayerController_OnLaneSwitch;

        StopAllCoroutines();
    }

    private void PlayerController_OnLaneSwitch()
    {
        if (_isMoving) return;

        _isMoving = true;

        lastLaneInt = CurrentLaneInt;
        CurrentLaneInt = LaneManager.GetRandomAdjacentLane(CurrentLaneInt);
        _targetPosition = LaneManager.GetTargetLanePosition(transform.position, CurrentLaneInt);

        _ = StartCoroutine(MoveToLaneCR());
    }

    private IEnumerator MoveToLaneCR()
    {
        yield return MoveToTargetPos(_targetPosition, movementSpeed);
        _isMoving = false;
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

    private void OnTriggerEnter(Collider other)
    {
        // If colliding with another obstacle, restart the coroutine with moving in the other direction.
        if (!_isMoving) return;  // Don't start moving if it the other block is moving into this block while stationary.

        StopAllCoroutines();  // Stop the current movement

        // Switch the target destination back to the previous lane.
        CurrentLaneInt = lastLaneInt;
        _targetPosition = LaneManager.GetTargetLanePosition(transform.position, CurrentLaneInt);

        _ = StartCoroutine(MoveToLaneCR());
    }
/*    private void OnCollisionEnter(Collision collision)
    {
        // If colliding with another obstacle, restart the coroutine with moving in the other direction.
        if (!_isMoving) return;  // Don't start moving if it the other block is moving into this block while stationary.

        StopAllCoroutines();
        CurrentLaneInt = lastLaneInt;
        _targetPosition = LaneManager.GetTargetLanePosition(transform.position, CurrentLaneInt);
        _ = StartCoroutine(MoveToLaneCR());
    }*/

    public override void ResetValues()
    {
        base.ResetValues();
        _isMoving = false;
    }
}
