using System.Collections;
using UnityEngine;

/// <summary>
/// Detection of the player hitting any blocks on the interactable layer.
/// </summary>
public class HitDetection : MonoBehaviour
{
    [SerializeField] private float immunityLength = 3f;
    [SerializeField, Range(0.1f, 1f)] private float speedMultiplierOnObstacleHit = 0.6f;
    [SerializeField] private float blinkIntervalSpeed = 0.6f;
    [SerializeField] private float alphaOnBlink = 0.4f;

    private MeshRenderer _playerRenderer;
    private Material _playerMaterial;
    private bool _immune = false;

    private void Awake()
    {
        _playerRenderer = GetComponentInChildren<MeshRenderer>();
        _playerMaterial = _playerRenderer.sharedMaterial;
    }

/*    private void OnCollisionEnter(Collision collision)
    {
        bool isInteractable = collision.gameObject.layer == LayerMask.NameToLayer("Interactable");
        if (isInteractable)
        {
            collision.gameObject.TryGetComponent(out InteractableBlock interactable);
            if (interactable != null)
            {
                interactable.InteractEffect();

                bool isObstacle = interactable.GetType().IsSubclassOf(typeof(GreyObstacle)) || interactable.GetType() == typeof(GreyObstacle);
                if (isObstacle && !_immune)
                {
                    OnObstacleHit();
                }
            }
            else
            {
                Debug.LogWarning("Hit a gameobject on obstacle layer without obstacle script");
            }
        }
    }*/
    private void OnTriggerEnter(Collider other)
    {
        bool isInteractable = other.gameObject.layer == LayerMask.NameToLayer("Interactable");
        if (isInteractable)
        {
            other.TryGetComponent(out InteractableBlock interactable);
            if (interactable != null)
            {
                interactable.InteractEffect();

                bool isObstacle = interactable.GetType().IsSubclassOf(typeof(GreyObstacle)) || interactable.GetType() == typeof(GreyObstacle);
                if (isObstacle && !_immune)
                {
                    OnObstacleHit();
                }
            }
            else
            {
                Debug.LogWarning("Hit a gameobject on obstacle layer without obstacle script");
            }
        }
    }

    private void OnObstacleHit()
    {
        if (!_immune)
        {
            _ = StartCoroutine(BlinkEffectCR());

            GameController.Lives--;
        }

        IEnumerator BlinkEffectCR()
        {
            _immune = true;
            float timer = 0;
            WaitForSeconds blinkInterval = new(blinkIntervalSpeed);

            Color startColor = _playerMaterial.color;
            Color nextColor = startColor;
            nextColor.a = alphaOnBlink;
            float timeStamp = Time.time;

            PlayerController.SpeedMultiplier = speedMultiplierOnObstacleHit;

            while (timer <= immunityLength)
            {
                timer += Time.time - timeStamp;

                _playerRenderer.material.color = nextColor;

                yield return blinkInterval;

                nextColor.a = nextColor.a == alphaOnBlink ? 1f : alphaOnBlink;
            }

            PlayerController.SpeedMultiplier = 1f;

            _playerMaterial.color = startColor;
            _immune = false;
        }
    }
}