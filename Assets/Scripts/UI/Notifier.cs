using System.Collections;
using UnityEngine;

/// <summary>
/// Can be used to display messages to the player, such as error messages in runtime.
/// </summary>
public class Notifier : MonoBehaviour
{
    private static Notifier Singleton;
    private static GameObject _wrapper;
    private static TMPro.TextMeshProUGUI _text;

    [SerializeField] private float notificationLength = 3.5f;

    private void Awake()
    {
        if (Singleton)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;

        _wrapper = transform.GetChild(0).gameObject;
        _text = GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
    }

    public static void SetNotification(string text)
    {
        Singleton.StartCoroutine(ShowNotificationCR(text));

        static IEnumerator ShowNotificationCR(string text)
        {
            _text.SetText(text);
            _wrapper.SetActive(true);

            yield return new WaitForSeconds(Singleton.notificationLength);

            _wrapper.SetActive(false);
        }
    }

    private void OnDestroy() => Singleton = null;
}
