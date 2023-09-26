using System;

public class RedPowerup : InteractableBlock
{
    public static event Action OnRedPowerup;

    public override void InteractEffect()
    {
        base.InteractEffect();

        OnRedPowerup?.Invoke();

        Disable();
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}
