using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    // 새 저장 데이터 생성
    public void CreateNewSave(int characterID, string playerName)
    {
        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.SetInt("CutscenePlayed", 1);
        PlayerPrefs.SetInt("SelectedCharacter", characterID);
        PlayerPrefs.SetString("PlayerName", playerName);

        PlayerPrefs.Save();
    }

    // 저장 데이터 존재 여부
    public bool HasSave()
    {
        return PlayerPrefs.GetInt("HasSave", 0) == 1;
    }

    // 컷씬을 봤는지
    public bool HasPlayedCutscene()
    {
        return PlayerPrefs.GetInt("CutscenePlayed", 0) == 1;
    }

    // 선택 캐릭터 가져오기
    public int GetSelectedCharacter()
    {
        return PlayerPrefs.GetInt("SelectedCharacter", -1);
    }

    // 이름 가져오기
    public string GetPlayerName()
    {
        return PlayerPrefs.GetString("PlayerName", "");
    }

    // 저장 데이터 삭제
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey("HasSave");
        PlayerPrefs.DeleteKey("CutscenePlayed");
        PlayerPrefs.DeleteKey("SelectedCharacter");
        PlayerPrefs.DeleteKey("PlayerName");

        PlayerPrefs.Save();
    }
}