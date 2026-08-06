using System.Collections.Generic;
using CleanPlanet.Core.Appraisal;
using CleanPlanet.Core.Collection;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CleanPlanet.UI
{
    /// <summary>
    /// 디버그용: 프로젝트의 모든 CollectibleData를 종류별로 1개씩 CollectionInbox에 넣는다.
    /// 감정 로봇 테스트 시 표본을 수동으로 채우지 않아도 되게 한다.
    /// 같은 오브젝트의 Button을 자동 연결하며, 버튼 없이 컴포넌트 우클릭 메뉴로도 실행할 수 있다.
    /// CollectibleData 로드는 에디터 전용이다.
    /// </summary>
    public sealed class CollectibleInboxDebugButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_button != null)
            {
                _button.onClick.AddListener(AddOneOfEachToInbox);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(AddOneOfEachToInbox);
            }
        }

        [ContextMenu("수집물 종류별 1개씩 Inbox에 추가")]
        public void AddOneOfEachToInbox()
        {
            int added = 0;
            foreach (CollectibleData item in LoadAllCollectibles())
            {
                CollectionInbox.Add(item, 1);
                added++;
            }

            Debug.Log($"[Debug] 수집물 {added}종을 Inbox에 1개씩 추가했습니다. 감정 화면을 새로 열면 반영됩니다.", this);
        }

        [ContextMenu("Inbox 전부 비우기 (저장까지)")]
        public void ClearInbox()
        {
            int before = CollectionInbox.Count;
            CollectionInbox.Clear();
            Debug.Log($"[Debug] Inbox를 비웠습니다. ({before}개 삭제, 저장 반영). 관에 남은 아이콘은 씬을 다시 열면 사라집니다.", this);
        }

        [ContextMenu("Inbox 내용 로그")]
        public void LogInbox()
        {
            IReadOnlyList<string> ids = CollectionInbox.PendingIds;
            if (ids.Count == 0)
            {
                Debug.Log("[Debug] Inbox가 비어 있습니다.", this);
                return;
            }

            var counts = new Dictionary<string, int>();
            foreach (string id in ids)
            {
                counts.TryGetValue(id, out int c);
                counts[id] = c + 1;
            }

            var lines = new List<string>();
            foreach (KeyValuePair<string, int> pair in counts)
            {
                lines.Add($"  {pair.Key} x{pair.Value}");
            }

            Debug.Log($"[Debug] Inbox 총 {ids.Count}개:\n{string.Join("\n", lines)}", this);
        }

        private static IEnumerable<CollectibleData> LoadAllCollectibles()
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(CollectibleData)}");
            var results = new List<CollectibleData>(guids.Length);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CollectibleData item = AssetDatabase.LoadAssetAtPath<CollectibleData>(path);
                if (item != null)
                {
                    results.Add(item);
                }
            }

            return results;
#else
            Debug.LogWarning($"{nameof(CollectibleInboxDebugButton)}은(는) 에디터 전용입니다.");
            return System.Array.Empty<CollectibleData>();
#endif
        }
    }
}
