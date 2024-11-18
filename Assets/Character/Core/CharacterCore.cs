using UnityEngine;
using UniRx;
using Vitract.Character.Movement;

namespace Vitract.Character.Core
{
    public class CharacterCore : MonoBehaviour
    {
        // キャラクターベース
        [SerializeField] private CharacterBase characterBase;
        // 現在ステータス
        private CharacterCurrentParam currentParam;
        // 敵か味方かのフラグ
        [SerializeField] bool isPlayer;

        // Facades and other components
        [SerializeField] private Battle.BattleFacade battleFacade;
        [SerializeField] private Effects.EffectFacade effectFacade;
        [SerializeField] private UI.CharacterUIFacade characterUIFacade;
        [SerializeField] private CharacterMover characterMover;
        [SerializeField] private State.StateMachine stateMachine;

        public void Start()
        {
            if (characterMover == null)
            {
                Debug.Log("characterMover is null");
            }

            // AnimetorをEffectFacadeにセット
            Animator animator = GetComponentInChildren<Animator>();
            effectFacade = new Effects.EffectFacade(animator);

            // お試し移動
            characterMover.Move(characterBase.Stats.speed, isPlayer);
            effectFacade.Run(characterBase.Stats.speed);
        }
    }
}
