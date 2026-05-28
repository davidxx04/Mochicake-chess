using System; // Necesario para los Eventos (Action)
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    // Cualquiera puede LEER de quién es el turno, pero solo esta clase puede MODIFICARLO (private set)
    public TeamColor currentTurn { get; private set; } = TeamColor.White;

    //// El megáfono: Avisará a la UI, a la IA y a quien haga falta cuando cambie el turno
    //public event Action<TeamColor> OnTurnChanged;

    //public void StartMatch()
    //{
    //    currentTurn = TeamColor.White;
    //    OnTurnChanged?.Invoke(currentTurn); // Lanzamos el evento inicial
    //}

    //public void PassTurn()
    //{
    //    currentTurn = (currentTurn == TeamColor.White) ? TeamColor.Black : TeamColor.White;

    //    Debug.Log($"[TurnManager] Cambio de turno. Ahora juegan: {currentTurn}");
    //    OnTurnChanged?.Invoke(currentTurn);
    //}
}