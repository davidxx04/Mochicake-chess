using UnityEngine;

[CreateAssetMenu(fileName = "MatchConfigData", menuName = "Ajedrez/Configuracion de Partida")]
public class MatchConfig : ScriptableObject
{
    [Header("Modo de Juego")]
    public bool isVsComputer = true;

    [Header("Color del Jugador")]
    public bool isPlayingWhite = true;
}