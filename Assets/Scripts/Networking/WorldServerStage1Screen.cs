using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public sealed class WorldServerStage1Screen : MonoBehaviour
{
    private const string EditorServerBaseUrl = "http://127.0.0.1:5080";
    private const string AndroidServerBaseUrl = "http://192.168.1.8:5080";

    private static string ServerBaseUrl =>
        Application.isEditor ? EditorServerBaseUrl : AndroidServerBaseUrl;

    private TMP_Text statusText;

    [Serializable]
    private sealed class CharacterDto
    {
        public string id;
        public string name;
        public string faction;
        public int level;
        public long dharmaPoints;
        public string createdAtUtc;
    }

    [Serializable]
    private sealed class CharacterListWrapper
    {
        public CharacterDto[] items;
    }

    [Serializable]
    private sealed class EquipmentDto
    {
        public string id;
        public string slot;
        public string itemCode;
        public int refinementLevel;
        public bool isTradable;
        public string equippedAtUtc;
    }

    [Serializable]
    private sealed class EquipmentListWrapper
    {
        public EquipmentDto[] items;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject(nameof(WorldServerStage1Screen));
        DontDestroyOnLoad(go);
        go.AddComponent<WorldServerStage1Screen>();
    }

    private void Start()
    {
        CreateUi();
        StartCoroutine(LoadStage1Data());
    }

    private void CreateUi()
    {
        var canvasObject = new GameObject("Stage1Canvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var textObject = new GameObject("Stage1Status");
        textObject.transform.SetParent(canvasObject.transform, false);

        var rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.08f, 0.58f);
        rect.anchorMax = new Vector2(0.92f, 0.94f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        statusText = textObject.AddComponent<TextMeshProUGUI>();
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.fontSize = 38;
        statusText.textWrappingMode = TextWrappingModes.Normal;
        statusText.text = "Loading Character...";
    }

    private IEnumerator LoadStage1Data()
    {
        yield return FetchCharacters();
    }

    private IEnumerator FetchCharacters()
    {
        using var request = UnityWebRequest.Get(
            $"{ServerBaseUrl}/api/characters");

        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowError($"Character API gagal: {request.error}");
            yield break;
        }

        var wrappedJson = $"{{\"items\":{request.downloadHandler.text}}}";
        var response = JsonUtility.FromJson<CharacterListWrapper>(wrappedJson);

        if (response?.items == null || response.items.Length == 0)
        {
            ShowError("Belum ada character.");
            yield break;
        }

        var character = response.items[0];

        yield return FetchEquipment(character);

        Debug.Log(
            $"[WorldServer] Stage1 Character OK: {character.name} / {character.faction} / Lv.{character.level}");
    }

    private IEnumerator FetchEquipment(CharacterDto character)
    {
        using var request = UnityWebRequest.Get(
            $"{ServerBaseUrl}/api/characters/{character.id}/equipment");

        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            ShowError($"Equipment API gagal: {request.error}");
            yield break;
        }

        var wrappedJson = $"{{\"items\":{request.downloadHandler.text}}}";
        var response = JsonUtility.FromJson<EquipmentListWrapper>(wrappedJson);

        var builder = new StringBuilder();

        builder.AppendLine("STAGE 1");
        builder.AppendLine();
        builder.AppendLine(character.name);
        builder.AppendLine($"Faction: {character.faction}");
        builder.AppendLine($"Level: {character.level}");
        builder.AppendLine($"Dharma: {character.dharmaPoints}");
        builder.AppendLine();
        builder.AppendLine("EQUIPMENT");

        if (response?.items == null || response.items.Length == 0)
        {
            builder.Append("Belum ada equipment.");
        }
        else
        {
            foreach (var item in response.items)
            {
                builder.AppendLine(
                    $"{item.slot}: {item.itemCode} +{item.refinementLevel}");
            }
        }

        statusText.text = builder.ToString();
    }

    private void ShowError(string message)
    {
        statusText.text = message;
        Debug.LogError($"[WorldServer] Stage1 FAILED: {message}");
    }
}
