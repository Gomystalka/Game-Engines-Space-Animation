using UnityEngine;

public static class Utilities
{
    public static float Clamp01(this float value) => value < 0 ? 0 : value > 1 ? 1 : value;
    //public static Vector3 TryGetPosition(this Transform transform) => transform ? transform.position : Vector3.zero;
    //public static Transform TryGetTransformAt(this Transform[] transforms, int index) => index < transforms.Length ? transforms[index] : null;
    public static Vector3 TryGetPositionAt(Transform[] t, int index) { //Making this an extension crashes Unity
        if (t == null) return Vector3.zero;
        return index < t.Length ? t[index] ? t[index].position : Vector3.zero : Vector3.zero;
    }

    public static bool verboseMode = true;
    public static bool IsMouseOverUI => UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

    public static T CreateSingleton<T>(T obj, T value)
    {
        if (obj == null)
        {
            Print($"Assigned new instance to Singleton.", LogLevel.Info);
            return value;
        }
        else
        {
            Print($"Singleton instance already exists ({obj}). '{value}' will be destroyed.", LogLevel.Warning);
            Object.Destroy(value as Component);
        }
        return obj;
    }

    public static void Print(object log, LogLevel level)
    {
        if (!verboseMode) return;
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
        System.Diagnostics.StackFrame[] stackFrames = trace.GetFrames();
        string caller = stackFrames[1].GetMethod().Name;
        for (int i = 0; i < stackFrames.Length; i++)
        {
            System.Type reflectedType = stackFrames[i].GetMethod().ReflectedType;
            if (builder.ToString().IndexOf(reflectedType.FullName) == -1 && reflectedType.FullName.Length < 32)
                builder.Append($"{reflectedType.FullName}{(i == stackFrames.Length - 1 ? "" : ">>")}");
        }
        if (builder.ToString().EndsWith(">>"))
            builder = builder.Remove(builder.Length - 2, 2);

        switch (level)
        {
            case LogLevel.Warning:
                Debug.LogWarning($"[{builder}:{caller}] -> {log}");
                break;
            case LogLevel.Assert:
                Debug.LogAssertion($"[{builder}:{caller}] -> {log}");
                break;
            case LogLevel.Error:
                Debug.LogError($"[{builder}:{caller}] -> {log}");
                break;
            default:
                Debug.Log($"[{builder}:{caller}] -> {log}");
                break;
        }
    }
    public static Transform FindChildInChildrenByName(this Transform transform, string name, ChildSearchMode searchMode = ChildSearchMode.Equals)
    {
        name = name.ToLower();
        foreach (Transform t in transform.GetComponentsInChildren<Transform>())
        {
            string n = t.name.ToLower();
            if (!t) continue;
            switch (searchMode)
            {
                case ChildSearchMode.Contains:
                    if (n.Contains(name))
                        return t;
                    break;
                case ChildSearchMode.StartsWith:
                    if (n.StartsWith(name))
                        return t;
                    break;
                default:
                    if (n.ToLower() == name)
                        return t;
                    break;
            }
        }
        return null;
    }
}
public enum LogLevel : byte
{
    Info,
    Warning,
    Assert,
    Error
}

public enum ChildSearchMode : byte
{
    Equals,
    Contains,
    StartsWith
}