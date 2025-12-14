using UnityEngine;
using System.Collections;

public class GameFreezeManager : MonoBehaviour
{
    public static GameFreezeManager Instance;

    void Awake() => Instance = this;

    public void Freeze(float duration)
    {
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}
