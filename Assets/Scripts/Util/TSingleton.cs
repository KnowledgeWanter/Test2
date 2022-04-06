// ===============================================
// 描 述：
// 作 者：
// 创建时间：
// 备 注：
// ================================================

using UnityEngine;

public class TSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    T[] _instances = (T[])FindObjectsOfType(typeof(T));

                    if (_instances.Length > 1)
                    {
                        Debug.LogError("too mush");
                    }

                    if (_instances != null && _instances.Length > 0)
                    {
                        _instance = _instances[0];
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = typeof(T).ToString() + "__Singleton";
                        if (Application.isPlaying)
                            DontDestroyOnLoad(singleton);
                    }

                    _instance.gameObject.SendMessage("OnCreateInstance", SendMessageOptions.DontRequireReceiver);
                }

                return _instance;
            }
        }
    }

    public static bool HasInstance()
    {
        if (_instance == null)
        {
            T[] _instances = (T[])FindObjectsOfType(typeof(T));
            return _instances.Length >= 1;
        }
        return true;
    }
}