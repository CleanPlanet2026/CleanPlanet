using UnityEngine;

namespace CleanPlanet.Core
{
    /// <summary>
    /// 스테이지 선택 화면에서 고른 스테이지 인덱스를 GameScene으로 넘기는 런타임 전용 상태.
    /// 저장 데이터가 아니라 현재 플레이 세션에서만 유효하다.
    /// </summary>
    public static class StageSessionState
    {
        public static int SelectedStageIndex { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            SelectedStageIndex = 0;
        }
    }
}
