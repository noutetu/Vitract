using UnityEngine;
using UniRx;
using System;

public interface ISkillStrategy
{
    void Activate(Character character, IDamageable target);
    IReadOnlyReactiveProperty<bool> CanUseSkill { get; }
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill/BasicSkill")]
public abstract class SkillData : ScriptableObject, ISkillStrategy
{
    public string skillName;
    public float value;  // スキルの倍率や影響度
    public float cooldownTime;  // クールダウン時間
    public Sprite icon;

    private ReactiveProperty<bool> canUseSkill = new ReactiveProperty<bool>(true);
    public IReadOnlyReactiveProperty<bool> CanUseSkill => canUseSkill;

    public abstract void Activate(Character character, IDamageable target);  // キャラクターを引数に追加

    private CompositeDisposable disposables = new CompositeDisposable();

    protected void StartCoolDown()
    {
        disposables.Clear();  // 既存のタイマーをクリア
        canUseSkill.Value = false;
        Observable.Timer(TimeSpan.FromSeconds(cooldownTime / GameManager.Instance.gameSpeed))
            .Subscribe(_ => canUseSkill.Value = true)
            .AddTo(disposables);
    }

}
