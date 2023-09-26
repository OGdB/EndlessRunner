using UnityEngine;

public class DifficultyProgression : MonoBehaviour
{
    #region Properties
    [Header("Speed")]
    [Tooltip("At this speed, the speed will stop increasing.")] 
    [SerializeField] private float maxSpeed;

    [Tooltip("The total speed increase per interval.")]
    [SerializeField] private float forwardSpeedIncreaseInterval = 2.5f;

    [SerializeField, Tooltip("The amount of seconds the player will run at a speed before it gradually increases.")]
    private float speedIncreaseIntervalSeconds = 15f;

    [Tooltip("The speed at which the speed increases. Side speed increases proportionate.")]
    [SerializeField] private float _forwardSpeedIncreaseRate = 0.1f;
    private float _sideSpeedIncreaseRate;

    private float _startSpeed;
    private float _currentSpeedTarget;
    private float _nextSpeedIncreaseTime;
    #endregion

    /* Difficulty Progression:
     * Phases until max phase?
     * Linear gradual increase until max number?
     *  Chose linear as I believe it'll have a smoother 'flow' in the user's perspective.
     * 
     * For X amount of seconds, speed will not increase.
     * When the current speedIncreaseTime is reached, speed will start gradually increasing until the current
     * 'Sidemovement' speed increases in proportion with forward movement speed.
     * target speed is reached.
     * Cycle repeating until maxSpeed is reached.
     */

    private void Start()
    {
        _startSpeed = PlayerController.ForwardMovementSpeed;
        _currentSpeedTarget = _startSpeed + forwardSpeedIncreaseInterval;

        _nextSpeedIncreaseTime = Time.time + speedIncreaseIntervalSeconds;

        float forwardSideRatio = PlayerController.SideMovementSpeed / PlayerController.ForwardMovementSpeed;
        _sideSpeedIncreaseRate = forwardSideRatio * _forwardSpeedIncreaseRate;
    }

    private void Update()
    {
        SpeedIncrease();
    }

    private void SpeedIncrease()
    {
        // If no timer or max speed reached, increase forward speed gradually, pause for an interval, repeat.
        float forwardMovementSpeed = PlayerController.ForwardMovementSpeed;
        if (Time.time < _nextSpeedIncreaseTime || forwardMovementSpeed >= maxSpeed) return;  // If on interval OR max speed is reached, return.

        if (forwardMovementSpeed <= _currentSpeedTarget)  // Gradual, proportional speed increase.
        {
            PlayerController.ForwardMovementSpeed += _forwardSpeedIncreaseRate * Time.deltaTime;
            PlayerController.SideMovementSpeed += _sideSpeedIncreaseRate * Time.deltaTime;
            return;
        }

        _currentSpeedTarget = forwardMovementSpeed + forwardSpeedIncreaseInterval;
        _nextSpeedIncreaseTime = Time.time + speedIncreaseIntervalSeconds;
    }
}
