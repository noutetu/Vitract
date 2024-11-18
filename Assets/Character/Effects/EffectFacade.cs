using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vitract.Character.Effects
{
    public class EffectFacade
    {
        AnimationManager animationManager;

        public EffectFacade(Animator animator)
        {
            animationManager = new AnimationManager(animator);
        }

        public void Run(float speed)
        {
            animationManager.RunAnim(speed);
        }
    }
}