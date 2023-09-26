using UnityEngine;

public class HitDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            other.TryGetComponent(out InteractableBlock obstacle);
            if (obstacle != null)
            {
                obstacle.InteractEffect();
            }
            else
                Debug.LogWarning("Hit a gameobject on obstacle layer without obstacle script");
        }
    }
}
