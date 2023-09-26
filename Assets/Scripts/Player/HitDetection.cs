using System.Collections;
using UnityEngine;

/// <summary>
/// Detection of the player hitting any blocks on the interactable layer.
/// </summary>
public class HitDetection : MonoBehaviour
{
    [SerializeField]
    private float immunityLength = 3f;
    [SerializeField]
    private float blinkIntervalSpeed = 0.6f;
    [SerializeField]
    private float alphaOnBlink = 0.4f;
    private bool _blinking = false;

    private MeshRenderer _playerRenderer;
    private Material _playerMaterial;

    private void Awake()
    {
        _playerRenderer = GetComponentInChildren<MeshRenderer>();
        _playerMaterial = _playerRenderer.sharedMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            other.TryGetComponent(out InteractableBlock obstacle);
            if (obstacle != null)
            {
                obstacle.InteractEffect();
                OnObstacleHit();
            }
            else
                Debug.LogWarning("Hit a gameobject on obstacle layer without obstacle script");
        }
    }

    private void OnObstacleHit()
    {
        if (!_blinking)
            _ = StartCoroutine(BlinkEffectCR());

        IEnumerator BlinkEffectCR()
        {
            _blinking = true;
            float timer = 0;
            WaitForSeconds blinkInterval = new(blinkIntervalSpeed);

            Color startColor = _playerMaterial.color;
            Color nextColor = startColor;
            nextColor.a = alphaOnBlink;
            float timeStamp = Time.time;

            while (timer <= immunityLength)
            {
                timer += Time.time - timeStamp;

                _playerRenderer.material.color = nextColor; // Modify the base color property.

                yield return blinkInterval; // Wait for the specified interval.
                print("Blink");
                nextColor.a = nextColor.a == alphaOnBlink ? 1f : alphaOnBlink;
            }
            print("Finished blink");

            _playerMaterial.color = startColor;
            _blinking = false;
        }
    }
}
