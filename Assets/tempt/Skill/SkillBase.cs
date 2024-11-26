using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill/Create New Skill")]
public class SkillBase : ScriptableObject
{
    [SerializeField] private string skillName;
    [SerializeField] private float coolTime;
    [SerializeField] private float value;
    [SerializeField] private Sprite icon;
    [SerializeField] private int number;

    // プロパティでフィールドを公開
    public string SkillName => skillName;
    public float CoolTime => coolTime;
    public float Value => value;
    public Sprite Icon => icon;
    public int Number => number;
}

