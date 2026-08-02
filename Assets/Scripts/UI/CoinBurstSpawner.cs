using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CleanPlanet.UI
{
    /// <summary>
    /// 감정로봇 출력구에서 골드 HUD로 코인이 날아가는 연출을 발사한다.
    /// 코인 개수·크기·전체 길이는 AppraisalEffectDirector가 티어별로 지시하며,
    /// 이 컴포넌트는 그 지시를 받아 풀에서 코인을 꺼내 흩뿌리기만 한다.
    /// </summary>
    public sealed class CoinBurstSpawner : MonoBehaviour
    {
        [SerializeField] private CoinBurst _coinPrefab;
        [SerializeField] private RectTransform _poolParent;
        [SerializeField] private RectTransform _spawnAnchor;
        [SerializeField] private RectTransform _targetAnchor;

        [Header("연출")]
        [SerializeField, Min(1)] private int _poolSize = 512;

        [Header("폭발 범위(코인 수에 따라 넓어짐)")]
        [SerializeField, Min(1)] private int _radiusScaleBaseCoins = 20;
        [SerializeField, Min(1f)] private float _radiusScaleMax = 4f;

        private readonly List<CoinBurst> _pool = new List<CoinBurst>();

        private void Awake()
        {
            if (_coinPrefab == null)
            {
                return;
            }

            for (int i = 0; i < _poolSize; i++)
            {
                CreatePooledCoin();
            }
        }

        private void OnEnable()
        {
            if (_coinPrefab == null || _spawnAnchor == null || _targetAnchor == null)
            {
                Debug.LogError($"{nameof(CoinBurstSpawner)}에 필요한 참조가 없습니다.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// coinCount개의 코인을 발사한다. explode=false면 duration 동안 고루 퍼지는 스트림
        /// (stagger = duration / coinCount), explode=true면 stagger 없이 전부 동시에 스폰해
        /// 각 코인이 "흩어졌다 일제히 상승"하는 폭발 연출을 재생한다.
        /// coinScale은 티어별 코인 크기(잭팟일수록 큼)를 그대로 각 코인에 전달한다.
        /// </summary>
        public void Burst(int coinCount, float duration, float coinScale, bool explode)
        {
            StartCoroutine(RunBurst(coinCount, duration, coinScale, explode));
        }

        private IEnumerator RunBurst(int coinCount, float duration, float coinScale, bool explode)
        {
            Vector2 from = _spawnAnchor.anchoredPosition;
            Vector2 to = _targetAnchor.anchoredPosition;

            if (explode)
            {
                // 면적이 개수에 비례하도록 반경은 √(개수/기준)로 키우고 상한을 둔다.
                float radiusScale = Mathf.Min(_radiusScaleMax,
                    Mathf.Sqrt(coinCount / (float)_radiusScaleBaseCoins));
                radiusScale = Mathf.Max(1f, radiusScale);

                for (int i = 0; i < coinCount; i++)
                {
                    CoinBurst coin = GetPooledCoin();
                    coin.gameObject.SetActive(true);
                    coin.PlayExplode(from, to, coinScale, duration, radiusScale);
                }

                yield break;
            }

            float stagger = duration / Mathf.Max(1, coinCount);
            var wait = new WaitForSeconds(stagger);

            for (int i = 0; i < coinCount; i++)
            {
                CoinBurst coin = GetPooledCoin();
                coin.gameObject.SetActive(true);
                coin.Play(from, to, coinScale);

                if (stagger > 0f)
                {
                    yield return wait;
                }
            }
        }

        private CoinBurst GetPooledCoin()
        {
            foreach (CoinBurst coin in _pool)
            {
                if (!coin.gameObject.activeSelf)
                {
                    return coin;
                }
            }

            return CreatePooledCoin();
        }

        private CoinBurst CreatePooledCoin()
        {
            Transform parent = _poolParent != null ? _poolParent : transform;
            CoinBurst coin = Instantiate(_coinPrefab, parent);
            coin.gameObject.SetActive(false);
            _pool.Add(coin);
            return coin;
        }
    }
}
