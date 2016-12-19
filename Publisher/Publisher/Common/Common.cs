using System.Configuration;
using System.IO;
using Publisher.Settings;

namespace Publisher
{
    /// <summary>
    /// 共通モジュール
    /// </summary>
    public class Common
    {
        /// <summary>
        /// 指定したディレクトリ配下のファイルを全て削除する(サブディレクトリを含む)
        /// 残したいファイルのパスを指定する場合、該当ファイルは削除しない
        /// </summary>
        /// <param name="directoryPath">削除するディレクトリ</param>
        /// <param name="exceptionFilePath">残したいファイル</param>
        public static void DeleteDirectory(string directoryPath, string exceptionFilePath)
        {
            // 削除ディレクトリ情報を取得
            System.IO.DirectoryInfo delDir = new System.IO.DirectoryInfo(directoryPath);

            // サブディレクトリ内も含めすべてのファイルを取得する
            System.IO.FileSystemInfo[] fileInfos = delDir.GetFileSystemInfos("*", System.IO.SearchOption.AllDirectories);

            // 例外ファイル以外のすべてのファイル、フォルダを削除
            foreach (System.IO.FileSystemInfo fileInfo in fileInfos)
            {
                // ディレクトリまたはファイルであるかを判断する
                if ((fileInfo.Attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                {
                    // ディレクトリが存在する場合、削除する
                    if (Directory.Exists(fileInfo.FullName))
                    {
                        System.IO.Directory.Delete(fileInfo.FullName, true);
                    }
                }
                else
                {
                    // ファイルの場合
                    // 例外ファイル以外の場合
                    if (fileInfo.FullName != exceptionFilePath)
                    {
                        // ファイルが存在する場合、削除する
                        if (System.IO.File.Exists(fileInfo.FullName))
                        {
                            System.IO.File.Delete(fileInfo.FullName);
                        }
                    }
                    else
                    {
                        string aa = fileInfo.FullName;
                    }
                }
            }
        }

        /// <summary>
        /// メール送信
        /// </summary>
        /// <param name="mailMessage">メール本文</param>
        /// <param name="subject">メール件名</param>
        /// <returns>処理結果(true:正常、false:異常)</returns>
        public static bool SendMail(string mailMessage, string subject)
        {
            //// アラートメールを送信する
            //WebMail webMail = new WebMail();
            //// serverEnvironment:サーバー環境は一つしかないため、Prodにする
            //// apiEnvironment：apiは参考してないが、本番の発行作業を行うため、1にする
            //// stmp設定はweb.configから取得
            //webMail.Init(ConfigurationManager.AppSettings["Smtp"], "Prod", 1);

            //// 設定ファイルの読み込み
            //if (webMail.ReadConfig() == false)
            //{
            //    return false;
            //}

            //webMail.FromName = AlertMail.From;
            //webMail.Receipt = AlertMail.Receipt;
            //webMail.Subject = AlertMail.Subject + subject;
            //// ("、"と"。"の間に改行を入れる)
            //webMail.Body = mailMessage.Replace("、", "\r\n").Replace("。", "。\r\n");

            //if (webMail.SendMail() == false)
            //{
            //    return false;
            //}

            return true;
        }

        /// <summary>
        /// パスの一番後尾に"\"を排除する
        /// (例)\\aaa\bbb\ ->\\aaa\bbb)
        /// (xcopy処理時にディレクトリとして認識できない場合がある)
        /// </summary>
        /// <param name="path">パスの文字列</param>
        /// <returns>"\"を排除したパスの文字列</returns>
        public static string GetDirectoryPath(string path)
        {
            DirectoryInfo toDirectory = new DirectoryInfo(path);

            return path.Substring(0, path.LastIndexOf(toDirectory.Name)) + toDirectory.Name;
        }
    }
}