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
        [SerializeField] private string timestamp;
        
        protected TurnEvent()
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
    
    /// <summary>
    /// 共鳴発生イベント
    /// </summary>
    [Serializable]
    public class ResonanceEvent : TurnEvent
    {
        [SerializeField] private CardData resonanceCard;
        
        public ResonanceEvent(CardData resonanceCard)
        {
            this.resonanceCard = resonanceCard;
        }
    }
    
    /// <summary>
    /// カード進化イベント
    /// </summary>
    [Serializable]
    public class CardEvolutionEvent : TurnEvent
    {
        [SerializeField] private string actorId;
        [SerializeField] private CardData fromCard;
        [SerializeField] private CardData toCard;
        
        public CardEvolutionEvent(string actorId, CardData fromCard, CardData toCard)
        {
            this.actorId = actorId;
            this.fromCard = fromCard;
            this.toCard = toCard;
        }
    }
    
    /// <summary>
    /// カード崩壊イベント
    /// </summary>
    [Serializable]
    public class CardCollapseEvent : TurnEvent
    {
        [SerializeField] private string actorId;
        [SerializeField] private CardData collapseCard;
        
        public CardCollapseEvent(string actorId, CardData collapseCard)
        {
            this.actorId = actorId;
            this.collapseCard = collapseCard;
        }
    }
}