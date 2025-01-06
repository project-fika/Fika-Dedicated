using EFT;
using System;
using UnityEngine;

namespace Fika.Core.Coop.ObservedClasses
{
	public class HeadlessMovementContext : MovementContext
	{
		public override void ApplyGravity(ref Vector3 motion, float deltaTime, bool stickToGround)
		{
			// Do nothing
		}

		public new static HeadlessMovementContext Create(Player player, Func<IAnimator> animatorGetter, Func<ICharacterController> characterControllerGetter, LayerMask groundMask)
		{
			HeadlessMovementContext movementContext = Create<HeadlessMovementContext>(player, animatorGetter, characterControllerGetter, groundMask);
			return movementContext;
		}

		public override void DirectApplyMotion(Vector3 motion, float deltaTime)
		{
			// Do nothing
		}
	}
}
