using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// UI가 생성될 Canvas들을 관리합니다.
/// </summary>
public class CanvasManager : MonoBehaviour
{
    // Canvas 타입을 정의하는 Enum
    public enum ECanvasType
    {
        Overlay,    // 일반적인 UI (팝업 등)
        Camera,     // 특정 카메라에 종속되는 UI
        World,      // 3D 월드 공간에 존재하는 UI (캐릭터 머리 위 HP 바 등)
    }

    // 인스펙터에서 Canvas를 타입과 함께 등록하기 위한 클래스
    [Serializable]
    public class CanvasMapping
    {
        public ECanvasType Type;
        public Canvas Canvas;
    }

    // 인스펙터에 노출될 Canvas 매핑 리스트
    [SerializeField] private List<CanvasMapping> _canvasMappings;

    // 빠른 조회를 위한 딕셔너리 (캐싱)
    private Dictionary<ECanvasType, Transform> _canvasTransforms;

    private void Awake()
    {
        // 성능을 위해 리스트를 딕셔너리로 변환하여 캐싱합니다.
        _canvasTransforms = _canvasMappings.ToDictionary(mapping => mapping.Type, mapping => mapping.Canvas.transform);
    }

    /// <summary>
    /// 지정된 타입의 Canvas Transform을 반환합니다. UI를 이 Transform의 자식으로 생성하면 됩니다.
    /// </summary>
    /// <param name="type">가져올 Canvas의 타입</param>
    /// <returns>해당 타입의 Canvas Transform</returns>
    public Transform GetCanvas(ECanvasType type)
    {
        if (_canvasTransforms.TryGetValue(type, out var canvasTransform))
        {
            return canvasTransform;
        }

        Debug.LogError($"[CanvasManager] Canvas of type {type} not found!");
        return null;
    }
}