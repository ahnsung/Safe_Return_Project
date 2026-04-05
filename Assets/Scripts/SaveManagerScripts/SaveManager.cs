using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string HAS_SAVE_KEY = "HasSave";
    private const string CUTSCENE_PLAYED_KEY = "CutscenePlayed";
    private const string SELECTED_CHARACTER_KEY = "SelectedCharacter";
    private const string PLAYER_NAME_KEY = "PlayerName";

    private const string ROOM_X_KEY = "ROOM_X";
    private const string ROOM_Y_KEY = "ROOM_Y";
    private const string VISITED_KEY = "VISITED";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreateNewSave(int characterID, string playerName)
    {
        PlayerPrefs.SetInt(HAS_SAVE_KEY, 1);
        PlayerPrefs.SetInt(CUTSCENE_PLAYED_KEY, 1);
        PlayerPrefs.SetInt(SELECTED_CHARACTER_KEY, characterID);
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        PlayerPrefs.Save();
    }

    public bool HasSave()
    {
        return PlayerPrefs.GetInt(HAS_SAVE_KEY, 0) == 1;
    }

    public bool HasPlayedCutscene()
    {
        return PlayerPrefs.GetInt(CUTSCENE_PLAYED_KEY, 0) == 1;
    }

    public int GetSelectedCharacter()
    {
        return PlayerPrefs.GetInt(SELECTED_CHARACTER_KEY, -1);
    }

    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(HAS_SAVE_KEY);
        PlayerPrefs.DeleteKey(CUTSCENE_PLAYED_KEY);
        PlayerPrefs.DeleteKey(SELECTED_CHARACTER_KEY);
        PlayerPrefs.DeleteKey(PLAYER_NAME_KEY);

        // 던전/미니맵 데이터 삭제
        PlayerPrefs.DeleteKey(ROOM_X_KEY);
        PlayerPrefs.DeleteKey(ROOM_Y_KEY);
        PlayerPrefs.DeleteKey(VISITED_KEY);

        // 새 게임용 임시 플래그도 정리
        PlayerPrefs.DeleteKey("AfterCutsceneGoToCharacterSelect");

        PlayerPrefs.Save();

        Debug.Log("세이브 + 던전 + 미니맵 데이터 삭제 완료");
    }
}