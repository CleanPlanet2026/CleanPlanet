using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.Core.Appraisal
{
    /// <summary>
    /// 감정 슬롯의 릴 하나(왼쪽 아이콘 또는 오른쪽 배수)를 담당한다.
    /// 고정 개수의 셀을 링 버퍼처럼 재활용해 세로로 흐르는 스핀 연출을 만들고,
    /// 정착(settle) 구간에서는 다음에 중앙에 들어올 셀에 정답을 미리 채운 뒤
    /// 그 시점의 정확한 남은 거리만큼만 ease-out으로 트윈한다. 거리·시간이
    /// 정착 시작 순간에 확정되므로 프레임레이트와 무관하게 항상 정중앙에서 멈춘다.
    /// </summary>
    public sealed class AppraisalReel : MonoBehaviour
    {
        private enum ReelMode
        {
            Icon,
            Number
        }

        [Serializable]
        private sealed class ReelCell
        {
            [SerializeField] private RectTransform _rectTransform;
            [SerializeField] private Image _icon;
            [SerializeField] private Text _number;

            public RectTransform RectTransform => _rectTransform;

            /// <summary>
            /// 대기 상태에서는 빈 Image가 흰 사각으로 그려지는 걸 막기 위해 스핀이
            /// 시작되기 전까지 아이콘/숫자를 완전히 숨긴다.
            /// </summary>
            public void SetVisible(bool visible)
            {
                if (_icon != null)
                {
                    _icon.enabled = visible;
                }

                if (_number != null)
                {
                    _number.enabled = visible;
                }
            }

            public void SetIcon(Sprite sprite)
            {
                if (_icon != null)
                {
                    _icon.sprite = sprite;
                }
            }

            public void SetNumber(string text)
            {
                if (_number != null)
                {
                    _number.text = text;
                }
            }
        }

        [SerializeField] private ReelMode _mode;
        [SerializeField] private RectTransform _window;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ReelCell[] _cells;
        [SerializeField, Min(0.01f)] private float _cellHeight = 100f;
        [SerializeField, Min(0f)] private float _scrollSpeed = 800f;
        [SerializeField, Range(0f, 1f)] private float _settlePortion = 0.35f;

        [SerializeField] private Sprite[] _decoyIcons;
        [SerializeField] private int[] _decoyMultipliers = { 1, 2, 4, 8, 16 };

        [SerializeField] private AudioSource _tickAudioSource;
        [SerializeField] private AudioClip _tickClip;
        [SerializeField] private AudioClip _settleClip;

        public bool IsSpinning { get; private set; }

        private bool _isValid;
        private Coroutine _spinCoroutine;

        private void Awake()
        {
            _isValid = _window != null && _content != null && _cells != null && _cells.Length > 0;
            if (!_isValid)
            {
                Debug.LogError($"{nameof(AppraisalReel)}에 필요한 참조가 없습니다.", this);
                return;
            }

            HideAllCells();
        }

        private void OnEnable()
        {
            if (_isValid && !IsSpinning)
            {
                HideAllCells();
            }
        }

        private void OnDisable()
        {
            StopSpin();
        }

        private void HideAllCells()
        {
            foreach (ReelCell cell in _cells)
            {
                cell.SetVisible(false);
            }
        }

        public void SpinToIcon(Sprite target, float duration)
        {
            if (!_isValid)
            {
                return;
            }

            if (_mode != ReelMode.Icon)
            {
                Debug.LogWarning($"{nameof(AppraisalReel)}: Icon 모드가 아닌 릴에 {nameof(SpinToIcon)}이 호출되었습니다.", this);
                return;
            }

            BeginSpin(duration, cell => cell.SetIcon(target), FillDecoyIcon);
        }

        public void SpinToNumber(int target, float duration)
        {
            if (!_isValid)
            {
                return;
            }

            if (_mode != ReelMode.Number)
            {
                Debug.LogWarning($"{nameof(AppraisalReel)}: Number 모드가 아닌 릴에 {nameof(SpinToNumber)}이 호출되었습니다.", this);
                return;
            }

            string targetText = target.ToString();
            BeginSpin(duration, cell => cell.SetNumber(targetText), FillDecoyNumber);
        }

        private void BeginSpin(float duration, Action<ReelCell> applyTarget, Action<ReelCell> applyDecoy)
        {
            StopSpin();
            _spinCoroutine = StartCoroutine(RunSpin(duration, applyTarget, applyDecoy));
        }

        private void StopSpin()
        {
            if (_spinCoroutine != null)
            {
                StopCoroutine(_spinCoroutine);
                _spinCoroutine = null;
            }

            IsSpinning = false;
        }

        private IEnumerator RunSpin(float duration, Action<ReelCell> applyTarget, Action<ReelCell> applyDecoy)
        {
            IsSpinning = true;

            int cellCount = _cells.Length;
            var cellLocalY = new float[cellCount];

            // 초기 배치: 절반은 창 위쪽, 절반은 아래쪽에 걸치도록 흩어놓아 스핀 시작 즉시
            // 자연스럽게 흐르는 것처럼 보이게 한다. 실제 화면상의 위치는 스크립트가 전담한다.
            for (int i = 0; i < cellCount; i++)
            {
                _cells[i].SetVisible(true);
                cellLocalY[i] = (cellCount / 2f - i) * _cellHeight;
                SetCellY(_cells[i], cellLocalY[i]);
                applyDecoy(_cells[i]);
            }

            SetContentY(0f);

            if (duration <= 0f)
            {
                int immediateIndex = cellCount / 2;
                applyTarget(_cells[immediateIndex]);
                SetCellY(_cells[immediateIndex], 0f);
                SetContentY(0f);
                IsSpinning = false;
                _spinCoroutine = null;
                yield break;
            }

            float spinDuration = duration * (1f - _settlePortion);
            float settleDuration = duration - spinDuration;
            float recycleBound = -(_window.rect.height * 0.5f + _cellHeight);
            float scrollDistance = 0f;

            // 셀 하나가 지나갈 때마다 한 번씩 틱을 울린다. 정착 구간의 ease-out으로 셀당
            // 진행 거리가 점점 짧아지므로 틱 간격도 자연히 벌어져 슬롯머신처럼 감속한다.
            float nextTickDistance = _cellHeight;

            // 스핀 구간: 일정 속도로 계속 흘려보내다가, 창 아래로 완전히 빠져나간 셀만
            // 반대쪽(위)으로 재배치하고 새 디코이 값을 채워 무한 스크롤처럼 보이게 한다.
            float spinElapsed = 0f;
            while (spinElapsed < spinDuration)
            {
                float dt = Time.deltaTime;
                spinElapsed += dt;
                scrollDistance += _scrollSpeed * dt;
                SetContentY(-scrollDistance);

                while (scrollDistance >= nextTickDistance)
                {
                    PlayTick();
                    nextTickDistance += _cellHeight;
                }

                for (int i = 0; i < cellCount; i++)
                {
                    bool recycled = false;
                    while (cellLocalY[i] - scrollDistance < recycleBound)
                    {
                        cellLocalY[i] += cellCount * _cellHeight;
                        recycled = true;
                    }

                    if (recycled)
                    {
                        SetCellY(_cells[i], cellLocalY[i]);
                        applyDecoy(_cells[i]);
                    }
                }

                yield return null;
            }

            // 정착 구간: 다음에 중앙에 들어올 셀을 정답으로 확정한다. 이 순간 남은 거리와
            // 남은 시간이 둘 다 고정값이므로, ease-out으로 트윈하면 프레임레이트와 무관하게
            // 항상 정확히 창 정중앙에서 멈춘다.
            int targetIndex = FindNextCenteringCell(cellLocalY, scrollDistance);
            applyTarget(_cells[targetIndex]);

            float targetDistance = cellLocalY[targetIndex];
            float startDistance = scrollDistance;
            float distanceToTravel = targetDistance - startDistance;

            float settleElapsed = 0f;
            while (settleElapsed < settleDuration)
            {
                settleElapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(settleElapsed / settleDuration);
                float eased = 1f - Mathf.Pow(1f - progress, 3f);
                scrollDistance = startDistance + distanceToTravel * eased;
                SetContentY(-scrollDistance);

                // 마지막 칸(정답이 들어오는 칸)의 틱은 생략한다. 그 자리는 착지 순간의
                // clank가 대신 울려 "틱틱틱…쿵" 하고 멈추는 느낌을 만든다.
                while (scrollDistance >= nextTickDistance && nextTickDistance < targetDistance)
                {
                    PlayTick();
                    nextTickDistance += _cellHeight;
                }

                yield return null;
            }

            // 최종값 보장: 누적 오차나 지속 시간 0 설정과 무관하게 실제 값과 정중앙 위치로 강제 고정.
            SetContentY(-targetDistance);
            applyTarget(_cells[targetIndex]);
            PlaySettle();

            IsSpinning = false;
            _spinCoroutine = null;
        }

        /// <summary>
        /// 스크롤이 계속 진행될 때 다음으로 중앙에 도달할(아직 지나치지 않은) 셀을 찾는다.
        /// 후보가 전혀 없는 예외적인 경우엔 창에서 가장 위쪽에 있는 셀로 대체한다.
        /// </summary>
        private static int FindNextCenteringCell(float[] cellLocalY, float scrollDistance)
        {
            int bestIndex = -1;
            float bestValue = float.PositiveInfinity;

            for (int i = 0; i < cellLocalY.Length; i++)
            {
                float y = cellLocalY[i];
                if (y > scrollDistance && y < bestValue)
                {
                    bestValue = y;
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0)
            {
                return bestIndex;
            }

            int fallbackIndex = 0;
            float fallbackValue = float.NegativeInfinity;
            for (int i = 0; i < cellLocalY.Length; i++)
            {
                if (cellLocalY[i] > fallbackValue)
                {
                    fallbackValue = cellLocalY[i];
                    fallbackIndex = i;
                }
            }

            return fallbackIndex;
        }

        private void PlayTick()
        {
            if (_tickAudioSource == null || _tickClip == null)
            {
                return;
            }

            _tickAudioSource.PlayOneShot(_tickClip);
        }

        private void PlaySettle()
        {
            if (_tickAudioSource == null || _settleClip == null)
            {
                return;
            }

            _tickAudioSource.PlayOneShot(_settleClip);
        }

        private void FillDecoyIcon(ReelCell cell)
        {
            if (_decoyIcons == null || _decoyIcons.Length == 0)
            {
                return;
            }

            cell.SetIcon(_decoyIcons[UnityEngine.Random.Range(0, _decoyIcons.Length)]);
        }

        private void FillDecoyNumber(ReelCell cell)
        {
            if (_decoyMultipliers == null || _decoyMultipliers.Length == 0)
            {
                return;
            }

            int value = _decoyMultipliers[UnityEngine.Random.Range(0, _decoyMultipliers.Length)];
            cell.SetNumber(value.ToString());
        }

        private void SetContentY(float y)
        {
            Vector2 pos = _content.anchoredPosition;
            pos.y = y;
            _content.anchoredPosition = pos;
        }

        private static void SetCellY(ReelCell cell, float y)
        {
            RectTransform rt = cell.RectTransform;
            if (rt == null)
            {
                return;
            }

            Vector2 pos = rt.anchoredPosition;
            pos.y = y;
            rt.anchoredPosition = pos;
        }
    }
}
