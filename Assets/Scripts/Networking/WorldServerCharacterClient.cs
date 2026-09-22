using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public sealed class WorldServerCharacterClient : MonoBehaviour
{
    private const string EditorServerBaseUrl = "http://127.0.0.1:5080";
    private const string AndroidServerBaseUrl = "http://192.168.1.8:5080";

    private static string ServerBaseUrl =>
        Application.isEditor ? EditorServerBaseUrl : AndroidServerBaseUrl;

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
