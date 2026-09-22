using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public sealed class WorldServerHandshake : MonoBehaviour
{
    private const string ServerBaseUrl = "http://192.168.1.8:5080";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject(nameof(WorldServerHandshake));
        DontDestroyOnLoad(go);
        go.AddComponent<WorldServerHandshake>();
    }

    private IEnumerator Start()
    {
        var url = $"{ServerBaseUrl}/api/handshake";

        using var request = UnityWebRequest.Get(url);
        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[WorldServer] Handshake OK: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"[WorldServer] Handshake FAILED: {request.error}");
        }
    }
}
