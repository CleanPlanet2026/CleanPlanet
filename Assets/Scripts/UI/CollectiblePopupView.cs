using System.Collections;
using System.Collections.Generic;
using CleanPlanet.Trash;
using UnityEngine;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 쓰레기 더미에서 수집물을 얻을 때마다 "이름 x개수" 한 줄을 화면 우측에 띄운다.
    /// 각 줄은 일정 시간 뒤 사라지며, 남은 줄은 위에서부터 다시 쌓인다.
    /// </summary>
    public sealed class CollectiblePopupView : MonoBehaviour
    {
        [SerializeField] private TrashInteractionController _interaction;
        [SerializeField] private Text _lineTemplate;
        [SerializeField] private RectTransform _container;
        [SerializeField, Min(0f)] private float _lineLifetime = 3f;
        [SerializeField, Min(0f)] private float _lineHeight = 36f;

        private readonly List<Text> _activeLines = new();

        private void OnEnable()
        {
            if (_interaction == null || _lineTemplate == null || _container == null)
            {
                Debug.LogError($"{nameof(CollectiblePopupView)}에 필요한 참조가 없습니다.", this);
                enabled = false;
                return;
            }

            _lineTemplate.gameObject.SetActive(false);
            _interaction.OnCollectibleRewardGranted += HandleRewardGranted;
        }

        private void OnDisable()
        {
            if (_interaction != null)
            {
                _interaction.OnCollectibleRewardGranted -= HandleRewardGranted;
            }
        }

        private void HandleRewardGranted(TrashPile trash, int count)
        {
            if (count <= 0 || trash.Reward == null)
            {
                return;
            }

            Text line = Instantiate(_lineTemplate, _container);
            line.text = $"{trash.Reward.Name} x{count}";
            line.gameObject.SetActive(true);
            _activeLines.Add(line);
            RepositionLines();

            StartCoroutine(RemoveAfterDelay(line));
        }

        private IEnumerator RemoveAfterDelay(Text line)
        {
            yield return new WaitForSeconds(_lineLifetime);

            _activeLines.Remove(line);
            Destroy(line.gameObject);
            RepositionLines();
        }

        private void RepositionLines()
        {
            for (int i = 0; i < _activeLines.Count; i++)
            {
                var rect = (RectTransform)_activeLines[i].transform;
                rect.anchoredPosition = new Vector2(0f, -i * _lineHeight);
            }
        }
    }
}
