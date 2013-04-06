using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine;
using Nine.Animations;
using Nine.Graphics;

namespace XNAControlGame
{
    class AttackAndRun : Component
    {
        protected override void OnAdded(Group parent)
        {
            var model = parent.Find<Nine.Graphics.Model>();
            var attack = new BoneAnimationController(model.Source.GetAnimation("Attack"));
            var run = new BoneAnimationController(model.Source.GetAnimation("Run"));
            run.Speed = 0.8f;

            var blended = new BoneAnimation(model.Skeleton);
            blended.Controllers.Add(run);
            blended.Controllers.Add(attack);

            blended.Controllers[run].Disable("Bip01_Pelvis", false);
            blended.Controllers[run].Disable("Bip01_Spine1", true);

            blended.Controllers[attack].Disable("Bip01", false);
            blended.Controllers[attack].Disable("Bip01_Spine", false);
            blended.Controllers[attack].Disable("Bip01_L_Thigh", true);
            blended.Controllers[attack].Disable("Bip01_R_Thigh", true);

            blended.KeyController = run;
            blended.IsSychronized = true;

            model.Animations.Play(blended);
        }
    }
}
