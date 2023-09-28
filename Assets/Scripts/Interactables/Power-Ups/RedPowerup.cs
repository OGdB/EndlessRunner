using System;

public class RedPowerup : InteractableBlock
{
    public static event Action<int> OnRedPowerup;

    public override void InteractEffect()
    {
        base.InteractEffect();

        OnRedPowerup?.Invoke(CurrentLaneInt);

        gameObject.SetActive(false);
    }
}
