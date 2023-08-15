namespace Sandbox.pawn.pawncontrollers.abilities;

public interface IAbility
{
	void OnActivate(Pawn pawn);
	void OnDeactivate();
}

public interface ISimulable
{
	void Simulate(IClient client);
}
