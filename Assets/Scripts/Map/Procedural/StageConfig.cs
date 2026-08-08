using UnityEngine;

namespace CleanPlanet.Map.Procedural
{
    [CreateAssetMenu(fileName = "StageConfig", menuName = "CleanPlanet/Map/Stage Config")]
    public sealed class StageConfig : ScriptableObject
    {
        [SerializeField] private int _stageId = 1;
        [SerializeField] private string _displayName = "Stage";
        [SerializeField, TextArea] private string _shortDescription;

        [SerializeField] private MapGenerationSettings _mapSettings = new();
        [SerializeField] private TileSet _tileSet;

        [Tooltip("이 스테이지를 클리어(Earth Clean 누적치 도달)하면 다음 스테이지가 열린다. 마지막 스테이지는 의미 없음.")]
        [SerializeField] private float _cleanGoalToUnlockNext = 100f;

        [Tooltip("상위 등급 쓰레기 더미 가중치에 곱해지는 스테이지 배수. 업그레이드 배수와 별개로 누적 적용된다.")]
        [SerializeField, Min(0f)] private float _highTierWeightMultiplier = 1f;

        public int StageId => _stageId;
        public string DisplayName => _displayName;
        public string ShortDescription => _shortDescription;
        public MapGenerationSettings MapSettings => _mapSettings;
        public TileSet TileSet => _tileSet;
        public float CleanGoalToUnlockNext => _cleanGoalToUnlockNext;
        public float HighTierWeightMultiplier => _highTierWeightMultiplier;
    }
}
