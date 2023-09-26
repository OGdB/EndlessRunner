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
        PlayerController.OnLaneSwitch += OnPlayerSwitchesLane;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        PlayerController.OnLaneSwitch -= OnPlayerSwitchesLane;
    }

    private void OnPlayerSwitchesLane()
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

    private void OnTriggerEnter(Collider other)
    {
        // If colliding with another obstacle, restart the coroutine with moving in the other direction.
        if (other.gameObject.layer != LayerMask.NameToLayer("Interactable") && !_isMoving) return;

        StopAllCoroutines();
        CurrentLaneInt = lastLaneInt;
        _targetPosition = LaneManager.GetTargetLanePosition(transform.position, CurrentLaneInt);
        _ = StartCoroutine(MoveToLaneCR());
    }

    public override void ResetValues()
    {
        base.ResetValues();
        _isMoving = false;
    }
}
