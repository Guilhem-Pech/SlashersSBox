using Sandbox.Utils;

namespace Sandbox.pawn.PawnControllers;

public partial class MovementsController
{
	private HFSM<States, TransitionEvents>? m_hfsm;
	public string DebugCurrentState => m_hfsm != null ? m_hfsm.GetDebugCurrentStateName() : string.Empty;

	private void BuildHFSM()
	{
		HFSMBuilder<States, TransitionEvents> builder = new HFSMBuilder<States, TransitionEvents>();
		builder.AddState( States.OnGroundState , OnEnterGroundState, OnUpdateGroundState, OnExitGroundState);
		{
			builder.AddState( States.JogState ,States.OnGroundState, OnEnterJogState);
			{
				builder.AddTransition( States.JogState, States.RunState , () => Input.Down( "run" ) && CurrentStamina.CurrentStamina > 0);
				builder.AddTransition( States.JogState, States.WalkState , () => Input.Down( "walk" ));
				builder.AddTransition( States.JogState, States.DuckState , () => Input.Down( "duck" ));
			}
			builder.AddState( States.RunState ,States.OnGroundState, OnEnterSprintState, OnUpdateSprintState, OnExitSprintState);
			{
				builder.AddTransition( States.RunState, States.JogState , () => CurrentStamina.CurrentStamina <= 0);
				builder.AddTransition( States.RunState, States.JogState , () => !Input.Down( "run" ));
				builder.AddTransition( States.RunState, States.WalkState , () => Input.Down( "walk" ));
				builder.AddTransition( States.RunState, States.DuckState , () => Input.Down( "duck" ));
			}
			builder.AddState( States.WalkState ,States.OnGroundState, OnEnterWalkState);
			{
				builder.AddTransition( States.WalkState, States.JogState , () => !Input.Down( "walk" ));
				builder.AddTransition( States.WalkState, States.DuckState , () => Input.Down( "duck" ));
			}
			builder.AddState( States.DuckState, States.OnGroundState, OnEnterDuckState, null , OnExitDuckState);
			{
				builder.AddTransition( States.DuckState, States.JogState , () => !Input.Down( "duck" ) && CanUnduck());
			}
			
			builder.AddTransition( States.OnGroundState, States.SwimmingState , () => IsSwimming);
			builder.AddTransition( States.OnGroundState, States.InAirState , () => !Grounded);
		}
		builder.AddState( States.InAirState , OnEnterAirState, OnUpdateAirState, OnExitAirState);
		{
			builder.AddTransition( States.InAirState, States.DuckState , () => Input.Down( "duck" ) && Grounded ); 
			builder.AddTransition( States.InAirState, States.SwimmingState , () => IsSwimming);
			builder.AddTransition( States.InAirState, States.OnGroundState , () => Grounded );
		}
		builder.AddState( States.SwimmingState , OnEnterSwimState, OnUpdateSwimState, OnExitSwimState);
		{
			builder.AddTransition( States.SwimmingState, States.InAirState , () => !IsSwimming && !Grounded );
			builder.AddTransition( States.SwimmingState, States.OnGroundState , () => !IsSwimming );
		}
		m_hfsm = builder.Build();
	}
}

enum States
{
	OnGroundState,
	JogState,
	RunState,
	InAirState,
	WalkState,
	DuckState,
	SwimmingState
}

enum TransitionEvents
{}
