using UnityEngine;

namespace Core
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {

        private static T _instance;
        public static T instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(T)) as T;
                return _instance;
            }
            set => _instance = value;
        }
        
        private void Awake()
        {
            if (_instance == null) {
                _instance = this as T;
            } else if (_instance != this) {
                Debug.LogError ("Another instance of " + GetType () + " is already exist! Destroying self...");
                DestroyImmediate (this);
                return;
            }
            _instance.Init ();
        }
        public virtual void Init(){}
        
        private void OnApplicationQuit()
        {
            _instance = null;
        }
    }
}