using MyGame;

namespace Sandbox.pawn.PawnControllers;

public class CameraController : EntityComponent<Pawn>
{
	bool IsThirdPerson { get; set; } = false;
	
	public void Simulate( IClient cl )
	{
		SimulateRotation();
	}

	public void BuildInput()
	{
		Entity.InputDirection = Input.AnalogMove;

		if ( Input.StopProcessing )
			return;

		var look = Input.AnalogLook;

		if ( Entity.ViewAngles.pitch > 90f || Entity.ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = Entity.ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		Entity.ViewAngles = viewAngles.Normal;
	}

	public void FrameSimulate( IClient cl )
	{
		SimulateRotation();

		Camera.Rotation = Entity.ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		if ( Input.Pressed( "view" ) )
		{
			IsThirdPerson = !IsThirdPerson;
		}
		
		if ( IsThirdPerson )
		{
			Vector3 targetPos;
			var pos = Entity.Position + Vector3.Up * 64;
			var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			float distance = 80.0f * Entity.Scale;
			targetPos = pos + rot.Right * ((Entity.CollisionBounds.Mins.x + 50) * Entity.Scale);
			targetPos += rot.Forward * -distance;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.Ignore( Entity )
				.Radius( 8 )
				.Run();
			
			Camera.FirstPersonViewer = null;
			Camera.Position = tr.EndPosition;
		}
		else
		{
			Camera.FirstPersonViewer = Entity;
			Camera.Position = Entity.EyePosition;
		}
	}

	private void SimulateRotation()
	{
		Entity.EyeRotation = Entity.ViewAngles.ToRotation();
		Entity.Rotation = Entity.ViewAngles.WithPitch( 0f ).ToRotation();
	}
}
