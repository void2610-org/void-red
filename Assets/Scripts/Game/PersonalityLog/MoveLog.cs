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
        [SerializeField] public CardData card;
        [SerializeField] public PlayStyle playStyle;
        [SerializeField] public int mentalBet;
        [SerializeField] public int currentMentalPower;
        
        public MoveLog(PlayerMove move, int currentMentalPower)
        {
            this.card = move.SelectedCard;
            this.playStyle = move.PlayStyle;
            this.mentalBet = move.MentalBet;
            this.currentMentalPower = currentMentalPower;
        }
    }
}