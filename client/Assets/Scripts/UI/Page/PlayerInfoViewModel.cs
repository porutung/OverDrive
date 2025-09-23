using System;
using UnityEngine;

[UIPrefab("Prefab/UI/PlayerInfoPage")]
public class PlayerInfoViewModel : ViewModel_Base
{
    // 페이지에 필요한 데이터가 있다면 여기에 작성
    private PlayerInfoModel _model;

    public string speedText { get; private set; }
    public event Action<string> OnChangeSpeedText;
    
    public string comboText { get; private set; }
    public event Action<string> OnChangeComboText;
    
    public float fuelAmount { get; private set; }
    public event Action<float> OnChangeFuelText;

    public bool isNitro { get; private set; }
    public event Action<bool> OnActiveNitro;
    public PlayerInfoViewModel(PlayerInfoModel model)
    {
        _model = model;
        _model.OnSpeedChanged += UpdateSpeedView;
        _model.OnComboChanged += UpdateComboView;
        _model.OnFuelChanged += UpdateFuelView;
        _model.OnIsNitro += UpdateActiveNitro;
    }
    protected override void OnDispose()
    {
        _model.OnSpeedChanged -= UpdateSpeedView;
        _model.OnComboChanged -= UpdateComboView;
        _model.OnFuelChanged -= UpdateFuelView;
        _model.OnIsNitro -= UpdateActiveNitro;
    }

    private void UpdateSpeedView(float speed)
    {
        speedText = string.Format($"{speed:F0}km/h");
        
        OnChangeSpeedText?.Invoke(speedText);
    }

    private void UpdateComboView(int combo)
    {
        comboText = string.Format($"Combo : {combo}");
        
        OnChangeComboText?.Invoke(comboText);
    }

    private void UpdateFuelView(float amount)
    {
        fuelAmount =  amount;
        OnChangeFuelText?.Invoke(fuelAmount);
    }

    private void UpdateActiveNitro(bool active)
    {
        isNitro = active;
        OnActiveNitro?.Invoke(active);
    }
    public void OnClickNitroBoost()
    {
        _model.ExcuteNitroBoost();
    }
    
}
