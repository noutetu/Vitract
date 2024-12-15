using UnityEditor;
using UnityEngine;

public class SkillDataCreator
{
    [MenuItem("Assets/Create/Skill/BasicSkill Auto Save to Resources")]
    public static void CreateSkillData()
    {
        // SkillDataオブジェクトを生成
        SkillData newSkillData = ScriptableObject.CreateInstance<SkillData>();

        // Resources/Skill フォルダのパスを指定
        string resourcesPath = "Assets/Resources/Skill";

        // Resources/Skill フォルダが存在しない場合は作成する
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Skill");
        }

        // 新しいスキルデータの保存パスを作成
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{resourcesPath}/NewSkill.asset");

        // スキルデータをアセットとして保存
        AssetDatabase.CreateAsset(newSkillData, assetPath);
        AssetDatabase.SaveAssets();

        // 保存したアセットを選択
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newSkillData;

        Debug.Log($"新しいスキルデータ '{newSkillData.SkillName}' が {assetPath} に保存されました。");
    }
}
