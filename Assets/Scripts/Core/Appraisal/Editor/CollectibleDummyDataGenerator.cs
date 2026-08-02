using UnityEditor;
using UnityEngine;

namespace CleanPlanet.Core.Appraisal.Editor
{
    /// <summary>
    /// 감정로봇 테스트용 더미 CollectibleData 16개(등급별 4변형)를 생성하는 에디터 전용 도구.
    /// 사람이 Unity 메뉴에서 직접 실행한다(배치모드 자동 실행 없음).
    /// 이미 만든 에셋을 다시 실행하면 같은 경로를 덮어써 반복 실행해도 안전하다.
    /// </summary>
    internal static class CollectibleDummyDataGenerator
    {
        private const string OutputFolder = "Assets/ScriptableObjects/Collectibles";

        private readonly struct GradeInfo
        {
            public readonly string SpriteFolder;
            public readonly string Name;
            public readonly ItemGrade Grade;
            public readonly int BaseValue;

            public GradeInfo(string spriteFolder, string name, ItemGrade grade, int baseValue)
            {
                SpriteFolder = spriteFolder;
                Name = name;
                Grade = grade;
                BaseValue = baseValue;
            }
        }

        private static readonly GradeInfo[] Grades =
        {
            new("common", "고철", ItemGrade.Common, 10),
            new("glass", "유리", ItemGrade.Uncommon, 40),
            new("electric", "전자", ItemGrade.Rare, 150),
            new("jewel", "보석", ItemGrade.Epic, 600)
        };

        [MenuItem("CleanPlanet/Appraisal/더미 CollectibleData 16개 생성")]
        private static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Collectibles");
            }

            int created = 0;

            foreach (var info in Grades)
            {
                for (int variant = 1; variant <= 4; variant++)
                {
                    string spritePath = $"Assets/Art/Sprites/Items/{info.SpriteFolder}/item_{info.SpriteFolder}_{variant}.png";
                    Sprite icon = LoadSprite(spritePath);
                    if (icon == null)
                    {
                        Debug.LogWarning($"{nameof(CollectibleDummyDataGenerator)}: 스프라이트를 찾을 수 없습니다. {spritePath}");
                        continue;
                    }

                    string assetPath = $"{OutputFolder}/Collectible_{info.Grade}_{variant}.asset";
                    CreateCollectibleAsset(assetPath, info, icon);
                    created++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{nameof(CollectibleDummyDataGenerator)}: {created}개 더미 CollectibleData 생성 완료 ({OutputFolder}).");
        }

        private static Sprite LoadSprite(string path)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                {
                    return sprite;
                }
            }

            return null;
        }

        private static void CreateCollectibleAsset(string assetPath, GradeInfo info, Sprite icon)
        {
            if (AssetDatabase.LoadAssetAtPath<CollectibleData>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            var data = ScriptableObject.CreateInstance<CollectibleData>();
            var serialized = new SerializedObject(data);
            serialized.FindProperty("_name").stringValue = info.Name;
            serialized.FindProperty("_grade").enumValueIndex = (int)info.Grade;
            serialized.FindProperty("_baseValue").intValue = info.BaseValue;
            serialized.FindProperty("_icon").objectReferenceValue = icon;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(data, assetPath);
        }
    }
}
