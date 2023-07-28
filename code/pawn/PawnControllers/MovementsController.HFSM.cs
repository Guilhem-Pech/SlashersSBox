using Sandbox.Utils;

namespace Sandbox.pawn.PawnControllers;

public partial class MovementsController
{
	private HFSM<States,TransitionEvents> m_hfsm;
	public string DebugCurrentState => m_hfsm != null ? m_hfsm.GetDebugCurrentStateName() : string.Empty;

	private void BuildHFSM()
	{
		HFSMBuilder<States, TransitionEvents> builder = new HFSMBuilder<States, TransitionEvents>();
		builder.AddState( States.OnGroundState , OnEnterGroundState, OnUpdateGroundState, OnExitGroundState);
		{
			builder.AddState( States.JogState ,States.OnGroundState, OnEnterJogState);
			{
				builder.AddTransition( States.JogState, States.RunState , () => Input.Down( "run" ));
				builder.AddTransition( States.JogState, States.WalkState , () => Input.Down( "walk" ));
			}
			builder.AddState( States.RunState ,States.OnGroundState, OnEnterSprintState);
			{
				builder.AddTransition( States.RunState, States.JogState , () => !Input.Down( "run" ));
				builder.AddTransition( States.RunState, States.WalkState , () => Input.Down( "walk" ));
			}
			builder.AddState( States.WalkState ,States.OnGroundState, OnEnterWalkState);
			{
				builder.AddTransition( States.WalkState, States.JogState , () => !Input.Down( "walk" ));
			}
			builder.AddTransition( States.OnGroundState, States.InAirState , () => !Grounded);
		}
		builder.AddState( States.InAirState , OnEnterAirState, OnUpdateAirState, OnExitAirState);
		{
			builder.AddTransition( States.InAirState, States.OnGroundState , () => Grounded );
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
	WalkState
}

enum TransitionEvents
{
	TouchedGround,
	LeftGround
}
