namespace Publisher.Models
{
    /// <summary>
    ///  商材情報コンテナ
    /// </summary>
    public class EnvDetail
    {
        /// <summary>
        /// プルダウン表示名(商材区分名)
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
        /// 商材のHttpHeader
        /// </summary>
        public string httpHeader
        {
            get;
            set;
        }

        /// <summary>
        /// 商材の発行元(仮本番、若しくはバックアップフォルダ)stagingEnvPath
        /// </summary>
        public string FromPath
        {
            get;
            set;
        }

        /// <summary>
        /// 商材の発行先(基本的には本番)productionEnvPath
        /// </summary>
        public string ToPath
        {
            get;
            set;
        }
    }
}