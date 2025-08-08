using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // 플레이어 차 컨트롤러를 인스펙터에서 연결해줍니다.
    [SerializeField] private PlayerCarController playerCarController;

    private InputSystem_Actions _playerControls;
    
    // 키보드/게임패드용 좌우 이동 액션
    private InputAction _moveAction;
    private InputAction _nitroAction;
    // 터치용 액션
    private InputAction _touchPressAction;
    private InputAction _touchPositionAction;

    
    private void Awake()
    {
        _playerControls = new InputSystem_Actions();
        
        // 'Move' 액션이 수행될 때 HandleMove 메서드를 호출하도록 등록합니다.
        
        _moveAction = _playerControls.Player.Move;
        _nitroAction = _playerControls.Player.Nitro;
        
        _touchPressAction = _playerControls.Player.TouchPress;
        _touchPositionAction = _playerControls.Player.TouchPosition;
    }

    private void OnEnable()
    {
        _playerControls.Player.Enable();
        _touchPressAction.Enable();
        _touchPositionAction.Enable();
        
        _playerControls.Player.Move.performed += HandleMove;
        _playerControls.Player.Nitro.performed += HandleNitro;
        // TouchPress 액션이 시작될 때(started) OnTouchInput 메서드를 호출하도록 '구독'
        _touchPressAction.started += OnTouchInput;
    }

    private void OnDisable()
    {
        _playerControls.Player.Move.performed -= HandleMove;
        _playerControls.Player.Nitro.performed -= HandleNitro;
        // 스크립트가 비활성화될 때 구독을 '해제'하여 메모리 누수 방지
        _touchPressAction.started -= OnTouchInput;

        _moveAction.Disable();
        _touchPressAction.Disable();
        _touchPositionAction.Disable();        
    }

    // 입력이 감지되면 실행되는 메서드입니다.
    private void HandleMove(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        
        // 커맨드를 생성하고 즉시 실행합니다.
        ICommand moveCommand = new MoveCommand(playerCarController, moveInput);
        moveCommand.Execute();
    }

    private void HandleNitro(InputAction.CallbackContext context)
    {
        playerCarController.ActivateNitroBoost();
        Debug.Log("키보드 입력: 나이트로!");
    }
    // --- 터치 입력 처리 (이벤트 기반 방식) ---
    // TouchPress 액션이 시작될 때 Input System에 의해 자동으로 호출되는 메서드
    private void OnTouchInput(InputAction.CallbackContext context)
    {
        // 중요: UI 위를 터치했는지 확인
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // TouchPosition 액션에서 현재 터치된 좌표를 읽어옵니다.
        Vector2 touchPosition = _touchPositionAction.ReadValue<Vector2>();

        ICommand touchCommand = new TouchMoveCommand(playerCarController, touchPosition);
        touchCommand.Execute();
    }

    private void Update()
    {
        //Vector2 moveInput = _moveAction.ReadValue<Vector2>();        
    }
}