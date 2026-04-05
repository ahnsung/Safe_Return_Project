using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject continueButton;
    public GameObject warningPanel;

    void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
            continueButton.SetActive(true);
        else
            continueButton.SetActive(false);

        if (warningPanel != null)
            warningPanel.SetActive(false);
    }

    // Continue 버튼
    public void ContinueGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }

    // New Game 버튼
    public void NewGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            // 저장 데이터가 있으면 경고창
            warningPanel.SetActive(true);
        }
        else
        {
            // 저장 데이터가 없으면 바로 새 게임 시작
            PlayerPrefs.SetInt("AfterCutsceneGoToCharacterSelect", 1);
            SceneManager.LoadScene("StartScene");
        }
    }

    // 경고창 Yes
    public void ConfirmNewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }

        // 컷씬 끝나면 CharacterSelectScene으로 바로 가기 위한 플래그
        PlayerPrefs.SetInt("AfterCutsceneGoToCharacterSelect", 1);

        warningPanel.SetActive(false);
        SceneManager.LoadScene("StartScene");
    }

    // 경고창 No
    public void CancelNewGame()
    {
        warningPanel.SetActive(false);
    }
}