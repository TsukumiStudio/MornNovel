using System;
using UnityEngine;

namespace MornLib
{
    [Serializable]
    public struct MornNovelAddress
    {
        [SerializeField] private string _address;
        public string Key => _address;

        public MornNovelAddress(string address)
        {
            _address = address;
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(_address);
        }
    }
}