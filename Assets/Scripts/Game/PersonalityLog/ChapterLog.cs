using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.PersonalityLog
{
    /// <summary>
    /// 1つのチャプター（1体の敵との全バトル）の全ログ情報
    /// </summary>
    [Serializable]
    public class ChapterLog
    {
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private List<TurnLog> turns = new();
        
        public ChapterLog(EnemyData enemyData)
        {
            this.enemyData = enemyData;
        }
        
        /// <summary>
        /// ターンを追加
        /// </summary>
        public void AddTurn(TurnLog turnLog)
        {
            turns.Add(turnLog);
        }
        
        /// <summary>
        /// チャプターを完了
        /// </summary>
        public void CompleteChapter()
        {
            // 空実装 - セーブトリガー用
        }
    }
}