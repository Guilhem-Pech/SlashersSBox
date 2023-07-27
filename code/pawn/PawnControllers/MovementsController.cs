using System;
using System.Collections.Generic;
using MyGame;

namespace Sandbox.pawn.PawnControllers;

public partial class MovementsController : EntityComponent<Pawn>
{
	public float JogSpeed { get; set; } = 150f;
	public float SprintSpeed { get; set; } = 275f;
	public int StepSize => 24;
	public int GroundAngle => 45;
	public int JumpSpeed => 300;
	public float Gravity => 800f;

	HashSet<string> ControllerEvents = new( StringComparer.OrdinalIgnoreCase );

	bool Grounded => Entity.GroundEntity.IsValid();

	protected override void OnActivate()
	{
		BuildHFSM();
		base.OnActivate();
	}

	protected override void OnDeactivate()
	{
		m_hfsm?.Stop();
	}

	public void Simulate( IClient cl )
	{
		ControllerEvents.Clear();
		m_hfsm.Update();
	}
	
	[GameEvent.Client.BuildInput]
	public void BuildInput()
	{
		Entity.InputDirection = Input.AnalogMove;
	}

	Entity CheckForGround()
	{
		if ( Entity.Velocity.z > 100f )
			return null;

		var trace = Entity.TraceBBox( Entity.Position, Entity.Position + Vector3.Down, 2f );

		if ( !trace.Hit )
			return null;

		if ( trace.Normal.Angle( Vector3.Up ) > GroundAngle )
			return null;

		return trace.Entity;
	}

	public bool HasEvent( string eventName )
	{
		return ControllerEvents.Contains( eventName );
	}

	void AddEvent( string eventName )
	{
		if ( HasEvent( eventName ) )
			return;

		ControllerEvents.Add( eventName );
	}
}
