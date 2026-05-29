using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public TeamColor currentTurn { get; private set; } = TeamColor.White;

    // NUEVO: Candado del final de partida
    public bool isGameOver { get; private set; } = false;

    public event Action<TeamColor> OnTurnChanged;

    // NUEVOS EVENTOS PARA LA UI
    public event Action<TeamColor> OnCheckmate; // Avisa de quién ha ganado
    public event Action OnStalemate; // Avisa de empate

    public void StartMatch()
    {
        isGameOver = false;
        currentTurn = TeamColor.White;
        OnTurnChanged?.Invoke(currentTurn);
    }

    public void PassTurn()
    {
        currentTurn = (currentTurn == TeamColor.White) ? TeamColor.Black : TeamColor.White;
        Debug.Log($"[TurnManager] Cambio de turno. Ahora juegan: {currentTurn}");
        OnTurnChanged?.Invoke(currentTurn);
    }

    // NUEVO: Métodos de victoria
    public void DeclareCheckmate(TeamColor winner)
    {
        isGameOver = true;
        Debug.Log($"¡JAQUE MATE! Han ganado las {winner}");
        OnCheckmate?.Invoke(winner);
    }

    public void DeclareStalemate()
    {
        isGameOver = true;
        Debug.Log("¡REY AHOGADO! La partida termina en Empate.");
        OnStalemate?.Invoke();
    }
}