using System.Collections.Generic;
using UnityEngine;

// 던전 전체 맵의 논리 좌표를 관리하는 스크립트
// 역할:
// 1) 현재 방 좌표 저장
// 2) 방문한 방 목록 저장
// 3) 이동 가능한 방향 계산
// 4) 미니맵 / 방향 버튼 UI 갱신
public class DungeonManager : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private int mapWidth = 5;   // 가로 칸 수
    [SerializeField] private int mapHeight = 5;  // 세로 칸 수

    [Header("Start")]
    [SerializeField] private Vector2Int startRoom = new Vector2Int(2, 2); // 첫 시작 좌표

    [Header("Refs")]
    [SerializeField] private DungeonUIManager uiManager; // 방향 버튼 갱신용
    [SerializeField] private MinimapUIManager minimapUI; // 미니맵 갱신용

    // 현재 플레이어가 있는 논리 방 좌표
    private Vector2Int currentRoom;

    // 방문한 방들 저장
    // "2,3" 같은 문자열 형태로 저장
    private HashSet<string> visited = new HashSet<string>();

    // PlayerPrefs 키 이름
    private const string XKEY = "ROOM_X";
    private const string YKEY = "ROOM_Y";
    private const string VISITED = "VISITED";

    // 다른 스크립트에서 현재 맵 크기 참조할 수 있게 프로퍼티 제공
    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;
    public Vector2Int CurrentRoom => currentRoom;

    private void Awake()
    {
        // 저장된 좌표/방문 정보 불러오기
        Load();

        // 현재 방은 무조건 방문 처리
        MarkVisited(currentRoom);

        // 다시 저장
        Save();
    }

    private void Start()
    {
        // 시작 시 UI 전체 갱신
        RefreshAll();
    }

    // 방향 버튼을 눌렀을 때 실제로 다음 방 좌표로 바꾸는 함수
    public void MoveToNextRoom(MoveDirection dir)
    {
        Vector2Int next = currentRoom;

        // 눌린 방향에 따라 좌표 변경
        if (dir == MoveDirection.Up) next += Vector2Int.up;
        if (dir == MoveDirection.Down) next += Vector2Int.down;
        if (dir == MoveDirection.Left) next += Vector2Int.left;
        if (dir == MoveDirection.Right) next += Vector2Int.right;

        // 맵 바깥이면 이동 무시
        if (!IsInside(next)) return;

        // 현재 방 갱신
        currentRoom = next;

        // 방문 처리
        MarkVisited(currentRoom);

        // 저장
        Save();

        // 미니맵 / 방향 버튼 갱신
        RefreshAll();
    }

    // 특정 좌표를 이미 방문했는지 확인
    public bool IsVisited(int x, int y)
    {
        return visited.Contains(x + "," + y);
    }

    // 현재 좌표 기준으로 상하좌우 이동 가능 여부 계산
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

    // UI 전체 갱신
    public void RefreshAll()
    {
        if (uiManager != null)
            uiManager.RefreshDirectionButtons(GetDirections());

        if (minimapUI != null)
            minimapUI.RefreshMinimap();
    }

    // 방문한 방 목록에 현재 좌표 추가
    private void MarkVisited(Vector2Int r)
    {
        visited.Add(r.x + "," + r.y);
    }

    // 맵 안쪽인지 확인
    private bool IsInside(Vector2Int r)
    {
        return r.x >= 0 && r.x < mapWidth && r.y >= 0 && r.y < mapHeight;
    }

    // 현재 좌표와 방문 목록 저장
    private void Save()
    {
        PlayerPrefs.SetInt(XKEY, currentRoom.x);
        PlayerPrefs.SetInt(YKEY, currentRoom.y);

        // HashSet 내용을 | 로 이어붙인 문자열로 저장
        string merged = string.Join("|", visited);
        PlayerPrefs.SetString(VISITED, merged);

        PlayerPrefs.Save();
    }

    // 저장 데이터 불러오기
    private void Load()
    {
        // 좌표 저장이 있으면 불러오고, 없으면 기본 시작 좌표 사용
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

        // 방문 목록 초기화
        visited.Clear();

        // 방문 목록 저장이 있으면 다시 복원
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
    }
}