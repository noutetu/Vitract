using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

// --------------- ジャンプアタック ---------------
[CreateAssetMenu(fileName = "IronWill", menuName = "Skill/IronWill")]
public class IronWill : SkillData
{
    [Tooltip("スキルの継続時間")]
    [SerializeField] private int time; // スキルの持続時間
    public int Time { get => time; }

    private float originalDefence; // 元の防御力を保存するフィールド

    public override void Activate(Character character, IDamageable target)
    {

        Debug.Log("IronWill activated!");
        // 元の防御力が未保存の場合のみ保存
        if (originalDefence == 0)
        {
            originalDefence = character.deffence;
        }

        // 倍率を計算
        float multiplier = Mathf.Clamp(Value / 100f, 0.1f, 3f);
        character.deffence = originalDefence * multiplier;

        Debug.Log($"Value: {Value}, Multiplier: {multiplier}, Original Defence: {originalDefence}");

        // Timerで防御力を元に戻し、クールダウンを開始
        Observable.Timer(System.TimeSpan.FromSeconds(time))
            .Subscribe(_ =>
            {
                character.deffence = originalDefence; // 防御力を元に戻す
                originalDefence = 0; // 元の防御力をリセット
                StartCoolDown();
                Debug.Log($"Defence reset to {originalDefence} after {time} seconds.");
            },
            () =>
            {
                Debug.Log("Timer subscription completed and disposed.");
            })
            .AddTo(character); // キャラクターのライフサイクルに購読を紐付ける
    }
}



