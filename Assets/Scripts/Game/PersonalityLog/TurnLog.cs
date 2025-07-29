using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.PersonalityLog
{
    /// <summary>
    /// 1ターンの全ログ情報
    /// </summary>
    [Serializable]
    public class TurnLog
    {
        [SerializeField] public MoveLog playerMove;
        [SerializeField] public MoveLog enemyMove;
        [SerializeField, SerializeReference] public List<TurnEvent> events;
        
        public TurnLog(MoveLog playerMove, MoveLog enemyMove, List<TurnEvent> events)
        {
            this.playerMove = playerMove;
            this.enemyMove = enemyMove;
            this.events = events ?? new List<TurnEvent>();
        }
    }
}