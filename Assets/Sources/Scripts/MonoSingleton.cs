//参考: http://wiki.unity3d.com/index.php?title=Singleton
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance = null;

    public static T Instance
    {
        get
        {
            //Scene内にあったら取得
            instance = instance ?? (FindObjectOfType(typeof(T)) as T);
            //TをアタッチしたGameObject生成してT取得
            instance = instance ?? new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
            return instance;
        }
    }

    private void OnApplicationQuit()
    {
        instance = null;
    }
}