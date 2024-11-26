using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterState
{
    void EnterState(Character character);
    void UpdateState(Character character);
    void ExitState(Character character);
}

public class RunState : ICharacterState
{
    public void EnterState(Character character)
    {
    }

    public void UpdateState(Character character)
    {
        
    }

    public void ExitState(Character character)
    {
        
    }
}
public class IdleState : ICharacterState
{
    public void EnterState(Character character)
    {
        
    }

    public void UpdateState(Character character)
    {
        // 必要に応じて待機中の処理を追加
    }

    public void ExitState(Character character)
    {
        
    }
}
public class AttackState : ICharacterState
{
    public void EnterState(Character character)
    {
        
    }

    public void UpdateState(Character character)
    {
        
    }

    public void ExitState(Character character)
    {
        Debug.Log("Exiting Attack State");
    }
}
public class DieState : ICharacterState
{
    public void EnterState(Character character)
    {

    }

    public void UpdateState(Character character)
    {
        
    }

    public void ExitState(Character character)
    {
        
    }
}