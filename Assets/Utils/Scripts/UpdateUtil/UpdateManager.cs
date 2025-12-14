using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class UpdateManager : MonoBehaviour
    {
        private static UpdateManager _instance;

        private static readonly List<IUpdateObserver> _updates = new();
        private static readonly List<IFixedUpdateObserver> _fixedUpdates = new();
        private static readonly List<ILateUpdateObserver> _lateUpdates = new();

        private static int _updateIndex;
        private static int _fixedIndex;
        private static int _lateIndex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (_instance != null) return;
            var obj = new GameObject("[UpdateManager]");
            _instance = obj.AddComponent<UpdateManager>();
            DontDestroyOnLoad(obj);
        }

        private void Update()
        {
            for (_updateIndex = _updates.Count - 1; _updateIndex >= 0; _updateIndex--)
            {
                _updates[_updateIndex].OnUpdate();
            }
        }

        private void FixedUpdate()
        {
            for (_fixedIndex = _fixedUpdates.Count - 1; _fixedIndex >= 0; _fixedIndex--)
            {
                _fixedUpdates[_fixedIndex].OnFixedUpdate();
            }
        }

        private void LateUpdate()
        {
            for (_lateIndex = _lateUpdates.Count - 1; _lateIndex >= 0; _lateIndex--)
            {
                _lateUpdates[_lateIndex].OnLateUpdate();
            }
        }

        // -------- Register / DeRegister --------

        public static void Register(object observer)
        {
            if (observer is IUpdateObserver u)
                _updates.Add(u);

            if (observer is IFixedUpdateObserver f)
                _fixedUpdates.Add(f);

            if (observer is ILateUpdateObserver l)
                _lateUpdates.Add(l);
        }

        public static void DeRegister(object observer)
        {
            if (observer is IUpdateObserver u)
            {
                _updates.Remove(u);
                _updateIndex--;
            }

            if (observer is IFixedUpdateObserver f)
            {
                _fixedUpdates.Remove(f);
                _fixedIndex--;
            }

            if (observer is ILateUpdateObserver l)
            {
                _lateUpdates.Remove(l);
                _lateIndex--;
            }
        }
    }
}
