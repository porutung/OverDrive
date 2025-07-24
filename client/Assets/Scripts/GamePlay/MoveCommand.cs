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
