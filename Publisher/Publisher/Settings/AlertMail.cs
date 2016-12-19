namespace Publisher.Settings
{
    /// <summary>
    /// アラートメール用設定
    /// </summary>
    public abstract class AlertMail
    {
        /// <summary>
        /// 送信先
        /// </summary>
        public const string From = "Publisher<publisher@test.com>";

        /// <summary>
        /// 開封確認要求
        /// </summary>
        public const string Receipt = "receipte@test.com";

        /// <summary>
        /// 件名
        /// </summary>
        public const string Subject = "Publisher: Information!";
    }
}