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
	private int StuckTries { get; set; } = 0;
	private int AttemptsPerTick { get; } = 20;

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
		if(CheckStuckAndFix())
			return;
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

		SurfaceFriction = trace.Surface.Friction;
		return trace.Entity;
	}
	
	private bool CheckStuckAndFix()
	{
		var result = Entity.TraceBBox( Entity.Position, Entity.Position );

		if ( !result.StartedSolid )
		{
			StuckTries = 0;
			return false;
		}

		if ( Game.IsClient ) return true;

		

		for ( int i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Entity.Position + Vector3.Random.Normal * (StuckTries / 2.0f);

			if ( i == 0 )
			{
				pos = Entity.Position + Vector3.Up * 5;
			}

			result = Entity.TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				Entity.Position = pos;
				return false;
			}
		}

		++StuckTries;
		return true;
	}
}
