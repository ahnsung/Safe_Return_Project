using System.Collections.Generic;
using UnityEngine;

// 던전 전체 맵의 논리 좌표, 방문 방, 미니맵 갱신,
// 그리고 "던전 내 턴 흐름"까지 같이 관리하는 스크립트
public class DungeonManager : MonoBehaviour
{
    // 다른 스크립트(전투, 인벤토리, 이벤트 등)에서
    // 쉽게 현재 던전 매니저에 접근할 수 있게 싱글톤처럼 사용
    public static DungeonManager Instance;

    [Header("Map Size")]
    [SerializeField] private int mapWidth = 5;
    [SerializeField] private int mapHeight = 5;

    [Header("Start")]
    [SerializeField] private Vector2Int startRoom = new Vector2Int(2, 2);

    [Header("Refs")]
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private MinimapUIManager minimapUI;

    // 현재 플레이어가 있는 논리 방 좌표
    private Vector2Int currentRoom;

    // 방문한 방 목록
    private HashSet<string> visited = new HashSet<string>();

    // 저장 키
    private const string XKEY = "ROOM_X";
    private const string YKEY = "ROOM_Y";
    private const string VISITED = "VISITED";

    // 턴 저장 키
    private const string TURN_KEY = "DUNGEON_TURN";

    // 현재 던전 턴
    private int currentTurn = 0;

    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public Vector2Int CurrentRoom => currentRoom;
    public int CurrentTurn => currentTurn;

    private void Awake()
    {
        // 싱글톤 등록
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 저장된 좌표 / 방문 / 턴 불러오기
        Load();

        // 현재 방은 무조건 방문 처리
        MarkVisited(currentRoom);

        // 다시 저장
        Save();
    }

    private void Start()
    {
        RefreshAll();

        Debug.Log($"[Dungeon] 현재 턴: {currentTurn}");
    }

    // =========================================================
    // 턴 시스템
    // =========================================================

    // 던전 내에서 "행동 1번" 했을 때 호출하는 함수
    // 예:
    // AddTurn("맵 이동");
    // AddTurn("인벤토리에서 무기 사용");
    // AddTurn("몬스터와 전투");
    public void AddTurn(string reason)
    {
        currentTurn++;
        SaveTurn();

        Debug.Log($"[Dungeon Turn] 현재 턴: {currentTurn} / 행동: {reason}");
    }

    // 턴 강제 초기화
    public void ResetTurn()
    {
        currentTurn = 0;
        SaveTurn();

        Debug.Log("[Dungeon Turn] 턴이 0으로 초기화되었습니다.");
    }

    // 턴만 따로 저장
    private void SaveTurn()
    {
        PlayerPrefs.SetInt(TURN_KEY, currentTurn);
        PlayerPrefs.Save();
    }

    // =========================================================
    // 방 이동 / 맵 시스템
    // =========================================================

    // 방향 버튼 눌렀을 때 실제로 다음 방으로 이동
    public void MoveToNextRoom(MoveDirection dir)
    {
        Vector2Int next = currentRoom;

        if (dir == MoveDirection.Up) next += Vector2Int.up;
        if (dir == MoveDirection.Down) next += Vector2Int.down;
        if (dir == MoveDirection.Left) next += Vector2Int.left;
        if (dir == MoveDirection.Right) next += Vector2Int.right;

        // 맵 바깥이면 무시
        if (!IsInside(next)) return;

        // 좌표 갱신
        currentRoom = next;

        // 방문 처리
        MarkVisited(currentRoom);

        // "방 이동"은 턴 1 증가
        AddTurn("맵 이동");

        // 저장
        Save();

        // UI 갱신
        RefreshAll();
    }

    public bool IsVisited(int x, int y)
    {
        return visited.Contains(x + "," + y);
    }

    public Dictionary<MoveDirection, bool> GetDirections()
    {
        return new Dictionary<MoveDirection, bool>()
        {
            { MoveDirection.Up, currentRoom.y < mapHeight - 1 },
            { MoveDirection.Down, currentRoom.y > 0 },
            { MoveDirection.Left, currentRoom.x > 0 },
            { MoveDirection.Right, currentRoom.x < mapWidth - 1 }
        };
    }

    public void RefreshAll()
    {
        if (uiManager != null)
            uiManager.RefreshDirectionButtons(GetDirections());

        if (minimapUI != null)
            minimapUI.RefreshMinimap();
    }

    private void MarkVisited(Vector2Int r)
    {
        visited.Add(r.x + "," + r.y);
    }

    private bool IsInside(Vector2Int r)
    {
        return r.x >= 0 && r.x < mapWidth && r.y >= 0 && r.y < mapHeight;
    }

    // 좌표 / 방문 / 턴 저장
    private void Save()
    {
        PlayerPrefs.SetInt(XKEY, currentRoom.x);
        PlayerPrefs.SetInt(YKEY, currentRoom.y);

        string merged = string.Join("|", visited);
        PlayerPrefs.SetString(VISITED, merged);

        PlayerPrefs.SetInt(TURN_KEY, currentTurn);

        PlayerPrefs.Save();
    }

    // 좌표 / 방문 / 턴 불러오기
    private void Load()
    {
        if (PlayerPrefs.HasKey(XKEY))
        {
            currentRoom = new Vector2Int(
                PlayerPrefs.GetInt(XKEY),
                PlayerPrefs.GetInt(YKEY)
            );
        }
        else
        {
            currentRoom = startRoom;
        }

        visited.Clear();

        if (PlayerPrefs.HasKey(VISITED))
        {
            string data = PlayerPrefs.GetString(VISITED);
            string[] arr = data.Split('|');

            foreach (var s in arr)
            {
                if (!string.IsNullOrEmpty(s))
                    visited.Add(s);
            }
        }

        // 턴 불러오기
        currentTurn = PlayerPrefs.GetInt(TURN_KEY, 0);
    }
}