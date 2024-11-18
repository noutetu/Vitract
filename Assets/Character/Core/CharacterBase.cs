using UnityEngine;

namespace Vitract.Character.Core
{
    [CreateAssetMenu(fileName = "NewCharacterBase", menuName = "Vitract/CharacterBase")]
    [System.Serializable]
    public class CharacterBase : ScriptableObject
    {
        [Header("Basic Info")]
        // 対応するプレハブ
        [SerializeField] CharacterCore prefab;
        public CharacterCore Prefab { get => prefab; }
        // キャラクター画像
        [SerializeField] Sprite sprite;
        public Sprite Sprite { get => sprite; }
        // 名前
        [SerializeField] new string name;
        public string Name { get => name; }
        // ID
        [SerializeField] int id;
        public int Id { get => id; }
        // 進化後情報
        [SerializeField] CharacterBase next;
        public CharacterBase Next { get => next; }

        [Header("Stats")]
        // ステータス
        [SerializeField] CharacterStats stats;
        public CharacterStats Stats { get => stats; }
        // キャラクタータイプ
        [SerializeField] CharacterType characterType;
        public CharacterType CharacterType { get => characterType; }

        [Header("Abilities")]
        // スキル
        [SerializeField] SkillBase skill;
        public SkillBase Skill { get => skill; }

        [Header("Evolution Requirements")]
        // 進化に必要なアイテム情報
        [SerializeField] EvolutionRequirement evolutionRequirement;
        public EvolutionRequirement EvolutionRequirement { get => evolutionRequirement; }
    }

    public enum CharacterType
    {
        Vanguard,
        Frontline,
        Defender,
        Archer,
        Mage,
        AOE_Mage,
        Healer
    }

    [System.Serializable]
    public class CharacterStats
    {
        public int cost;
        public float maxHp;
        public float attackPower;
        public float attackSpeed;
        public float attackCoolTime;
        public float speed;
        public float defence;
        public float magicDefence;
        public float range;
    }

    [System.Serializable]
    public class EvolutionRequirement
    {
        // 進化に必要な仮のアイテム情報
        [SerializeField] string requiredItemName; // 必要アイテム名（仮）
        public string RequiredItemName { get => requiredItemName; }

        [SerializeField] int requiredItemCount; // 必要アイテムの数（仮）
        public int RequiredItemCount { get => requiredItemCount; }
    }
}
