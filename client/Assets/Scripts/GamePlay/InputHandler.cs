using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    // 플레이어 차 컨트롤러를 인스펙터에서 연결해줍니다.
    [SerializeField] private PlayerCarController playerCarController;

    private InputSystem_Actions _playerControls;

    private void Awake()
    {
        _playerControls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _playerControls.Player.Enable();
        // 'Move' 액션이 수행될 때 HandleMove 메서드를 호출하도록 등록합니다.
        _playerControls.Player.Move.performed += HandleMove;
    }

    private void OnDisable()
    {
        _playerControls.Player.Move.performed -= HandleMove;
        _playerControls.Player.Disable();
    }

    // 입력이 감지되면 실행되는 메서드입니다.
    private void HandleMove(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        
        // 커맨드를 생성하고 즉시 실행합니다.
        ICommand moveCommand = new MoveCommand(playerCarController, moveInput);
        moveCommand.Execute();
    }
}