using UnityEngine;

[CreateAssetMenu(fileName = "NewMatchSettings", menuName = "Chess/Match Settings")]
public class MatchConfig : ScriptableObject
{
    [Header("Modo de Juego")]
    public bool isVsComputer = true;

    [Header("Color del Jugador")]
    public bool isPlayingWhite = true;
}