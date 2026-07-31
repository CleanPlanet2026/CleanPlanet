using System;
using UnityEngine;

namespace CleanPlanet.Player
{
    public sealed class TrashInventory : MonoBehaviour
    {
        [SerializeField, Min(0)] private int _trashCount;

        public event Action<int> TrashCountChanged;

        public int TrashCount => _trashCount;

        public void AddTrash(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _trashCount += amount;
            TrashCountChanged?.Invoke(_trashCount);
        }
    }
}
