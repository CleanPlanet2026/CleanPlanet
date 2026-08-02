namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 결과(수집물, 추첨 배수, 지급액). 감정 코어는 이 값을 이벤트로만 알리고
    /// 재화를 직접 소유·저장하지 않는다.
    /// </summary>
    public readonly struct AppraisalResult
    {
        public CollectibleData Item { get; }
        public int Multiplier { get; }
        public int Payout { get; }

        public AppraisalResult(CollectibleData item, int multiplier, int payout)
        {
            Item = item;
            Multiplier = multiplier;
            Payout = payout;
        }
    }
}
