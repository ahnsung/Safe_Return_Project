using System.Collections;
using UnityEngine;

// 플레이어가 "방 1개 안에서" 어떻게 움직이는지 관리하는 핵심 스크립트
// 흐름:
// StartPoint -> EventTriggerPoint -> ExitPoint -> 방향 선택 -> 다음 방
public class RoomTraversalController : MonoBehaviour
{
    // 현재 플레이어가 방 내부 진행의 어느 단계에 있는지 나타내는 상태
    private enum State
    {
        MovingToEvent, // 이벤트 지점까지 이동 중
        EventRunning,  // 이벤트 진행 중
        MovingToExit,  // 이벤트 후 출구까지 이동 중
        Waiting,       // 출구 도착 후 방향 선택 대기 중
        Transition     // Fade 하면서 다음 방으로 넘어가는 중
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f; // 플레이어 이동 속도

    [Header("Move Clamp")]
    [SerializeField] private float minX = -8f; // 플레이어가 이동할 수 있는 최소 X
    [SerializeField] private float maxX = 8f;  // 플레이어가 이동할 수 있는 최대 X

    [Header("Points")]
    [SerializeField] private Transform startPoint; // 시작 위치
    [SerializeField] private Transform eventPoint; // 이벤트 발생 위치
    [SerializeField] private Transform exitPoint;  // 출구 위치

    [Header("Managers")]
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private DungeonUIManager uiManager;
    [SerializeField] private FadeController fadeController;

    [Header("Event")]
    [SerializeField] private float eventDuration = 1f; // 현재는 더미 이벤트 지속 시간

    private State state;

    // 현재 방에서 이벤트가 이미 발생했는지
    private bool eventDone;

    // 현재 방에서 출구에 이미 도착했는지
    private bool exitDone;

    // 방 전환 중 중복 입력 막기용
    private bool isTransitioning;

    private void Start()
    {
        // 방 진입 시 초기 흐름 시작
        StartFlow();
    }

    private void Update()
    {
        // 이동 가능한 상태일 때만 좌우 이동 처리
        if (state == State.MovingToEvent || state == State.MovingToExit)
            Move();

        // 이벤트로 가는 중일 때만 이벤트 지점 체크
        if (state == State.MovingToEvent)
            CheckEvent();

        // 출구로 가는 중일 때만 출구 체크
        if (state == State.MovingToExit)
            CheckExit();
    }

    // 현재 방의 흐름을 처음부터 시작하는 함수
    private void StartFlow()
    {
        // 플레이어를 시작 위치로 보냄
        transform.position = startPoint.position;

        // 이벤트/출구 상태 초기화
        eventDone = false;
        exitDone = false;

        // 방향 선택 패널 숨김
        if (uiManager != null)
            uiManager.HideDirectionPanel();

        // 첫 상태는 이벤트 지점으로 이동
        state = State.MovingToEvent;
    }

    // A / D 키로 플레이어를 좌우 이동시키는 함수
    private void Move()
    {
        float h = 0f;

        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;

        Vector3 pos = transform.position;
        pos.x += h * moveSpeed * Time.deltaTime;

        // 방 내부 이동 범위 제한
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;
    }

    // 이벤트 지점에 닿았는지 체크
    private void CheckEvent()
    {
        if (eventDone) return;

        if (Vector3.Distance(transform.position, eventPoint.position) < 0.2f)
        {
            eventDone = true;
            StartCoroutine(EventRoutine());
        }
    }

    // 출구 지점에 닿았는지 체크
    private void CheckExit()
    {
        if (exitDone) return;

        if (Vector3.Distance(transform.position, exitPoint.position) < 0.2f)
        {
            exitDone = true;
            state = State.Waiting;

            // 출구 도착 시 방향 버튼 갱신 + 미니맵 갱신
            dungeonManager.RefreshAll();

            // 방향 선택 패널 표시
            if (uiManager != null)
                uiManager.ShowDirectionPanel();
        }
    }

    // 현재는 더미 이벤트 코루틴
    // 나중에 여기서 몬스터/상인/함정 등 분기할 예정
    private IEnumerator EventRoutine()
    {
        state = State.EventRunning;

        yield return new WaitForSeconds(eventDuration);

        // 이벤트 끝나면 출구 방향으로 다시 이동 가능
        state = State.MovingToExit;
    }

    // UI 버튼에서 방향을 누르면 호출됨
    public void SelectNextRoom(MoveDirection dir)
    {
        // 출구 도착 후 대기 상태일 때만 이동 가능
        if (state != State.Waiting) return;

        // 전환 중이면 중복 실행 금지
        if (isTransitioning) return;

        StartCoroutine(ChangeRoom(dir));
    }

    // 다음 방으로 실제 전환하는 코루틴
    private IEnumerator ChangeRoom(MoveDirection dir)
    {
        isTransitioning = true;
        state = State.Transition;

        // 방향 선택 UI 숨김
        if (uiManager != null)
            uiManager.HideDirectionPanel();

        // 화면 어둡게
        if (fadeController != null)
            yield return fadeController.FadeOut();

        // 논리 좌표 이동
        dungeonManager.MoveToNextRoom(dir);

        // 플레이어를 다시 StartPoint 위치로 보냄
        transform.position = startPoint.position;

        // 현재 방 상태 초기화
        eventDone = false;
        exitDone = false;

        // 화면 다시 보이게
        if (fadeController != null)
            yield return fadeController.FadeIn();

        // 다시 이벤트 지점으로 가는 상태로 시작
        state = State.MovingToEvent;
        isTransitioning = false;
    }
}