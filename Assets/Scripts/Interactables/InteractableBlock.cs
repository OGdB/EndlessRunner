using UnityEngine;

public class InteractableBlock : MonoBehaviour
{
    private int currentLaneInt = 1;
    protected bool _hit = false; // 'Safety check'; only allow interaction effect to trigger once.

    public int CurrentLaneInt { get => currentLaneInt; set => currentLaneInt = value; }

    protected virtual void OnEnable()
    {
        Vector3 position = transform.position;
        position.x = LaneManager.GetLaneX(CurrentLaneInt);
        transform.position = position;
    }

    public void SetPosition(Vector3 position) => transform.position = position;

    /// <summary>
    /// Invoked when the player touches this block.
    /// </summary>
    public virtual void InteractEffect()
    {
        if (_hit) return;
        _hit = true;
    }

    public virtual void ResetValues() => _hit = false;
}
