using UnityEngine;

namespace Utils
{
    public static class GameObjectExtension
    {
        public static T TryGetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component))
                component = gameObject.AddComponent<T>();

            return component;
        }

        public static T TryGetOrAddComponent<T>(this Component component)
            where T : Component
        {
            return component.gameObject.TryGetOrAddComponent<T>();
        }
    }
}