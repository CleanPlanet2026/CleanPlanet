using UnityEngine.InputSystem;

namespace CleanPlanet.Utils
{
    /// <summary>
    /// 씬 전환 직후 홀드형 입력이 곧바로 재작동하는 것을 막는 정적 게이트.
    /// 마우스를 누른 채로 씬 전환을 유발하면(홀드 버튼 확정 등) 새 씬의 버튼이나 클릭 이동이
    /// 이어지는 같은 입력을 새 입력으로 오인해 반응해버릴 수 있어, 완전히 뗐다가
    /// 다시 누르기 전까지 잠근다.
    /// </summary>
    public static class PointerGate
    {
        public static bool IsLocked { get; private set; }

        public static void Lock()
        {
            IsLocked = true;
        }

        /// <summary>매 프레임 호출한다. 마우스 왼쪽 버튼이 떨어져 있으면 잠금을 해제한다.</summary>
        public static void ReleaseIfButtonUp()
        {
            if (IsLocked && Mouse.current != null && !Mouse.current.leftButton.isPressed)
            {
                IsLocked = false;
            }
        }
    }
}
