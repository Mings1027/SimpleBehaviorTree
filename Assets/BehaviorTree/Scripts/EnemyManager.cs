using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class EnemyManager : MonoBehaviour
    {
        public static List<Enemy> allEnemies;
        private static EnemyManager instance;

        private void Awake()
        {
            instance = this;
            allEnemies = new List<Enemy>();
        }

        public static void AddEnemy(Enemy enemy)
        {
            allEnemies.Add(enemy);
        }

        public static void RemoveEnemy(Enemy enemy)
        {
            allEnemies.Remove(enemy);
        }
    }
}