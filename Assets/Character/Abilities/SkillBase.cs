using UnityEngine;
using Vitract.Character.Core;

namespace Vitract.Character.Abilities
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "Vitract/Skill")]
    public class Skill : ScriptableObject
　    {
        [Header("Basic Info")]
        public string skillName;
        public Sprite skillIcon;

        [Header("Skill Details")]
        public float coolDown;
        public string description;
        public TargetType targetType;
        public SkillEffectType[] effectTypes; // 修正: 複数の効果を持てるように配列に変更
        public float duration; // 修正: スキルの継続時間を追加

        [Header("Skill Stats")]
        public int damageAmount;
        public int healAmount;

        public virtual void Activate(CharacterCore user, CharacterCore target)
        {
            // スキルの効果を実行
            // 必要に応じて継承して具体的な動作を定義
        }
    }

    public enum SkillEffectType
    {
        Damage,
        Heal,
        Buff,
        Debuff
    }

    public enum TargetType
    {
        Self,
        Enemy,
        Ally
    }
}