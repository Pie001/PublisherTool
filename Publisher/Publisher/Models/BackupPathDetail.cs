using System;

namespace Publisher.Models
{
    /// <summary>
    ///  バックアップ情報コンテナ
    /// </summary>
    public class BackupPathDetail
    {
        /// <summary>
        /// プルダウン表示名(バックアップディレクトリ名)
        /// </summary>
        public string viewName
        {
            get;
            set;
        }

        /// <summary>
        /// プルダウンの値
        /// </summary>
        public string value
        {
            get;
            set;
        }

        /// <summary>
        /// バックアップのパス
        /// </summary>
        public string BackupPath
        {
            get;
            set;
        }

        /// <summary>
        /// バックアップの作成日時
        /// </summary>
        public DateTime CreationTime
        {
            get;
            set;
        }
    }
}