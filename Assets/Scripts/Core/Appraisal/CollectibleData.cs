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
    /// 감정 대상 수집물 1종의 데이터 정의. 이름/등급/기본가치/아이콘은 수집 시점에 이미
    /// 정해져 있으며, 감정로봇은 이 아이콘을 그대로 표시할 뿐 스스로 고르지 않는다.
    /// </summary>
    [CreateAssetMenu(fileName = "CollectibleData", menuName = "CleanPlanet/Appraisal/Collectible Data")]
    public sealed class CollectibleData : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private ItemGrade _grade;
        [SerializeField, Min(0)] private int _baseValue;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Sprite _screenIcon;

        public string Name => _name;
        public ItemGrade Grade => _grade;
        public int BaseValue => _baseValue;
        public Sprite Icon => _icon;

        /// <summary>
        /// 감정로봇 디스플레이(릴)에서 쓸 화면용 아이콘. 검은 화면에 어울리게 주황으로
        /// 물들인 버전이며, 비어 있으면 일반 <see cref="Icon"/>으로 폴백한다. 유리관 등
        /// 다른 곳은 <see cref="Icon"/>을 그대로 쓰므로 여기 값을 넣어도 영향받지 않는다.
        /// </summary>
        public Sprite ScreenIcon => _screenIcon != null ? _screenIcon : _icon;
    }
}
