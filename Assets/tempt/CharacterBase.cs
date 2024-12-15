
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class CharacterBase : ScriptableObject
{
    [Header("基本情報")]
    [Tooltip("キャラクターのID")]
    [SerializeField] int characterID;

    [Tooltip("対応するキャラクタープレハブ")]
    [SerializeField] Character prefab;
    public Character Prefab { get => prefab; set => prefab = value; }

    [Tooltip("キャラクターの名前")]
    [SerializeField] new string name;
    public string Name { get => name; set => name = value; }

    [Tooltip("配置に必要なコスト")]
    [SerializeField] int cost;
    public int Cost { get => cost; set => cost = value; }

    [Header("ステータス")]
    [Tooltip("キャラクターの最大体力")]
    [SerializeField] float maxHp;
    public float MaxHp { get => maxHp; set => maxHp = value; }

    [Tooltip("キャラクターの防御力")]
    [SerializeField] float defence;
    public float Defence { get => defence; set => defence = value; }

    [Tooltip("キャラクターの魔法耐性")]
    [SerializeField] float magicDefence;
    public float MagicDefence { get => magicDefence; set => magicDefence = value; }

    [Tooltip("キャラクターの攻撃力")]
    [SerializeField] float atk;
    public float Atk { get => atk; set => atk = value; }

    [Tooltip("キャラクターの攻撃クールタイム")]
    [SerializeField] float coolTime;
    public float CoolTime { get => coolTime; set => coolTime = value; }
    
    [Tooltip("キャラクターの攻撃速度")]
    [SerializeField] float attackSpeed;
    public float AttackSpeed { get => attackSpeed; set => attackSpeed = value; }

    [Tooltip("キャラクターの攻撃範囲")]
    [SerializeField] float range;
    public float Range { get => range; }

    [Tooltip("キャラクターの移動速度")]
    [SerializeField] float speed;
    public float Speed { get => speed; set => speed = value; }

    [Header("スキル")]
    [Tooltip("キャラクターの特別なスキル")]
    [SerializeField] SkillData specialSkill;
    public SkillData SpecialSkill { get => specialSkill; set => specialSkill = value; }

    [Tooltip("キャラクターの通常スキル")]
    [SerializeField] SkillData normalSkill;
    public SkillData NormalSkill { get => normalSkill; set => normalSkill = value; }

    [Header("設定")]
    [Tooltip("キャラクターの種類")]
    [SerializeField] CharacterType characterType;
    public CharacterType CharacterType { get => characterType; set => characterType = value; }

    [Tooltip("キャラクターの役職")]
    [SerializeField] string roleName;
    [TextArea(3, 5)] // 最小3行、最大5行
    [SerializeField] string description;

    [Tooltip("キャラクター画像")]
    [SerializeField] Sprite icon;
    public Sprite Icon { get => icon; set => icon = value; }
}


public enum CharacterType
{
    SwordMan,
    BowMan,
    Magician,
}
