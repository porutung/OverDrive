using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 사용 예시.
/// GameObject 또는 Component 타입을 얻어옴.
/// 단일 변수, List, Array 지원.
///
/// [Bind("게임오브젝트 이름")] Component 변수명;
/// [Bind("게임오브젝트 이름", Bind.BindType.Once)] List<GameObject> 변수명; // Finds object "게임오브젝트 이름", gets its direct children as GameObjects
/// [Bind("게임오브젝트 이름", Bind.BindType.Once)] ComponentType[] 변수명; // Finds object "게임오브젝트 이름", gets ComponentType[] using GetComponentsInChildren
/// [Bind("게임오브젝트 이름", Bind.BindType.Multi)] List<ComponentType> 변수명 = new List<ComponentType>();
//  [Bind("게임오브젝트 이름", Bind.BindType.MultiContains)] List<GameObject> 변수명 = new List<GameObject>();
//  Bind.BindType.Once => GameObject 이름과 동일한 단일 오브젝트에서 타입 검색. List/Array는 해당 오브젝트의 자식에서 검색.
//  Bind.BindType.Multi => GameObject 이름과 정확히 일치하는 모든 오브젝트에서 타입 검색 (List<> 필요).
//  Bind.BindType.MultiContains => GameObject 이름 포함하는 모든 오브젝트에서 타입 검색 (List<> 필요).
/// </summary>

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class InnerBind : Attribute
{
    /// <summary>
    /// 검색할 GameObject의 이름
    /// </summary>
    public readonly string ObjectName;
    
    /// <summary>
    /// 부모 GameObject의 이름 (선택사항)
    /// </summary>
    public readonly string ParentName;
    
    /// <summary>
    /// Inner 클래스 객체를 생성하고 해당 클래스 내에서 Bind 속성을 찾을 GameObject의 이름입니다.
    /// </summary>
    /// <param name="objectName">검색할 GameObject의 이름</param>
    /// <param name="parentName">부모 GameObject의 이름 (선택사항)</param>
    public InnerBind(string objectName, string parentName = null)
    {
        ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
        ParentName = parentName;
    }
}

[AttributeUsage(AttributeTargets.Field, Inherited = false)] // 필드에만 적용 가능하도록 제한
public sealed class Bind : Attribute // ===> 이름 변경: BindProperty -> Bind
{
    public enum BindType
    {
        Once,          // 이름이 정확히 일치하는 첫번째 오브젝트를 찾음.
        Multi,         // 이름이 정확히 일치하는 모든 오브젝트를 찾음.
        MultiContains, // 이름에 해당 문자열이 포함된 모든 오브젝트를 찾음.
    }

    // --- Inner Cache Class ---
    // 내부 클래스 이름은 그대로 두거나 필요시 변경 (예: BindingInfo)
    // 여기서는 BindInfo 그대로 사용
    private sealed class BindInfo
    {
        public readonly FieldInfo Field;
        public readonly Bind Attribute;
        public readonly Type FieldElementType; // For Lists/Arrays, the T in List<T> or T[]
        public readonly bool IsList;
        public readonly bool IsArray;
        public readonly bool IsGameObject; // Is the FieldType or ElementType a GameObject?

        public object InnerObject; // Inner class 인스턴스
        public FieldInfo ParentField; // Inner class를 소유한 필드
        public string InnerObjectName; // Inner object의 GameObject 이름
        public string InnerParentName; // Inner object의 부모 GameObject 이름

        
        public BindInfo(FieldInfo field, Bind attribute) // ===> 타입 변경: BindProperty -> Bind
        {
            Field = field;
            Attribute = attribute;
            IsList = field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>);
            IsArray = field.FieldType.IsArray;

            if (IsList)
            {
                FieldElementType = field.FieldType.GetGenericArguments()[0];
            }
            else if (IsArray)
            {
                FieldElementType = field.FieldType.GetElementType();
            }
            else
            {
                FieldElementType = field.FieldType; // For single elements
            }

