using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Responsible for instantiating lanes.
/// </summary>
[DefaultExecutionOrder(-100)]
public class LaneManager : MonoBehaviour
{
    #region Properties
    [SerializeField] private Transform lanePrefab;
    [SerializeField] private float laneWidth = 3f;

    public static List<Transform> Lanes { get; private set; } = new();
    public static int NumberOfLanes { get; set; } = 4;
    public static Action OnLaneAddedLeftSide;
    #endregion

    private void OnEnable()
    {
        GameController.OnGameStart += GameController_OnGameStart;
        GreenPowerup.OnGreenPowerup += AddLane;
    }

    private void OnDisable()
    {
        GameController.OnGameStart -= GameController_OnGameStart;
        GreenPowerup.OnGreenPowerup -= AddLane;
    }

    private void GameController_OnGameStart()
    {
        NumberOfLanes = LevelGenerator.CurrentLevelSettings.startNumberOfLanes;

        Vector3 initialPosition = new(-((NumberOfLanes - 1) * laneWidth) / 2f, -0.5f, 0f);

        // Instantiate lanes based on the number of lanes
        for (int i = 0; i < NumberOfLanes; i++)
        {
            Transform lane = InstantiateLane();
            lane.name = $"Lane {i}";
            // Calculate the position for the current lane
            Vector3 position = initialPosition + Vector3.right * (i * laneWidth);

            // Set the lane's position and rotation
            lane.transform.SetPositionAndRotation(position, Quaternion.identity);

            // Add the lane to the list of lanes
            Lanes.Add(lane);
        }
    }

    /// <summary>
    /// Instantiates a lane on the side the player is closest to.
    /// </summary>
    public void AddLane()
    {
        Vector3 newPosition;
        bool isLeftSide = PlayerController.CurrentLaneInt < (NumberOfLanes - 1) / 2f;

        NumberOfLanes++;

        if (isLeftSide)
        {
            //print("Left side");
            // Add a lane to the left side
            newPosition = Lanes[0].transform.position + Vector3.left * laneWidth;
        }
        else
        {
            //print("Either middle or Right side");
            // Add a lane to the right side
            newPosition = Lanes[Lanes.Count - 1].transform.position + Vector3.right * laneWidth;
        }

        // Instantiate the new lane at the calculated position
        Transform newLane = InstantiateLane();

        newLane.name = $"Lane {NumberOfLanes - 1}";
        newLane.transform.SetPositionAndRotation(newPosition, Quaternion.identity);

        if (isLeftSide)
        {
            // Prepend the new lane to the list of lanes
            Lanes.Insert(0, newLane);

            // Correct the registered current lane numbers of the player and other objects.
            PlayerController.CurrentLaneInt++;
            OnLaneAddedLeftSide?.Invoke();
        }
        else
        {
            // Append the new lane to the list of lanes
            Lanes.Add(newLane);
        }
    }

    private Transform InstantiateLane()
    {
        Transform newLane = Instantiate(lanePrefab, transform);
        var constraint = newLane.GetComponent<PositionConstraint>();
        var constraintSource = new ConstraintSource
        {
            sourceTransform = PlayerController.PlayerTransform,
            weight = 1f
        };
        constraint.AddSource(constraintSource);
        return newLane;
    }

    /// <summary>
    /// Checks whether there is currently an obstacle at the provided index.
    /// </summary>
    /// <param name="targetLaneInt">The integer </param>
    /// <returns>Whether there is a lane on the index.</returns>
    public static bool LaneExists(int targetLaneInt)
    {
        // Is there a lane?
        bool laneExists = targetLaneInt >= 0 && targetLaneInt < NumberOfLanes;

        // Is the player in a state it can switch lanes?
        return laneExists;
    }

    /// <summary>
    /// Calculates and returns the target position for an object in a specific lane.
    /// </summary>
    /// <param name="currentPosition">The current position of the object.</param>
    /// <param name="targetLane">The lane index for the target lane.</param>
    /// <returns>The target position for the object in the specified lane.</returns>
    public static Vector3 GetTargetLanePosition(Vector3 currentPosition, int targetLane)
    {
        Vector3 targetPosition = currentPosition;
        targetPosition.x = GetLaneX(targetLane);
        return targetPosition;
    }

    /// <summary>
    /// Return the world X position of the provided integer.
    /// </summary>
    /// <param name="laneInt"></param>
    /// <returns>The x position of the lane with the provided integer</returns>
    public static float GetLaneX(int laneInt)
    {
        if (!LaneExists(laneInt))
            Debug.LogError($"Tried to get lane outside of bounds ({laneInt})");

        return Lanes[laneInt].transform.position.x;
    }

    /// <summary>
    /// Return the integer of a lane around the provided lane integer.
    /// </summary>
    /// <param name="currentLaneInt">The current lane, </param>
    /// <returns>The integer of an existing, random adjacent lane.</returns>
    public static int GetRandomAdjacentLane(int currentLaneInt)
    {
        // Random direction (1 or -1)
        float randomNumber = UnityEngine.Random.Range(0f, 1f);
        int randomDirection = (randomNumber < 0.5f) ? 1 : -1;

        int targetLane = currentLaneInt + randomDirection;

        // If random direction was past an edge, move the other way.
        if (!LaneExists(targetLane))
            targetLane = currentLaneInt - randomDirection;

        if (LaneExists(targetLane))
            return targetLane;

        return -1;
    }

    /// <summary>
    /// Returns a list of random x-positions for lanes with no duplicates.
    /// </summary>
    /// <param name="numberOfLanes">The total number of lanes.</param>
    /// <returns>A list of random x-positions of lanes with no duplicates.</returns>
    public static List<float> GetRandomLaneXPositions(int numberOfLanes, out List<int> laneInts)
    {
        List<float> laneXPositions = new();
        HashSet<float> uniquePositions = new();
        laneInts = new();

        // Generate unique random positions
        while (uniquePositions.Count < numberOfLanes)
        {
            float randomX = GetRandomLane(out int laneInt);
            laneInts.Add(laneInt);

            if (!uniquePositions.Contains(randomX))
            {
                uniquePositions.Add(randomX);
            }
        }

        // Convert to a list
        laneXPositions.AddRange(uniquePositions);

        return laneXPositions;
    }

    /// <summary>
    /// Generates a random x-position corresponding to a lane in the game.
    /// </summary>
    /// <returns>A random x-position representing a lane.</returns>
    public static float GetRandomLane(out int laneInt)
    {
        laneInt = UnityEngine.Random.Range(0, Lanes.Count);
        float randomX = GetLaneX(laneInt);
        return randomX;
    }

    private void OnDestroy()
    {
        Lanes.Clear();
        Lanes = new();
    }
}

/*
 * Default start with 3 lanes.
 * A lane can be added (Green Powerup). Will be placed on the edge the player is closest to. 
 * Right side if in middle lane. (no particular reason).
 * 
 * 
 */