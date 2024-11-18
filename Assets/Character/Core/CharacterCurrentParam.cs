using UniRx;
using UnityEngine;

namespace Vitract.Character.Core
{
    public class CharacterCurrentParam
    {
        CharacterStats currentStats;
        public CharacterStats CurrentStats { get => currentStats; set => currentStats = value; }
        public ReactiveProperty<float> CurrentHp { get; private set; }
        public ReactiveProperty<float> AttackPower { get; private set; }

        public CharacterCurrentParam(CharacterBase characterBase)
        {
            // 深いコピーを行い、元のデータが変わらないようにする
            CurrentStats = new CharacterStats
            {
                cost = characterBase.Stats.cost,
                maxHp = characterBase.Stats.maxHp,
                attackPower = characterBase.Stats.attackPower,
                attackSpeed = characterBase.Stats.attackSpeed,
                attackCoolTime = characterBase.Stats.attackCoolTime,
                speed = characterBase.Stats.speed,
                defence = characterBase.Stats.defence,
                magicDefence = characterBase.Stats.magicDefence,
                range = characterBase.Stats.range
            };

            CurrentHp = new ReactiveProperty<float>(characterBase.Stats.maxHp);
            AttackPower = new ReactiveProperty<float>(characterBase.Stats.attackPower);
        }
        public void ApplyDamage(float damage)
        {
            CurrentHp.Value = Mathf.Max(CurrentHp.Value - damage, 0);
        }
        public void Heal(float healAmount)
        {
            CurrentHp.Value = Mathf.Min(CurrentHp.Value + healAmount, CurrentStats.maxHp);
        }
    }
}