            IsGameObject = FieldElementType == typeof(GameObject);
        }
    }

    // --- Static Cache for Reflection Data ---
    // Cache BindInfo per MonoBehaviour Type to avoid repeated reflection
    // 캐시 변수 이름은 그대로 두거나 필요시 변경 (예: s_typeBindingInfoCache)
    private static readonly Dictionary<Type, List<BindInfo>> s_typeBindInfoCache = new Dictionary<Type, List<BindInfo>>();

    // --- Attribute Properties ---
    public readonly string ObjectName;
    public readonly string ParentName;
    public readonly BindType BindRule; // Enum 타입 접근은 Bind.BindType 으로 변경됨

    // --- Constructors ---
    // ===> 생성자 이름 변경: BindProperty -> Bind
    public Bind(string objectName, BindType bindType = BindType.Once, string parentName = null)
    {
        ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
        BindRule = bindType;
        ParentName = parentName;
    }

    // --- Core Binding Logic ---
    // ===> 메서드 이름 변경: BindProperty.DoUpdate -> Bind.DoUpdate
    public static void DoUpdate(MonoBehaviour target)
    {
        if (target == null)
        {
            // ===> 에러 메시지 변경: BindProperty -> Bind
            Debug.LogError("Bind.DoUpdate: Target MonoBehaviour is null.");
            return;
        }

        Type targetType = target.GetType();
        List<BindInfo> bindInfos;

        // 1. Get BindInfo (from cache or generate) - Reduced Reflection Cost
        lock (s_typeBindInfoCache) // Basic locking for safety, though less critical in Unity's main thread
        {
            if (!s_typeBindInfoCache.TryGetValue(targetType, out bindInfos))
            {
                bindInfos = new List<BindInfo>();
                FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                foreach (FieldInfo field in fields)
                {
                    Bind attr = field.GetCustomAttribute<Bind>(false);
                    if (attr != null)
                    {
                        // Validate Field Type compatibility with BindType
                        bool isCollection = field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>) || field.FieldType.IsArray;
                        if ((attr.BindRule == BindType.Multi || attr.BindRule == BindType.MultiContains) && !isCollection)
                        {
                            Debug.LogError($"Bind Error: Field '{field.Name}' in '{targetType.Name}' uses BindType '{attr.BindRule}' but is not a List<> or Array. Binding skipped.", target);
                            continue; // Skip this field
                        }
                        if (attr.BindRule == BindType.Once && isCollection)
                        {
                            // Allow List/Array for Once, but warn if it's not GameObject or Component based?
                            Type elementType = isCollection ? (field.FieldType.IsArray ? field.FieldType.GetElementType() : field.FieldType.GetGenericArguments()[0]) : field.FieldType;
                            if (elementType == null || (!typeof(Component).IsAssignableFrom(elementType) && elementType != typeof(GameObject)))
                            {
                                Debug.LogWarning($"Bind Warning: Field '{field.Name}' in '{targetType.Name}' uses BindType.Once with a collection of non-Component/GameObject type '{elementType.Name}'. Ensure '{attr.ObjectName}' exists.", target);
                                // We'll still try, but it relies on GetComponentsInChildren working
                            }
                        }

                        bindInfos.Add(new BindInfo(field, attr));
                    }
                    
                    InnerBind innerBindAttr = field.GetCustomAttribute<InnerBind>(false);
                    if (innerBindAttr != null && !typeof(MonoBehaviour).IsAssignableFrom(field.FieldType))
                    {
                        // 필드 값이 null이 아닌지 확인
                        object innerObject = field.GetValue(target);
                        if (innerObject == null)
                        {
                            innerObject = Activator.CreateInstance(field.FieldType);
                            field.SetValue(target, innerObject);
                        }
                        
                        // 내부 클래스의 타입
                        Type innerType = innerObject.GetType();
                            
                        // 내부 클래스의 모든 필드 검사
                        FieldInfo[] innerFields = innerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            
                        foreach (FieldInfo innerField in innerFields)
                        {
                            Bind innerBindAttrField = innerField.GetCustomAttribute<Bind>(false);
                            if (innerBindAttrField != null)
                            {
                                // Inner class의 필드를 위한 바인딩 정보 추가
                                bindInfos.Add(new BindInfo(innerField, innerBindAttrField) {
                                    InnerObject = innerObject,
                                    ParentField = field,
                                    InnerObjectName = innerBindAttr.ObjectName,
                                    InnerParentName = innerBindAttr.ParentName
                                });
                            }
                        }
                    }

                }
                s_typeBindInfoCache[targetType] = bindInfos;
            }
            else
            {
                Dictionary<string, object> innerObjectMap = new Dictionary<string, object>();
            
                // 현재 타겟에서 모든 InnerBind 필드 값 수집
                FieldInfo[] allFields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                foreach (FieldInfo field in allFields)
                {
                    InnerBind innerBindAttr = field.GetCustomAttribute<InnerBind>(false);
                    if (innerBindAttr != null && !typeof(MonoBehaviour).IsAssignableFrom(field.FieldType))
                    {
                        // 필드 값이 null이 아닌지 확인
                        object innerObject = field.GetValue(target);
                        if (innerObject == null)
                        {
                            // 객체가 null이면 생성
                            innerObject = Activator.CreateInstance(field.FieldType);
                            field.SetValue(target, innerObject);
                        }
                    
                        // 내부 필드 이름 + 객체 이름으로 맵에 저장
                        string key = $"{field.Name}_{innerBindAttr.ObjectName}";
                        innerObjectMap[key] = innerObject;
                    }
                }
                
                foreach (BindInfo info in bindInfos)
                {
                    if (info.InnerObject != null && info.ParentField != null)
                    {
                        string key = $"{info.ParentField.Name}_{info.InnerObjectName}";
                        if (innerObjectMap.TryGetValue(key, out object newInnerObject))
                        {
                            // 새 객체 참조로 업데이트
                            info.InnerObject = newInnerObject;
                        }
                    }
                }
            }
        }

        if (bindInfos.Count == 0)
        {
            // No fields with [Bind] found for this type
            return;
        }

        // 2. Get All Transforms Once - Reduced Hierarchy Traversal Cost
        List<Transform> allTransforms = new List<Transform>();
        target.GetComponentsInChildren<Transform>(true, allTransforms); // Include inactive
        allTransforms.Remove(target.transform); // Remove self

        // 3. Process Bindings
        foreach (BindInfo info in bindInfos)
        {
            try // Add try-catch around each field binding
            {
                if (info.InnerObject != null && !string.IsNullOrEmpty(info.InnerObjectName))
                {
                    // Inner object의 상위 객체 찾기
                    Transform innerTransform = FindTransform(target.transform, allTransforms, info.InnerObjectName, info.InnerParentName);
                    
                    if (innerTransform != null)
                    {
                        // Inner class의 필드에 대한 바인딩 처리
                        switch (info.Attribute.BindRule)
                        {
                            case BindType.Once:
                                BindOnceInner(target, info, innerTransform);
                                break;
                            case BindType.Multi:
                                BindMultiInner(target, info, innerTransform, false);
                                break;
                            case BindType.MultiContains:
                                BindMultiInner(target, info, innerTransform, true);
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Bind Error: Could not find GameObject named '{info.InnerObjectName}' for inner class field in '{target.name}'.", target);
                    }
                }
                else
                {
                    // Enum 접근 시 클래스 이름(Bind)을 명시해야 할 수도 있음 (Bind.BindType.Once)
                    // 하지만 현재 컨텍스트에서는 BindType 직접 접근 가능
                    switch (info.Attribute.BindRule)
                    {
                        case BindType.Once:
                            BindOnce(target, info, allTransforms);
                            break;
                        case BindType.Multi:
                            BindMulti(target, info, allTransforms, false);
                            break;
                        case BindType.MultiContains:
                            BindMulti(target, info, allTransforms, true);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Bind Error: Failed to bind field '{info.Field.Name}' in '{target.name}' ({targetType.Name}). Rule: {info.Attribute.BindRule}, ObjectName: '{info.Attribute.ObjectName}'.\nException: {ex}", target);
            }
        }
    }
    
    private static Transform FindTransform(Transform target, List<Transform> allTransforms, string objectName, string parentName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        // 부모 지정된 경우 부모 하위에서 검색
        if (!string.IsNullOrEmpty(parentName))
        {
            Transform parentTransform = target.transform.Find(parentName);
            if (parentTransform != null)
            {
                return parentTransform.Find(objectName);
            }
            return null;
        }
        
        // 일반 검색
        foreach (Transform t in allTransforms)
        {
            if (t.name.Equals(objectName, StringComparison.Ordinal))
            {
                return t;
            }
        }
        
        // 부모가 없는 경우 직접 검색
        return target.transform.Find(objectName);
    }

    private static void BindOnceInner(MonoBehaviour target, BindInfo info, Transform innerTransform)
    {
        Transform foundTransform = null;
        
        // 인아웃 클래스 필드의 GameObject 이름으로 찾기
        if (!string.IsNullOrEmpty(info.Attribute.ParentName))
        {
            Transform parentTransform = innerTransform.Find(info.Attribute.ParentName);
            if (parentTransform == null)
            {
                Debug.LogError($"Bind Error (Inner Once): Could not find Parent GameObject named '{info.Attribute.ParentName}' for inner field '{info.Field.Name}' in '{target.name}'.", target);
                return;
            }

            // Parent 하위에서 ObjectName 찾기
            foreach (Transform child in parentTransform)
            {
                if (child.name.Equals(info.Attribute.ObjectName, StringComparison.Ordinal))
                {
                    foundTransform = child;
                    break;
                }
            }
        }
        else
        {
            List<Transform> allTransforms = new List<Transform>();
            innerTransform.GetComponentsInChildren<Transform>(true, allTransforms); // Include inactive
            allTransforms.Remove(innerTransform); // Remove self
            
            foundTransform = FindTransform(innerTransform, allTransforms, info.Attribute.ObjectName, null);
        }

        if (foundTransform == null)
        {
            Debug.LogError($"Bind Error (Inner Once): Could not find GameObject named '{info.Attribute.ObjectName}' for inner field '{info.Field.Name}' in '{target.name}'.", target);
            return;
        }

        // 필드 타입에 따라 할당 처리
        if (info.IsList) // List<T>
        {
            Type listType = typeof(List<>).MakeGenericType(info.FieldElementType);
            System.Collections.IList list = Activator.CreateInstance(listType) as System.Collections.IList;

            if (info.IsGameObject) // List<GameObject> - Get direct children GameObjects
            {
                foreach (Transform child in foundTransform)
                {
                    list.Add(child.gameObject);
                }
            }
            else if (typeof(Component).IsAssignableFrom(info.FieldElementType)) // List<ComponentType> - Get components in children
            {
                Component[] components = foundTransform.GetComponentsInChildren(info.FieldElementType, true);
                foreach (Component comp in components)
                {
                    list.Add(comp);
                }
            }
            else
            {
                Debug.LogWarning($"Bind Warning (Inner Once): List field '{info.Field.Name}' requests unsupported element type '{info.FieldElementType.Name}'. Only GameObject and Component types supported for Lists.", target);
            }
            
            info.Field.SetValue(info.InnerObject, list);
        }
        else if (info.IsArray) // T[]
        {
            if (info.IsGameObject) // GameObject[] - Get direct children GameObjects
            {
                GameObject[] children = new GameObject[foundTransform.childCount];
                int i = 0;
                foreach (Transform child in foundTransform)
                {
                    children[i++] = child.gameObject;
                }
                
                info.Field.SetValue(info.InnerObject, children);
            }
            else if (typeof(Component).IsAssignableFrom(info.FieldElementType)) // ComponentType[] - Get components in children
            {
                Component[] components = foundTransform.GetComponentsInChildren(info.FieldElementType, true);
                Array typedArray = Array.CreateInstance(info.FieldElementType, components.Length);
                Array.Copy(components, typedArray, components.Length);
                
                info.Field.SetValue(info.InnerObject, typedArray);
            }
            else
            {
                Debug.LogWarning($"Bind Warning (Inner Once): Array field '{info.Field.Name}' requests unsupported element type '{info.FieldElementType.Name}'. Only GameObject and Component types supported for Arrays.", target);
                Array emptyArray = Array.CreateInstance(info.FieldElementType, 0);
                info.Field.SetValue(info.InnerObject, emptyArray);
            }
        }
        else // Single Field (GameObject or Component)
        {
            if (info.IsGameObject) // GameObject
            {
                info.Field.SetValue(info.InnerObject, foundTransform.gameObject);
            }
            else if (typeof(Component).IsAssignableFrom(info.FieldElementType)) // Component
            {
                Component component = foundTransform.GetComponent(info.FieldElementType);
                if (component == null)
                {
                    Debug.LogError($"Bind Error (Inner Once): Component of type '{info.FieldElementType.Name}' not found on GameObject '{info.Attribute.ObjectName}' for inner field '{info.Field.Name}' in '{target.name}'.", target);
                }
                else
                {
                    info.Field.SetValue(info.InnerObject, component);
                }
            }
            else
            {
                Debug.LogError($"Bind Error (Inner Once): Field '{info.Field.Name}' is not a GameObject, Component, List, or Array type. Cannot bind.", target);
            }
        }
    }

    private static void BindMultiInner(MonoBehaviour target, BindInfo info, Transform innerTransform, bool useContains)
    {
        System.Collections.IList list = info.Field.GetValue(info.InnerObject) as System.Collections.IList;
        
        if (list == null)
        {
            Type listType = typeof(List<>).MakeGenericType(info.FieldElementType);
            list = Activator.CreateInstance(listType) as System.Collections.IList;
            info.Field.SetValue(info.InnerObject, list);
        }
        else
        {
            list.Clear();
        }

        List<Transform> searchScope = new List<Transform>();
        innerTransform.GetComponentsInChildren<Transform>(true, searchScope);

        // ParentName 지정 시, 해당 부모 하위만 대상으로
        if (!string.IsNullOrEmpty(info.Attribute.ParentName))
        {
            Transform parentTransform = innerTransform.Find(info.Attribute.ParentName);
            if (parentTransform == null)
            {
                Debug.LogError($"Bind Error (Inner Multi): Could not find Parent GameObject named '{info.Attribute.ParentName}' for inner field '{info.Field.Name}' in '{target.name}'.", target);
                return;
            }

            // parentTransform 포함 안 하고 하위만
            searchScope.Clear();
            parentTransform.GetComponentsInChildren<Transform>(true, searchScope);
            searchScope.Remove(parentTransform);
        }

        foreach (Transform t in searchScope)
        {
            bool nameMatch = useContains
                ? t.name.Contains(info.Attribute.ObjectName)
                : t.name.Equals(info.Attribute.ObjectName, StringComparison.Ordinal);

            if (nameMatch)
            {
                if (info.IsGameObject)
                {
                    list.Add(t.gameObject);
                }
                else if (typeof(Component).IsAssignableFrom(info.FieldElementType))
                {
                    Component component = t.GetComponent(info.FieldElementType);
                    if (component != null)
                    {
                        list.Add(component);
                    }
                }
            }
        }

        if (info.IsArray)
        {
            Array array = Array.CreateInstance(info.FieldElementType, list.Count);
            list.CopyTo(array, 0);
            info.Field.SetValue(info.InnerObject, array);
        }
    }


    // --- Binding Implementations ---
    // 내부 헬퍼 메서드 이름은 그대로 사용
    private static void BindOnce(MonoBehaviour target, BindInfo info, List<Transform> allTransforms)
    {
        Transform foundTransform = null;
        // Find the first transform matching the name
        if (!string.IsNullOrEmpty(info.Attribute.ParentName))
        {
            // Parent Transform 찾기
            //Transform parentTransform = allTransforms.FirstOrDefault(t => t.name.Equals(info.Attribute.ParentName, StringComparison.Ordinal));
            Transform parentTransform = target.transform.Find(info.Attribute.ParentName);
            if (parentTransform == null)
            {
                Debug.LogError($"Bind Error (Once): Could not find Parent GameObject named '{info.Attribute.ParentName}' for field '{info.Field.Name}' in '{target.name}'.", target);
                return;
            }

            // Parent 하위에서 ObjectName 찾기
            foundTransform = parentTransform.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t != parentTransform && t.name.Equals(info.Attribute.ObjectName, StringComparison.Ordinal));
        }
        else
        {
            foreach (Transform t in allTransforms)
            {
                if (t.name.Equals(info.Attribute.ObjectName, StringComparison.Ordinal))
                {
                    foundTransform = t;
                    break;
                }
            }
        }

        if (foundTransform == null)
        {
            Debug.LogError($"Bind Error (Once): Could not find GameObject named '{info.Attribute.ObjectName}' for field '{info.Field.Name}' in '{target.name}'.", target);
            return;
        }

        // --- Assign based on Field Type ---
        if (info.IsList) // List<T>
        {
            Type listType = typeof(List<>).MakeGenericType(info.FieldElementType);
            System.Collections.IList list = Activator.CreateInstance(listType) as System.Collections.IList;

            if (info.IsGameObject) // List<GameObject> - Get direct children GameObjects
            {
                foreach (Transform child in foundTransform) // Iterate direct children
                {
                    list.Add(child.gameObject);
                }
            }
            else if (typeof(Component).IsAssignableFrom(info.FieldElementType)) // List<ComponentType> - Get components in children
            {
                Component[] components = foundTransform.GetComponentsInChildren(info.FieldElementType, true);
                foreach (Component comp in components)
                {
                    list.Add(comp);
                }
            }
            else
            {
                Debug.LogWarning($"Bind Warning (Once): List field '{info.Field.Name}' requests unsupported element type '{info.FieldElementType.Name}'. Only GameObject and Component types supported for Lists.", target);
            }
            info.Field.SetValue(target, list);
        }
        else if (info.IsArray) // T[]
        {
            if (info.IsGameObject) // GameObject[] - Get direct children GameObjects
            {
                GameObject[] children = new GameObject[foundTransform.childCount];
                int i = 0;
                foreach (Transform child in foundTransform)
                {
                    children[i++] = child.gameObject;
                }
                info.Field.SetValue(target, children);
            }
            else if (typeof(Component).IsAssignableFrom(info.FieldElementType)) // ComponentType[] - Get components in children
            {
                Component[] components = foundTransform.GetComponentsInChildren(info.FieldElementType, true);
                Array typedArray = Array.CreateInstance(info.FieldElementType, components.Length);
                Array.Copy(components, typedArray, components.Length);
                info.Field.SetValue(target, typedArray);
            }
            else
            {
                Debug.LogWarning($"Bind Warning (Once): Array field '{info.Field.Name}' requests unsupported element type '{info.FieldElementType.Name}'. Only GameObject and Component types supported for Arrays.", target);
                info.Field.SetValue(target, Array.CreateInstance(info.FieldElementType, 0)); // Set empty array
            }
        }
        else // Single Field (GameObject or Component)
        {
            if (info.IsGameObject) // GameObject
            {
                info.Field.SetValue(target, foundTransform.gameObject);
            }
            else if (typeof(Component).IsAssignableFrom(info.FieldElementType)) // Component
            {
                Component component = foundTransform.GetComponent(info.FieldElementType);
                if (component == null)
                {
                    Debug.LogError($"Bind Error (Once): Component of type '{info.FieldElementType.Name}' not found on GameObject '{info.Attribute.ObjectName}' for field '{info.Field.Name}' in '{target.name}'.", target);
                }
                else
                {
                    info.Field.SetValue(target, component);
                }
            }
            else
            {
                Debug.LogError($"Bind Error (Once): Field '{info.Field.Name}' is not a GameObject, Component, List, or Array type. Cannot bind.", target);
            }
        }
    }

    private static void BindMulti(MonoBehaviour target, BindInfo info, List<Transform> allTransforms, bool useContains)
    {
        System.Collections.IList list = info.Field.GetValue(target) as System.Collections.IList;
        if (list == null)
        {
            Type listType = typeof(List<>).MakeGenericType(info.FieldElementType);
            list = Activator.CreateInstance(listType) as System.Collections.IList;
            info.Field.SetValue(target, list);
        }
        else
        {
            list.Clear();
        }

        IEnumerable<Transform> searchScope = allTransforms;

        // ParentName 지정 시, 해당 부모 하위만 대상으로
        if (!string.IsNullOrEmpty(info.Attribute.ParentName))
        {
            //Transform parentTransform = allTransforms.FirstOrDefault(t => t.name.Equals(info.Attribute.ParentName, StringComparison.Ordinal));
            Transform parentTransform = target.transform.Find(info.Attribute.ParentName);
            if (parentTransform == null)
            {
                Debug.LogError($"Bind Error (Multi): Could not find Parent GameObject named '{info.Attribute.ParentName}' for field '{info.Field.Name}' in '{target.name}'.", target);
                return;
            }

            // parentTransform 포함 안 하고 하위만
            searchScope = parentTransform.GetComponentsInChildren<Transform>(true).Where(t => t != parentTransform);
        }

        foreach (Transform t in searchScope)
        {
            bool nameMatch = useContains
                ? t.name.Contains(info.Attribute.ObjectName)
                : t.name.Equals(info.Attribute.ObjectName, StringComparison.Ordinal);

            if (nameMatch)
            {
                if (info.IsGameObject)
                {
                    list.Add(t.gameObject);
                }
                else if (typeof(Component).IsAssignableFrom(info.FieldElementType))
                {
                    Component component = t.GetComponent(info.FieldElementType);
                    if (component != null)
                    {
                        list.Add(component);
                    }
                    // Optional: Warn if component is missing
                    // else { Debug.LogWarning($"Bind (Multi): GameObject '{t.name}' matched but lacks component '{info.FieldElementType.Name}'.", target); }
                }
            }
        }

        if (info.IsArray)
        {
            Array array = Array.CreateInstance(info.FieldElementType, list.Count);
            list.CopyTo(array, 0);
            if (info.InnerObject != null)
            {
                info.Field.SetValue(info.InnerObject, array);
            }
            else
            {
                info.Field.SetValue(target, array);
            }
        }
    }
}