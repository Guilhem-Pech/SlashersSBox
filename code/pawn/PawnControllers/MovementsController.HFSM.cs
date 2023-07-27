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
			}
			builder.AddState( States.RunState ,States.OnGroundState, OnEnterSprintState);
			{
				builder.AddTransition( States.RunState, States.JogState , () => !Input.Down( "run" ));
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
}

enum TransitionEvents
{
	TouchedGround,
	LeftGround
}
