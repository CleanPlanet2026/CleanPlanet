using System;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    public enum ItemGrade
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    /// <summary>
    /// 감정 대상 수집물 1개를 나타내는 자체 경량 데이터 타입.
    /// 다른 시스템(TrashItem 등)에 의존하지 않는다.
    /// </summary>
    [Serializable]
    public sealed class AppraisalItem
    {
        [SerializeField] private string _name;
        [SerializeField] private ItemGrade _grade;
        [SerializeField, Min(0)] private int _baseValue;

        public string Name => _name;
        public ItemGrade Grade => _grade;
        public int BaseValue => _baseValue;

        public AppraisalItem(string name, ItemGrade grade, int baseValue)
        {
            _name = name;
            _grade = grade;
            _baseValue = baseValue;
        }
    }
}
