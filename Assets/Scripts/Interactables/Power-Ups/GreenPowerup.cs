using System;

public class GreenPowerup : InteractableBlock
{
    public static event Action OnGreenPowerup;

    public override void InteractEffect()
    {
        base.InteractEffect();

        OnGreenPowerup?.Invoke();

        Disable();
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }
}
