using System.Collections.Generic;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 관 바닥의 트리거 영역. 자신의 Collider2D(Is Trigger) 안에 들어온 AppraisalTankIcon을
    /// 집합으로 유지해 "바닥에 안정적으로 도착한(grounded)" 아이콘을 이벤트 기반으로 추적한다.
    /// </summary>
    public sealed class AppraisalFloorSensor : MonoBehaviour
    {
        private readonly HashSet<AppraisalTankIcon> _groundedIcons = new();

        public bool Contains(AppraisalTankIcon icon)
        {
            return icon != null && _groundedIcons.Contains(icon);
        }

        /// <summary>
        /// 풀에서 재사용되는 아이콘을 스폰 지점으로 되돌릴 때, 트리거 Exit 콜백 타이밍에
        /// 의존하지 않고 즉시 grounded 상태를 해제하기 위한 방어용 메서드.
        /// </summary>
        public void Release(AppraisalTankIcon icon)
        {
            _groundedIcons.Remove(icon);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            AppraisalTankIcon icon = other.GetComponent<AppraisalTankIcon>();
            if (icon != null)
            {
                _groundedIcons.Add(icon);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            AppraisalTankIcon icon = other.GetComponent<AppraisalTankIcon>();
            if (icon != null)
            {
                _groundedIcons.Remove(icon);
            }
        }
    }
}
