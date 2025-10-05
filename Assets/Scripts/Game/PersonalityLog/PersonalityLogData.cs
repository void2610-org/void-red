using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.PersonalityLog
{
    /// <summary>
    /// 全体の人格ログデータ（セーブ・ロード対象）
    /// </summary>
    [Serializable]
    public class PersonalityLogData
    {
        [SerializeField] public List<ChapterLog> chapters = new();
        
        private ChapterLog _currentChapter;
        
        public PersonalityLogData()
        {
            chapters = new List<ChapterLog>();
            _currentChapter = null;
        }
        
        public void LoadFrom(PersonalityLogData other)
        {
            chapters = new List<ChapterLog>(other.chapters);
            _currentChapter = other._currentChapter;
        }
        
        /// <summary>
        /// 新しいチャプターを開始
        /// </summary>
        public ChapterLog StartNewChapter(EnemyData enemyData)
        {
            _currentChapter = new ChapterLog(enemyData);
            chapters.Add(_currentChapter);
            return _currentChapter;
        }
        
        /// <summary>
        /// 現在のチャプターを取得
        /// </summary>
        public ChapterLog GetCurrentChapter()
        {
            return _currentChapter;
        }

        /// <summary>
        /// リセット
        /// </summary>
        public void Reset()
        {
            chapters.Clear();
            _currentChapter = null;
        }
    }
}