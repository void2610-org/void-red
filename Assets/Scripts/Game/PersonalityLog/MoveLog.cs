using System;
using UnityEngine;

namespace Game.PersonalityLog
{
    /// <summary>
    /// プレイヤーまたは敵の手（ムーブ）のログ
    /// </summary>
    [Serializable]
    public class MoveLog
    {
        [SerializeField] private CardData selectedCard;
        [SerializeField] private PlayStyle playStyle;
        [SerializeField] private int mentalBet;
        [SerializeField] private int currentMentalPower;
        
        public MoveLog(PlayerMove move, int currentMentalPower)
        {
            this.selectedCard = move.SelectedCard;
            this.playStyle = move.PlayStyle;
            this.mentalBet = move.MentalBet;
            this.currentMentalPower = currentMentalPower;
        }
    }
}