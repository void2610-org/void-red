using System;
using UnityEngine;

namespace Game.PersonalityLog
{
    /// <summary>
    /// ターン中に発生するイベントの基底クラス
    /// </summary>
    [Serializable]
    public abstract class TurnEvent
    {
        [SerializeField] public string actorId;
        
        protected TurnEvent(string actorId)
        {
            this.actorId = actorId;
        }
    }
    
    /// <summary>
    /// 共鳴発生イベント
    /// </summary>
    [Serializable]
    public class ResonanceEvent : TurnEvent
    {
        [SerializeField] public CardData resonanceCard;
        
        public ResonanceEvent(string actorId, CardData resonanceCard) : base(actorId)
        {
            this.resonanceCard = resonanceCard;
        }
    }

    /// <summary>
    /// カード崩壊イベント
    /// </summary>
    [Serializable]
    public class CardCollapseEvent : TurnEvent
    {
        [SerializeField] public CardData collapseCard;
        
        public CardCollapseEvent(string actorId, CardData collapseCard) : base(actorId)
        {
            this.collapseCard = collapseCard;
        }
    }
}