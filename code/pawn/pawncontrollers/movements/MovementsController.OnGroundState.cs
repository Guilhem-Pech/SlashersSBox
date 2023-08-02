using System;
using Coroutines;
using Coroutines.Stallers;
using Sandbox.Utils;
using CoroutineEnumerator = System.Collections.Generic.IEnumerator<Coroutines.ICoroutineStaller>;

namespace Sandbox.pawn.PawnControllers;

public class EventOnJump : Utils.Event
{}

public class EventOnGrounded : Utils.Event
{}

class EventOnDuck : Utils.Event
{}

class EventOnUnDuck : Utils.Event
{}

public partial class MovementsController
{
	private float m_desiredMaxSpeed = 0f;
	public float SurfaceFriction { get; set; }

	private void OnEnterGroundState()
	{
		Entity.Velocity = Entity.Velocity.WithZ( 0 );
		Entity.EventDispatcher.SendEvent<EventOnGrounded>();
	}

	private void OnUpdateGroundState()
	{
		var movement = Entity.InputDirection.Normal;
		var angles = Entity.ViewAngles.WithPitch( 0 );
		var moveVector = Rotation.From( angles ) * movement * m_desiredMaxSpeed;
		var groundEntity = CheckForGround();
		
		Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 800f, 7.5f );
		Entity.Velocity = ApplyFriction( Entity.Velocity, 4.0f );

		if ( Input.Pressed( "jump" ) && CanJump())
		{
			DoJump();
		}

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity );

		if ( mh.TryUnstuck() )
		{
			Entity.Position = mh.Position;
		}
		
		if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
		{
			mh.Position = StayOnGround( mh.Position );
			
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}

		Entity.GroundEntity = groundEntity;
		CurrentStamina.Update( Time.Delta );
	}
	
	Vector3 ApplyFriction( Vector3 input, float frictionAmount )
	{
		const float stopSpeed = 100.0f;

		var speed = input.Length;
		if ( speed < 0.1f ) return input;

		// Bleed off some speed, but if we have less than the bleed
		// threshold, bleed the threshold amount.
		float control = (speed < stopSpeed) ? stopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;
		if ( Math.Abs(newspeed - speed) < float.Epsilon ) return input;

		newspeed /= speed;
		input *= newspeed;

		return input; 
	}

	Vector3 Accelerate( Vector3 input, Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		var currentspeed = input.Dot( wishdir );
		var addspeed = wishspeed - currentspeed;

		if ( addspeed <= 0 )
			return input;

		var accelspeed = acceleration * Time.Delta * wishspeed;

		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		input += wishdir * accelspeed;

		return input;
	}
	
	Vector3 StayOnGround( Vector3 position )
	{
		var start = position + Vector3.Up * 2;
		var end = position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = Entity.TraceBBox( position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = Entity.TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return position;
		if ( trace.Fraction >= 1 ) return position;
		if ( trace.StartedSolid ) return position;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return position;

		return trace.EndPosition;
	}
	
	void DoJump()
	{
		Entity.Velocity = ApplyJump( Entity.Velocity );
		Entity.EventDispatcher.SendEvent<EventOnJump>();
	}
	
	Vector3 ApplyJump( Vector3 input )
	{
		return input + Vector3.Up * JumpSpeed;
	}
	private void OnExitGroundState()
	{
	}

	// Jog state
	private void OnEnterJogState()
	{
		m_desiredMaxSpeed = JogSpeed;
	}
	
	
	//Sprint state
	private void OnEnterSprintState()
	{
		m_desiredMaxSpeed = SprintSpeed;
	}

	private void OnUpdateSprintState()
	{
		if (SqrCurrentSpeed > JogSpeed)
		{
			CurrentStamina.DesiredStaminaModifierRate = -1;
		}
		else if (SqrCurrentSpeed <= WalkSpeed)
		{
			CurrentStamina.DesiredStaminaModifierRate = 1;
		}
	}

	private void OnExitSprintState()
	{
		CurrentStamina.DesiredStaminaModifierRate = 1;
	}
	

	//Walk state
	private void OnEnterWalkState()
	{
		m_desiredMaxSpeed = WalkSpeed; 
	}
	
	//Duck state
	private CoroutineEnumerator m_duckCoroutine;

	private void OnEnterDuckState()
	{
		m_desiredMaxSpeed = DuckSpeed;
		Entity.EventDispatcher.SendEvent<EventOnDuck>();
		
		ResizeHull( Entity.DuckHull );
	}
	
	CoroutineEnumerator ResizeDuckHull(BBox to)
	{
		var curHull = Entity.Hull;
		float duckLerp = 0f;
		
		while ( duckLerp < 1 )
		{
			Entity.Hull = curHull.Lerp( to, duckLerp += (Game.TickInterval * 10f) );
			yield return new WaitForNextSimulate(Entity.Client);
		}
		Entity.Hull = to; 
	}
	
	private void OnExitDuckState()
	{
		ResizeHull(Entity.DefaultHull);
		Entity.EventDispatcher.SendEvent<EventOnUnDuck>();
	}

	private void ResizeHull(BBox to)
	{
		Coroutine.Stop(m_duckCoroutine);
		m_duckCoroutine = ResizeDuckHull(to);
		Coroutine.Start(m_duckCoroutine);
	}
}

class EventOnSprinting : Utils.Event
{
	public bool Start;

	public EventOnSprinting( bool start )
	{
		Start = start;
	}
}


