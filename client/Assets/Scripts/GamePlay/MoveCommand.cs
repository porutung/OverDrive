using UnityEngine;

public class MoveCommand : ICommand
{
    private PlayerCarController _playerCar;
    private Vector2 _direction;

    // 생성자: 어느 차를 어느 방향으로 움직일지 정보를 받습니다.
    public MoveCommand(PlayerCarController playerCar, Vector2 direction)
    {
        _playerCar = playerCar;
        _direction = direction;
    }

    // 실행: 플레이어 차의 이동 메서드를 호출합니다.
    public void Execute()
    {
        // 좌우 이동만 처리합니다.
        if (_direction.x < 0)
        {
            _playerCar.MoveLeft();
        }
        else if (_direction.x > 0)
        {
            _playerCar.MoveRight();
        }
       
    }
}

public class TouchMoveCommand : ICommand
{
    private PlayerCarController _playerCar;
    private Vector2 _direction;

    public TouchMoveCommand(PlayerCarController playerCar, Vector2 direction)
    {
        _playerCar = playerCar;
        _direction = direction;
    }

    public void Execute()
    {
        // 터치된 위치에 따라 좌/우 이동 실행
        if (_direction.x < Screen.width / 2)
        {
            // 커맨드를 생성하고 즉시 실행합니다.
            _playerCar.MoveLeft();
            Debug.Log("터치 입력: 왼쪽");
        }
        else
        {
            // 커맨드를 생성하고 즉시 실행합니다.            
            Debug.Log("터치 입력: 오른쪽");
            _playerCar.MoveRight();
        }        
    }
}