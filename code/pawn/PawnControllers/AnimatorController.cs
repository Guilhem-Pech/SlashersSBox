using System;
using System.Collections.Generic;
using MyGame;
using Sandbox.Utils;

namespace Sandbox.pawn.PawnControllers;

public class AnimatorController : EntityComponent<Pawn>, ISingletonComponent, IEventListener
{
	
	HashSet<string> AnimationEvents = new( StringComparer.OrdinalIgnoreCase );
	protected override void OnActivate()
	{
		Entity.EventDispatcher.RegisterEvent<EventOnJump>( this, OnPawnJumped );
		base.OnActivate();
	}

	private void OnPawnJumped( EventOnJump obj )
	{
		AnimationEvents.Add( "jump" );
	}

	protected override void OnDeactivate()
	{
		Entity.EventDispatcher.UnregisterAllEvents( this );
		base.OnDeactivate();
	}

	public void Simulate( IClient client )
	{
		var helper = new CitizenAnimationHelper( Entity );
		helper.WithVelocity( Entity.Velocity );
		helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
		helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		helper.IsGrounded = Entity.GroundEntity.IsValid();

		if ( AnimationEvents.Remove( "jump" ) )
		{
			helper.TriggerJump();
		}
	}
}
