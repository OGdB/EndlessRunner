using UnityEngine;

/// <summary>
/// Blue Obstacle: Moves between lanes in a predictable pattern.
/// </summary>
public class BlueObstacle : GreyObstacle
{
    [SerializeField] 
    private float movementSpeed = 1f;
    [SerializeField] 
    private float movementIntervalSeconds = 2f;

    private Vector3 _targetPosition;
    private int _lastLaneInt;
    private int _currentDirectionInt = 1; // The direction the blue obstacle will (try to) move in. (-1 == left, 1 == right)
    private float _movementTimer; // The time between movement.

    private void Awake() => _movementTimer = movementIntervalSeconds;

    protected override void OnEnable()
    {
        base.OnEnable();

        _lastLaneInt = CurrentLaneInt;

        _targetPosition = transform.position;
        _targetPosition = LaneManager.GetTargetLanePosition(transform.position, CurrentLaneInt);
        _currentDirectionInt = Random.Range(0f, 1f) > 0.5f ? -1 : 1;

        _movementTimer = Random.Range(0f, movementIntervalSeconds);
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;

        // Runtimer while we're on the target position
        if (currentPosition == _targetPosition)
        {
            // Timer
            _movementTimer -= Time.deltaTime;

            // Wait timer over ~ Start Movement (set new target).
            if (_movementTimer <= 0f)
            {
                _movementTimer = movementIntervalSeconds;

                if (!LaneManager.LaneExists(CurrentLaneInt + _currentDirectionInt))
                    _currentDirectionInt *= -1;

                _lastLaneInt = CurrentLaneInt;
                CurrentLaneInt += _currentDirectionInt;

                // Get the next target lane position
                _targetPosition = LaneManager.GetTargetLanePosition(transform.position, CurrentLaneInt);
            }

            return;
        }

        // Move towards the target position
        transform.position = Vector3.MoveTowards(currentPosition, _targetPosition, movementSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (transform.position == _targetPosition) return;  // If at target / rest position

        // If colliding with another obstacle, change direction and reset the timer
        _currentDirectionInt *= -1;
        _targetPosition = LaneManager.GetTargetLanePosition(transform.position, _lastLaneInt);
    }
}
