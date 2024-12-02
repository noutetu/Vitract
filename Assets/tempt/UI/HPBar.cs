using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniRx;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

// HPの増減の描画
public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float HP)
    {
        health.transform.localScale = new Vector3(HP, 1, 1);
    }

    public void UpdateHP(float newHP)
    {
        if(health != null)
        {
            health.transform.localScale = new Vector3(newHP, 1, 1);
        }
    }

    public void SubscribeValue(ReactiveProperty<float> Value,float maxValue)
    {
        Value.DistinctUntilChanged()
        .Subscribe(value =>
        {
            UpdateHP(value/maxValue);
        });
    }

    /*public IEnumerator SetHPSmooth(float newHP)
    {
        float currentHP = health.transform.localScale.x;
        float changeAmount = currentHP - newHP;

        while (currentHP - newHP > Mathf.Epsilon)
        {
            currentHP -= changeAmount * Time.deltaTime;
            health.transform.localScale = new Vector3(currentHP, 1, 1);
            yield return null;
        }

        health.transform.localScale = new Vector3(newHP, 1, 1);
    }*/
}
