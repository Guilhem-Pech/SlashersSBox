namespace Sandbox.pawn.PawnControllers;

public partial class MovementsController
{
	private void OnEnterAirState()
	{
	}

	private void OnUpdateAirState()
	{
		var movement = Entity.InputDirection.Normal;
		var angles = Entity.ViewAngles.WithPitch( 0 );
		var moveVector = Rotation.From( angles ) * movement * 320f;
		var groundEntity = CheckForGround();
		
		Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 100, 20f );
		Entity.Velocity += Vector3.Down * Gravity * Time.Delta;

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity );

		if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
		{
			if ( Grounded )
			{
				mh.Position = StayOnGround( mh.Position );
			}
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}

		Entity.GroundEntity = groundEntity;
	}

	private void OnExitAirState()
	{
	}
}
