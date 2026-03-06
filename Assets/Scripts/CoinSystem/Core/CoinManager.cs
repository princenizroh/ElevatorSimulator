using System;
using System.Collections.Generic;
using UnityEngine;
using KProject.Core.Patterns;
using KProject.Core.Interfaces;

namespace Platformer.Managers
{
    public class CoinManager : Singleton<CoinManager>, ISaveable
    {
        public static event Action<int> OnCoinsChanged;

        private int _totalCoins = 0;
        private HashSet<string> _collectedCoinIds = new HashSet<string>();

        void Start()
        {
            OnCoinsChanged?.Invoke(_totalCoins);
        }

        public void AddCoins(int amount, string coinId = null)
        {
            _totalCoins += amount;
            if (coinId != null) _collectedCoinIds.Add(coinId);
            OnCoinsChanged?.Invoke(_totalCoins);
        }

        public bool IsCollected(string coinId) => _collectedCoinIds.Contains(coinId);

        public List<string> GetCollectedIds() => new List<string>(_collectedCoinIds);

        public void SetCollectedIds(List<string> ids)
        {
            _collectedCoinIds = new HashSet<string>(ids);
        }

        public object GetSaveData() => _totalCoins;

        public void LoadSaveData(object data)
        {
            _totalCoins = (int)data;
            OnCoinsChanged?.Invoke(_totalCoins);
        }
    }
}
