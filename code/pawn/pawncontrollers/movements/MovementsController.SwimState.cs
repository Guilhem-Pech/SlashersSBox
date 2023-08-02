 namespace Sandbox.pawn.PawnControllers;

public partial class MovementsController
{
	private bool IsSwimming => Entity.GetWaterLevel() > 0.6f; //TODO Need to redo this method
	private void OnEnterSwimState()
	{
		Entity.EventDispatcher.SendEvent<EventOnStartSwimming>();
		Entity.EventDispatcher.RegisterEvent<EventOnRespawn>(this, OnRespawn); 
	}

	private void OnRespawn( EventOnRespawn obj )
	{
		Entity.ClearWaterLevel();
	}
	private void OnUpdateSwimState()
	{
		var movement = Entity.InputDirection.Normal;
		var angles = Entity.ViewAngles.WithPitch( 0 );
		var moveVector = Rotation.From( angles ) * movement * 320f;
		
		Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, moveVector.Length, 100, 20f );
		Entity.Velocity += Vector3.Down * Gravity * Time.Delta;

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity );

		if ( mh.TryUnstuck() )
		{
			Entity.Position = mh.Position;
		}
		if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
		{
			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}
	}
	private void OnExitSwimState()
	{
		Entity.EventDispatcher.SendEvent<EventOnEndSwimming>();
		Entity.EventDispatcher.UnregisterEvent<EventOnRespawn>(this);
	}
}
public class EventOnStartSwimming : Utils.Event {}
public class EventOnEndSwimming : Utils.Event {}
