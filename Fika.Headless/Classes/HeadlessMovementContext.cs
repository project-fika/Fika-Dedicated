using EFT;
using HarmonyLib;
using System;
using UnityEngine;

namespace Fika.Core.Coop.ObservedClasses
{
    public class HeadlessMovementContext : MovementContext
    {
        private Action<CollisionFlags> OnMotion;

        public new static HeadlessMovementContext Create(Player player, Func<IAnimator> animatorGetter, Func<ICharacterController> characterControllerGetter, LayerMask groundMask)
        {
            HeadlessMovementContext movementContext = Create<HeadlessMovementContext>(player, animatorGetter, characterControllerGetter, groundMask);
            movementContext.OnMotion = Traverse.Create(movementContext).Field<Action<CollisionFlags>>("OnMotionApplied").Value;
            if (movementContext.OnMotion == null)
            {
                throw new NullReferenceException("Could not find OnMotionApplied event");
            }
            return movementContext;
        }

        public override void DirectApplyMotion(Vector3 motion, float deltaTime)
        {
            CollisionFlags collisionFlags = CharacterController.Move(motion + PlatformMotion, deltaTime);
            method_1(motion);
            OnMotion?.Invoke(collisionFlags);
        }
    }
}
