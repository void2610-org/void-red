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
        [SerializeField] private MoveLog playerMove;
        [SerializeField] private MoveLog enemyMove;
        [SerializeField, SerializeReference] private List<TurnEvent> events;
        
        public MoveLog PlayerMove => playerMove;
        public MoveLog EnemyMove => enemyMove;
        
        public TurnLog(MoveLog playerMove, MoveLog enemyMove, List<TurnEvent> events)
        {
            this.playerMove = playerMove;
            this.enemyMove = enemyMove;
            this.events = events ?? new List<TurnEvent>();
        }
    }
}