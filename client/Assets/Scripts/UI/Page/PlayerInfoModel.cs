using UnityEngine;
using System;

public class PlayerInfoModel
{ 
    public float Speed { get; private set; }     // 차량 속도
    
    public event Action<float> OnSpeedChanged;

    public void SetSpeed(float speed)
    {
        Speed = speed;
        OnSpeedChanged?.Invoke(Speed);
    }
    
    public int Combo { get; private set; }
    public event Action<int> OnComboChanged;

    public void SetCombo(int combo)
    {
        Combo = combo;
        OnComboChanged?.Invoke(Combo);
    }
    
    public float Fuel { get; private set; }
    public event Action<float> OnFuelChanged;

    public void SetFuel(float amount)
    {
        Fuel = amount;
        OnFuelChanged?.Invoke(Fuel);
    }

    public bool IsNitro { get; private set; }
    public event Action<bool> OnIsNitro;

    public void SetNitro(bool isNitro)
    {
        IsNitro = isNitro;
        OnIsNitro?.Invoke(IsNitro);
    }
    
    public event Action OnNirtoBoost;

    public void ExcuteNitroBoost()
    {
        OnNirtoBoost?.Invoke();
    }
}
