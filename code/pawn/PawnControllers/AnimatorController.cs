using System;
using System.Collections.Generic;
using MyGame;
using Sandbox.Utils;

namespace Sandbox.pawn.PawnControllers;

public class AnimatorController : EntityComponent<Pawn>, ISingletonComponent, IEventListener
{
	private CitizenAnimationHelper m_helper;

	private int m_duckLevel = 0;
	
	protected override void OnActivate()
	{
		m_helper = new CitizenAnimationHelper( Entity );
		Entity.EventDispatcher.RegisterEvent<EventOnJump>( this, OnPawnJumped );
		Entity.EventDispatcher.RegisterEvent<EventOnDuck>( this, OnPawnDuck );
		Entity.EventDispatcher.RegisterEvent<EventOnUnDuck>( this, OnPawnUnduck );
		base.OnActivate();
	}
 
	private void OnPawnDuck( EventOnDuck obj )
	{
		m_duckLevel = 1;
	}
	
	private void OnPawnUnduck( EventOnUnDuck obj )
	{
		m_duckLevel = 0;
	} 

	private void OnPawnJumped( EventOnJump obj )
	{
		m_helper.TriggerJump();
	}

	protected override void OnDeactivate()
	{
		Entity.EventDispatcher.UnregisterAllEvents( this );
		base.OnDeactivate();
	}

	public void Simulate( IClient client )
	{
		m_helper.WithVelocity( Entity.Velocity );
		m_helper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100 );
		m_helper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		m_helper.IsGrounded = Entity.GroundEntity.IsValid();
		m_helper.DuckLevel = m_duckLevel;
	}
}
