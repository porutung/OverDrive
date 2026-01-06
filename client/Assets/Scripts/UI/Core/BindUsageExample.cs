using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 예시용

/// <summary>
/// [Bind] 시스템 사용법 총정리 가이드
/// </summary>
public class BindUsageExample : MonoBehaviour
{
    [Header("미리 바인딩 하려는 경우는 [SerializeField] 필수!")]
    // ========================================================================
    // 1. 기본 사용법 (단일 객체 찾기)
    // ========================================================================

    [Header("1. Basic Single Binding")]
    
    // 내 자식들 중 이름이 "TitleText"인 오브젝트의 Text 컴포넌트를 연결
    [Bind("TitleText")] 
    [SerializeField]private Text _titleText;

    // 내 자식들 중 이름이 "SubmitBtn"인 오브젝트 자체(GameObject)를 연결
    [Bind("SubmitBtn")] 
    [SerializeField]private GameObject _submitBtnObj;

    // 내 자식들 중 이름이 "HeroModel"인 오브젝트의 Transform을 연결
    [Bind("HeroModel")] 
    [SerializeField]private Transform _heroModelTr;


    // ========================================================================
    // 2. 리스트/배열 사용법 (BindType에 따른 동작 차이 주의)
    // ========================================================================

    [Header("2. Collection Binding (List & Array)")]

    // [Case A: BindType.Once (컨테이너 방식)]
    // "InventoryGrid"라는 이름의 부모를 딱 하나 찾고, **그 직계 자식들**을 모두 리스트에 담습니다.
    // 용도: 그리드, 슬롯, 탭 메뉴 등
    [Bind("InventoryGrid", Bind.BindType.Once)] 
    [SerializeField]private List<GameObject> _inventorySlots;

    // [Case B: BindType.Multi (이름 일치 수집)]
    // 내 하위의 모든 자식들 중 이름이 정확히 "EnemySpawnPoint"인 것들을 모두 찾아 배열에 담습니다.
    // 용도: 특정 이름을 가진 흩어진 객체들 수집
    [Bind("EnemySpawnPoint", Bind.BindType.Multi)] 
    [SerializeField]private Transform[] _spawnPoints;

    // [Case C: BindType.MultiContains (이름 포함 수집)]
    // 내 하위 자식들 중 이름에 "Coin"이 포함된 모든 것을 찾습니다. (예: "Coin_1", "SmallCoin", "Coin_Big")
    // 용도: 이름에 규칙성이 있는 객체들 수집
    [Bind("Coin", Bind.BindType.MultiContains)] 
    [SerializeField]private List<GameObject> _allCoins;


    // ========================================================================
    // 3. 특정 부모 지정 (Parent Constraint)
    // ========================================================================

    [Header("3. Parent Constraint")]

    // "StatusPanel"이라는 부모를 찾고, **그 안에서만** "Icon"이라는 이름을 찾습니다.
    // (다른 곳에 있는 "Icon"은 무시됨)
    [Bind("Icon", Bind.BindType.Once, "StatusPanel")] 
    [SerializeField] private Image _statusIcon;

    // "MobArea"라는 부모를 찾고, 그 하위에서 이름에 "Goblin"이 포함된 애들을 다 찾음
    [Bind("Goblin", Bind.BindType.MultiContains, "MobArea")]
    [SerializeField]private List<GameObject> _goblinsInArea;


    // ========================================================================
    // 4. InnerBind (변수 그룹화 / 구조화)
    // ========================================================================

    [Header("4. InnerBind (Grouping)")]

    // [사용법]
    // 1. 일반 클래스(Serializable)를 정의합니다.
    // 2. 그 안에 [Bind] 변수들을 넣습니다.
    // 3. MonoBehaviour에서 [InnerBind]로 선언합니다.
    
    // "TopInfoPanel"이라는 오브젝트를 찾고, 그 자식들 중에서 아래 클래스 내용을 채웁니다.
    [InnerBind("TopInfoPanel")]
    [SerializeField]public PlayerInfoGroup playerInfo; 

    // "BottomInfoPanel"이라는 오브젝트를 찾고, 그 자식들 중에서 아래 클래스 내용을 채웁니다.
    // (동일한 클래스 재사용 가능!)
    [InnerBind("BottomInfoPanel")]
    [SerializeField]public PlayerInfoGroup otherPlayerInfo;


    // ========================================================================
    // Inner Class 정의 (MonoBehaviour 상속 X)
    // ========================================================================
    [System.Serializable] // 인스펙터에서 보기 위해 붙임
    public class PlayerInfoGroup
    {
        // InnerBind로 지정된 부모("TopInfoPanel")의 자식 중에서 "Name"을 찾음
        [Bind("Name")] public Text nameText;

        // InnerBind로 지정된 부모("TopInfoPanel")의 자식 중에서 "Level"을 찾음
        [Bind("Level")] public Text levelText;

        // InnerBind 내부에서도 리스트 사용 가능
        // "TopInfoPanel" 아래에 "Stars"라는 부모가 있고, 그 자식들을 리스트로 담음
        [Bind("Stars", Bind.BindType.Once)] 
        public List<Image> starIcons;
    }


    // ========================================================================
    // 실행부
    // ========================================================================
    private void Awake()
    {
        // 이 함수를 호출해야 바인딩이 수행됩니다.
        Bind.DoUpdate(this);

        // 사용 예시
        if (_titleText != null) 
            Debug.Log($"Title: {_titleText.text}");
        
        Debug.Log($"Inventory Slots Count: {_inventorySlots?.Count}");
        Debug.Log($"Player Name: {playerInfo?.nameText?.text}");
    }
    
    //=======================================================================
    // 컴포넌트 추가 시 / Reset 버튼 클릭 시 자동 실행
    //=======================================================================
    private void Reset()
    {
        // 이 함수를 호출해야 바인딩이 수행됩니다.
        // 스크립트를 게임오브젝트에 드래그&드롭 하는 순간 실행됩니다.
        Debug.Log($"[{gameObject.name}] Auto Binding Started...");
        Bind.DoUpdate(this);
    }
}