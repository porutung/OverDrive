using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// ==========================================
// 1. Attributes
// ==========================================

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class InnerBind : Attribute
{
    public readonly string ObjectName;
    public readonly string ParentName;

    public InnerBind(string objectName, string parentName = null)
    {
        ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
        ParentName = parentName;
    }
}

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class Bind : Attribute
{
    public enum BindType
    {
        Once,           // 이름 일치 단일 객체 (리스트인 경우 해당 객체의 자식들)
        Multi,          // 이름 일치 모든 객체
        MultiContains,  // 이름 포함 모든 객체
    }

    // [누락되었던 부분 복구] ================================
    public readonly string ObjectName;
    public readonly string ParentName;
    public readonly BindType BindRule;

    public Bind(string objectName, BindType bindType = BindType.Once, string parentName = null)
    {
        ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
        BindRule = bindType;
        ParentName = parentName;
    }
    // =======================================================

    // ==========================================
    // 2. Internal Cache Structure
    // ==========================================
    private sealed class BindInfo
    {
        public readonly FieldInfo Field;
        public readonly Bind Attribute;
        public readonly Type FieldElementType;
        public readonly bool IsList;
        public readonly bool IsArray;
        public readonly bool IsGameObject;

        // InnerBind 관련 정보
        public FieldInfo ParentField;   // Inner Class를 소유한 메인 필드
        public string InnerObjectName;  // Inner Class가 기준 잡을 객체 이름
        public string InnerParentName;  // Inner Class 기준 객체의 부모 이름

        // 런타임 인스턴스
        public object InnerInstance;    

        public BindInfo(FieldInfo field, Bind attribute)
        {
            Field = field;
            Attribute = attribute;
            IsList = field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>);
            IsArray = field.FieldType.IsArray;

            if (IsList) FieldElementType = field.FieldType.GetGenericArguments()[0];
            else if (IsArray) FieldElementType = field.FieldType.GetElementType();
            else FieldElementType = field.FieldType;

            IsGameObject = FieldElementType == typeof(GameObject);
        }
    }

    // ==========================================
    // 3. Static Caches (Optimization)
    // ==========================================
    private static readonly Dictionary<Type, List<BindInfo>> s_typeBindInfoCache = new Dictionary<Type, List<BindInfo>>();
    
    // [Optimized] GC 발생 방지를 위한 재사용 리스트
    private static readonly List<Transform> s_sharedTransforms = new List<Transform>(256);

    // ==========================================
    // 4. Public API (Main Entry)
    // ==========================================
    public static void DoUpdate(MonoBehaviour target)
    {
        if (target == null) return;

        // 1. 바인딩 정보 가져오기 (캐싱 처리됨)
        List<BindInfo> bindInfos = GetCachedBindInfos(target);
        if (bindInfos == null || bindInfos.Count == 0) return;

        // 2. 계층 구조 수집 (GC 최적화: 정적 리스트 재사용)
        s_sharedTransforms.Clear();
        target.GetComponentsInChildren<Transform>(true, s_sharedTransforms);

        try
        {
            // 3. 각 필드 바인딩 수행
            foreach (BindInfo info in bindInfos)
            {
                ProcessBinding(target, info);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Bind] 치명적 오류 발생: {ex.Message}", target);
        }
        finally
        {
            // 4. 정리 (메모리 누수 방지)
            s_sharedTransforms.Clear();
        }
    }

    // ==========================================
    // 5. Core Logic - Binding Process
    // ==========================================

    private static void ProcessBinding(MonoBehaviour target, BindInfo info)
    {
        // A. Inner Class 바인딩인 경우
        if (info.ParentField != null)
        {
            ResolveInnerBinding(target, info);
        }
        // B. 일반 필드 바인딩인 경우
        else
        {
            ResolveStandardBinding(target, info);
        }
    }

    private static void ResolveInnerBinding(MonoBehaviour target, BindInfo info)
    {
        // Inner Instance가 유효한지 확인
        if (info.InnerInstance == null) return;

        // Inner Class의 검색 기준이 될 Root 찾기
        Transform innerRoot = FindTransform(target.transform, s_sharedTransforms, info.InnerObjectName, info.InnerParentName);

        if (innerRoot != null)
        {
            ApplyBindRule(target, info, innerRoot, info.InnerInstance);
        }
        else
        {
            Debug.LogError($"[Bind] InnerObject '{info.InnerObjectName}'를 찾을 수 없습니다. (Parent: {info.InnerParentName ?? "None"})", target);
        }
    }

    private static void ResolveStandardBinding(MonoBehaviour target, BindInfo info)
    {
        // 일반 바인딩은 본인(target.transform) 하위에서 검색하고, 본인 인스턴스에 할당
        ApplyBindRule(target, info, target.transform, target);
    }

    private static void ApplyBindRule(MonoBehaviour context, BindInfo info, Transform searchRoot, object instanceToSet)
    {
        // 이제 BindRule에 정상적으로 접근 가능합니다.
        switch (info.Attribute.BindRule)
        {
            case BindType.Once:
                BindOnce(context, info, searchRoot, instanceToSet);
                break;
            case BindType.Multi:
                BindMulti(context, info, searchRoot, instanceToSet, false);
                break;
            case BindType.MultiContains:
                BindMulti(context, info, searchRoot, instanceToSet, true);
                break;
        }
    }

    // ==========================================
    // 6. Implementation Details (Once / Multi)
    // ==========================================

    private static void BindOnce(MonoBehaviour context, BindInfo info, Transform root, object instance)
    {
        // 이제 ObjectName, ParentName에 정상적으로 접근 가능합니다.
        Transform foundTransform = FindTransform(root, s_sharedTransforms, info.Attribute.ObjectName, info.Attribute.ParentName);

        if (foundTransform == null)
        {
            Debug.LogError($"[Bind] '{info.Attribute.ObjectName}'를 찾을 수 없습니다.", context);
            return;
        }

        // 값 할당 (Assign)
        if (info.IsList)
        {
            var list = CreateList(info.FieldElementType);
            CollectChildren(foundTransform, list, info.FieldElementType, info.IsGameObject);
            info.Field.SetValue(instance, list);
        }
        else if (info.IsArray)
        {
            var list = CreateList(info.FieldElementType);
            CollectChildren(foundTransform, list, info.FieldElementType, info.IsGameObject);
            info.Field.SetValue(instance, ListToArray(list, info.FieldElementType));
        }
        else
        {
            // 단일 할당
            object value = GetValueFromTransform(foundTransform, info.FieldElementType, info.IsGameObject);
            if (value != null) info.Field.SetValue(instance, value);
            else Debug.LogError($"[Bind] '{foundTransform.name}'에 컴포넌트 '{info.FieldElementType.Name}'가 없습니다.", context);
        }
    }

    private static void BindMulti(MonoBehaviour context, BindInfo info, Transform root, object instance, bool useContains)
    {
        List<Transform> candidates = s_sharedTransforms;

        // ParentName이 있다면 그 부모가 존재하는지 먼저 확인 (단순화를 위해 여기서는 유효성 체크만)
        if (!string.IsNullOrEmpty(info.Attribute.ParentName))
        {
             Transform scopeParent = FindTransform(root, s_sharedTransforms, info.Attribute.ParentName, null);
             if(scopeParent == null) 
             {
                 Debug.LogError($"[Bind] Parent '{info.Attribute.ParentName}' not found.", context);
                 return;
             }
        }

        var list = CreateList(info.FieldElementType);

        // 전체 리스트 순회하며 조건 검사
        int count = candidates.Count;
        for (int i = 0; i < count; i++)
        {
            Transform t = candidates[i];
            if (t == root) continue; // 루트 제외

            // 이름 조건 검사
            bool nameMatch = useContains
                ? t.name.Contains(info.Attribute.ObjectName)
                : t.name.Equals(info.Attribute.ObjectName, StringComparison.Ordinal);

            if (!nameMatch) continue;

            // 부모 조건 검사 (ParentName이 있을 경우)
            if (!string.IsNullOrEmpty(info.Attribute.ParentName))
            {
                if (t.parent == null || !t.parent.name.Equals(info.Attribute.ParentName, StringComparison.Ordinal))
                    continue;
            }

            // 조건 만족 시 리스트에 추가
            object val = GetValueFromTransform(t, info.FieldElementType, info.IsGameObject);
            if (val != null) list.Add(val);
        }

        // 결과 할당
        if (info.IsArray)
        {
            info.Field.SetValue(instance, ListToArray(list, info.FieldElementType));
        }
        else
        {
            info.Field.SetValue(instance, list);
        }
    }

    // ==========================================
    // 7. Helper Methods (Search & Instantiation)
    // ==========================================

    private static Transform FindTransform(Transform root, List<Transform> allTransforms, string name, string parentName)
    {
        if (string.IsNullOrEmpty(name)) return null;

        // 1. Fast Path: 부모가 명시된 경우 (직계 자식 우선 검색)
        if (!string.IsNullOrEmpty(parentName))
        {
            Transform p = root.Find(parentName);
            if (p != null) return p.Find(name);
            // 직계에서 못 찾으면 아래 루프에서 찾을 수도 있지만, 
            // ParentName의 의미가 "직계 부모"라면 여기서 null 리턴이 맞음.
            // 기존 로직 호환성을 위해 여기서는 단순 처리.
        }

        // 2. 전체 검색 (리스트 순회)
        int count = allTransforms.Count;
        for (int i = 0; i < count; i++)
        {
            Transform t = allTransforms[i];
            if (t == root) continue;

            if (t.name.Equals(name, StringComparison.Ordinal))
            {
                return t;
            }
        }

        return root.Find(name);
    }

    private static List<BindInfo> GetCachedBindInfos(MonoBehaviour target)
    {
        Type type = target.GetType();

        if (s_typeBindInfoCache.TryGetValue(type, out List<BindInfo> infos))
        {
            UpdateInnerInstances(target, infos);
            return infos;
        }

        infos = new List<BindInfo>();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        foreach (var field in fields)
        {
            // 1. 일반 Bind 처리
            var bindAttr = field.GetCustomAttribute<Bind>(false);
            if (bindAttr != null)
            {
                infos.Add(new BindInfo(field, bindAttr));
            }

            // 2. InnerBind 처리
            var innerAttr = field.GetCustomAttribute<InnerBind>(false);
            if (innerAttr != null && !typeof(MonoBehaviour).IsAssignableFrom(field.FieldType))
            {
                object innerObj = field.GetValue(target);
                if (innerObj == null)
                {
                    innerObj = Activator.CreateInstance(field.FieldType);
                    field.SetValue(target, innerObj);
                }

                var innerFields = field.FieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var inField in innerFields)
                {
                    var inBindAttr = inField.GetCustomAttribute<Bind>(false);
                    if (inBindAttr != null)
                    {
                        var info = new BindInfo(inField, inBindAttr)
                        {
                            ParentField = field,
                            InnerObjectName = innerAttr.ObjectName,
                            InnerParentName = innerAttr.ParentName,
                            InnerInstance = innerObj
                        };
                        infos.Add(info);
                    }
                }
            }
        }

        s_typeBindInfoCache[type] = infos;
        return infos;
    }

    private static void UpdateInnerInstances(MonoBehaviour target, List<BindInfo> infos)
    {
        foreach (var info in infos)
        {
            if (info.ParentField != null)
            {
                object innerObj = info.ParentField.GetValue(target);
                if (innerObj == null)
                {
                    innerObj = Activator.CreateInstance(info.ParentField.FieldType);
                    info.ParentField.SetValue(target, innerObj);
                }
                info.InnerInstance = innerObj;
            }
        }
    }

    // ==========================================
    // 8. Utility Methods (Collection Helpers)
    // ==========================================

    private static IList CreateList(Type elementType)
    {
        Type listType = typeof(List<>).MakeGenericType(elementType);
        return (IList)Activator.CreateInstance(listType);
    }

    private static Array ListToArray(IList list, Type elementType)
    {
        Array array = Array.CreateInstance(elementType, list.Count);
        list.CopyTo(array, 0);
        return array;
    }

    private static object GetValueFromTransform(Transform t, Type type, bool isGameObject)
    {
        if (isGameObject) return t.gameObject;
        return t.GetComponent(type);
    }

    private static void CollectChildren(Transform parent, IList list, Type type, bool isGameObject)
    {
        if (isGameObject)
        {
            foreach (Transform child in parent)
            {
                list.Add(child.gameObject);
            }
        }
        else
        {
            var components = parent.GetComponentsInChildren(type, true);
            foreach (var comp in components)
            {
                list.Add(comp);
            }
        }
    }
}