using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public sealed class WorldServerCharacterClient : MonoBehaviour
{
    private const string ServerBaseUrl = "http://192.168.1.8:5080";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject(nameof(WorldServerCharacterClient));
        DontDestroyOnLoad(go);
        go.AddComponent<WorldServerCharacterClient>();
    }

    private IEnumerator Start()
    {
        yield return FetchCharacters();
    }

    private IEnumerator FetchCharacters()
    {
        var url = $"{ServerBaseUrl}/api/characters";

        using var request = UnityWebRequest.Get(url);
        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[WorldServer] Characters OK: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"[WorldServer] Characters FAILED: {request.error}");
        }
    }
}
