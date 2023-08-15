namespace Sandbox.pawn.pawncontrollers.abilities.survivors;

public class MaxStaminaModifier : IAbility
{
	/// <summary>
	/// Stamina modifier ability
	/// </summary>
	/// <param name="modifier">a factor of the default MaxStamina defined in the Movement Controller. ex: 1.5 * MaxStamina or 0.5 * MaxStamina</param>
	public MaxStaminaModifier( float modifier )
	{
		Modifier = modifier;
	}
	private Pawn? Pawn { get; set; }
	public float Modifier { get; private set; }

	private float m_oldValue = 0f;
	public void OnActivate( Pawn pawn )
	{
		Pawn = pawn;
		if ( Pawn.MovementsController == null )
			return;

		m_oldValue = Pawn.MovementsController.CurrentStamina.MaxStaminaValue;
		Pawn?.MovementsController?.OverrideMaxStamina( m_oldValue * Modifier );
	}
	public void OnDeactivate()
	{
		Pawn?.MovementsController?.OverrideMaxStamina(m_oldValue);
	}
}
