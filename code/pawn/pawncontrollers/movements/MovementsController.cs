using Sandbox.pawn.PawnControllers.mechanics;
using Sandbox.Utils;

namespace Sandbox.pawn.PawnControllers;

public partial class MovementsController : EntityComponent<Pawn>, ISingletonComponent, IComponentSimulable, IEventListener
{
	public float JogSpeed => 150f;
	public float SprintSpeed => 275f;
	public float WalkSpeed => 80f;
	public float DuckSpeed => 80f;
	public int StepSize => 24;
	public int GroundAngle => 45;
	public int JumpSpeed => 300;
	public float Gravity => 800f;
	[Net, Predicted, Local] public StaminaHandler CurrentStamina { get; private set; } = new StaminaHandler( 5f );

	public float SqrCurrentSpeed => Entity.Velocity.LengthSquared;

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
		m_hfsm?.Update();
	}
	
	[GameEvent.Client.BuildInput]
	public void BuildInput()
	{
		Entity.InputDirection = Input.AnalogMove;
	}

	Entity? CheckForGround()
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
	
	bool CanUnduck()
	{
		var hit = Entity.TraceBBox( Entity.Position, Entity.Position , Entity.DefaultHull.Mins, Entity.DefaultHull.Maxs);
		return !hit.StartedSolid; 
	}

	bool CanJump()
	{
		if ( Grounded )
		{
			var hit = Entity.TraceBBox( Entity.Position, Entity.Position , Entity.DefaultHull.Mins, Entity.DefaultHull.Maxs);
			return !hit.StartedSolid;
		}
		return false;
	}
}
