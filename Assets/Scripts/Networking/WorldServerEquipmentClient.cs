using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public sealed class WorldServerEquipmentClient : MonoBehaviour
{
    private const string ServerBaseUrl = "http://192.168.1.8:5080";
    private const string CharacterId = "fcd658fb-948c-4b57-acfd-a368db4f5c78";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject(nameof(WorldServerEquipmentClient));
        DontDestroyOnLoad(go);
        go.AddComponent<WorldServerEquipmentClient>();
    }

    private IEnumerator Start()
    {
        var url = $"{ServerBaseUrl}/api/characters/{CharacterId}/equipment";

        using var request = UnityWebRequest.Get(url);
        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[WorldServer] Equipment OK: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"[WorldServer] Equipment FAILED: {request.error}");
        }
    }
}
