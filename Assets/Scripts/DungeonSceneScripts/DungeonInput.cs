using UnityEngine;

public class DungeonInput : MonoBehaviour
{
    [SerializeField] private DungeonManager dungeonManager;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           // if (dungeonManager != null && dungeonManager.CanExitToLobby())
            {
             //   dungeonManager.ExitToLobby();
            }
        }
    }
}