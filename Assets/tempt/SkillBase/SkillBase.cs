using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SkillBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] float coolTime;
    [SerializeField] Target target;
    [SerializeField] float Value;
    [SerializeField] Sprite sprite;
    [SerializeField] int number;
}

public enum Target
{
    Self,
    TeamMate,
    Enemy,
}
