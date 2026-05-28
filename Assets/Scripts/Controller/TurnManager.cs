using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public TeamColor currentTurn { get; private set; } = TeamColor.White;

    public event Action<TeamColor> OnTurnChanged;

    public void StartMatch()
    {
        currentTurn = TeamColor.White;
        OnTurnChanged?.Invoke(currentTurn);
    }

    public void PassTurn()
    {
        currentTurn = (currentTurn == TeamColor.White) ? TeamColor.Black : TeamColor.White;

        Debug.Log($"[TurnManager] Cambio de turno. Ahora juegan: {currentTurn}");
        OnTurnChanged?.Invoke(currentTurn);
    }
}