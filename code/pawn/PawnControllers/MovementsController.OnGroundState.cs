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
	private float m_desiredSpeed = 0f;
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
		var moveVector = Rotation.From( angles ) * movement * m_desiredSpeed;
		var groundEntity = CheckForGround();
		
		Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 800f, 7.5f );
		Entity.Velocity = ApplyFriction( Entity.Velocity, 4.0f );

		if ( Input.Pressed( "jump" ) && CanJump())
		{
			DoJump();
		}

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity );

		if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
		{
			mh.Position = StayOnGround( mh.Position );
			
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}

		Entity.GroundEntity = groundEntity;
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
		m_desiredSpeed = JogSpeed;
	}
	
	
	//Sprint state
	private void OnEnterSprintState()
	{
		m_desiredSpeed = SprintSpeed;
	}

	//Walk state
	private void OnEnterWalkState()
	{
		m_desiredSpeed = WalkSpeed; 
	}
	
	//Duck state
	private void OnEnterDuckState()
	{
		m_desiredSpeed = DuckSpeed;
		Entity.EventDispatcher.SendEvent<EventOnDuck>();
		Coroutine.Stop( m_duckCoroutine );
		m_duckCoroutine = ResizeDuckHull( Entity.DuckHull );
		Coroutine.Start( m_duckCoroutine);
	}

	private CoroutineEnumerator m_duckCoroutine;
	
	CoroutineEnumerator ResizeDuckHull(BBox to)
	{
		var curHull = Entity.Hull;
		float duckLerp = 0f;
		
		while ( !Entity.Hull.Size.AlmostEqual( to.Size ) )
		{
			Entity.Hull = curHull.Lerp( to, duckLerp += (Time.Delta * 10f) );
			yield return new WaitForNextTick();
		}
	}
	
	private void OnExitDuckState()
	{
		Coroutine.Stop( m_duckCoroutine );
		m_duckCoroutine = ResizeDuckHull( Entity.DefaultHull );
		Coroutine.Start( m_duckCoroutine);
		
		Entity.EventDispatcher.SendEvent<EventOnUnDuck>();
	}
}


