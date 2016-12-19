using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Publisher.Models;

namespace Publisher.Controllers
{
    /// <summary>
    /// Publishコントロール
    /// ※RouteConfigがデフォルトで表示する画面のコントロール
    /// PublishとBackup管理は機能が一緒のため、同じコントロールとする。
    /// </summary>
    public class PublishController : Controller
    {
        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Publish/Indexの初期表示
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult Index()
        {
            // トークンの比較(F5押下による呼び出しか確認)
            PublishModel publishModel = new PublishModel();

            return View(SetToken(publishModel));
        }

        /// <summary>
        /// Publish/Backupの初期表示
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult Backup()
        {
            // トークンの比較(F5押下による呼び出しか確認)
            PublishModel publishModel = new PublishModel();

            return View(SetToken(publishModel));
        }

        /// <summary>
        /// Publish/IndexのSubmit処理(Publisherボタン押下後の処理)
        /// </summary>
        /// <param name="model">Publisherモデル</param>
        /// <param name="productDivision">商材区分</param>
        /// <param name="guid">guid(トークン用)</param>
        /// <returns>ActionResult</returns>
        [STAThread]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(PublishModel model, string productDivision, string guid)
        {
            // トークンの比較(F5押下による呼び出しか確認)
            if (guid != (string)TempData["guid"])
            {
                // F5押下の場合は初期表示に戻す。
                return RedirectToAction("Index");
            }

            // 各プロパティに指定したバリデーションのチェックを行う。
            // チェックに反する場合は、画面に戻りエラーを返す。
            if (ModelState.IsValid)
            {
                model = PublishControl(model, guid, productDivision, null);
            }

            // 問題が発生した場合はフォームを再表示する
            // トークン処理(F5防止のため)
            return View(SetToken(model));
        }

        /// <summary>
        /// Publish/BackupのSubmit処理(Publisherボタン押下後の処理)
        /// </summary>
        /// <param name="model">Publisherモデル</param>
        /// <param name="productDivision">商材区分</param>
        /// <param name="guid">guid(トークン用)</param>
        /// <param name="backupDivision">バックアップ区分</param>
        /// <returns>ActionResult</returns>
        [STAThread]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Backup(PublishModel model, string productDivision, string guid, string backupDivision)
        {
            // トークンの比較(F5押下による呼び出しか確認)
            if (guid != (string)TempData["guid"])
            {
                // F5押下の場合は初期表示に戻す。
                return RedirectToAction("Backup");
            }

            // 各プロパティに指定したバリデーションのチェックを行う。
            // チェックに反する場合は、画面に戻りエラーを返す。
            if (ModelState.IsValid)
            {
                model = PublishControl(model, guid, productDivision, backupDivision);
            }

            // 問題が発生した場合はフォームを再表示する
            return View(SetToken(model));
        }

        /// <summary>
        /// トークン処理(F5防止のため)
        /// </summary>
        /// <param name="publishModel">publishModel to join</param>
        /// <returns>publishModel</returns>
        private PublishModel SetToken(PublishModel publishModel)
        {
            // GUID生成＆hiddenに格納
            string guid = Guid.NewGuid().ToString();
            publishModel.Guid = guid;
            ModelState.SetModelValue("Guid", new ValueProviderResult(guid, guid, null));

            // TempDataがない場合は追加、ある場合は値を上書き
            if (TempData["guid"] == null)
            {
                TempData.Add("guid", guid);
            }
            else
            {
                TempData["guid"] = guid;
            }

            return publishModel;
        }

        /// <summary>
        /// 発行コントロール
        /// </summary>
        /// <param name="publishModel">publishMode to join.</param>
        /// <param name="guid">guid</param>
        /// <param name="productDivision">商材区分</param>
        /// <param name="backupDivision">バックアップ区分(バックアップ管理画面用)</param>
        /// <returns>publishMode</returns>
        private PublishModel PublishControl(PublishModel publishModel, string guid, string productDivision, string backupDivision)
        {
            // 多重起動を禁止する。
            string mutexName = "publish";
            System.Threading.Mutex mutex = null;
            // 商材詳細
            EnvDetail envDetail = null;
            // メッセージ
            string message = string.Empty;

            try
            {
                // 固有のmutexNameを持つmutexを呼び出す。
                mutex = System.Threading.Mutex.OpenExisting(mutexName);
            }
            catch (System.Threading.WaitHandleCannotBeOpenedException ex)
            {
                // 現在起動しているものがない場合、Exception発生するのが当たり前なので、このまま流す。
            }

            // mutexがnullか確認する。
            if ((mutex == null))
            {
                // mutexを生成する。
                mutex = new System.Threading.Mutex(true, mutexName);
            }
            else
            {
                // mutexがnullではない場合、現在起動中のものがあるという意味なのでエラーを返す。
                Logger.Info("多重起動エラー：ただいま発行処理を実施しているため、この処理は中止します。guid：" + guid);
                ModelState.AddModelError(string.Empty, "ただいま発行処理を実施しています。時間をおいて再度お試しください。");
                return publishModel;
            }

            try
            {
                Logger.Info("### Start Publish ### -- guid：" + guid);

                // 入力フォームから渡された発行元と発行先の情報が正しいか確認(サーバー一覧から逆検索して確認)
                //EnvDetail serverDetail = null;
                foreach (EnvDetail server in PublishModel.ReadServerCsv())
                {
                    var serverValue = server.value.ToString().Split(',');
                    // 商材区分、発行元、発行先が一致するサーバー情報があるか確認
                    // バックアップからの発行の場合はFromPathは比較対象から外す
                    if (backupDivision == null &&
                        productDivision.Equals(serverValue[2]) && 
                        publishModel.FromPath.Equals(server.FromPath) && 
                        publishModel.ToPath.Equals(server.ToPath))
                    {
                        envDetail = server;
                        break;
                    }
                    else if (backupDivision != null &&
                        productDivision.Equals(serverValue[2]) &&
                        publishModel.ToPath.Equals(server.ToPath))
                    {
                        
                        envDetail = server;
                        break;
                    }
                }

                // 商材の情報がある場合、発行処理を行う
                if (envDetail != null)
                {
                    // 商材区分に商材名と発行先のhttpHeadをに表示する
                    envDetail.viewName = envDetail.httpHeader + "|" + envDetail.viewName;

                    Logger.Info("発行を開始します。商材区分：" + envDetail.viewName);

                    // バックアップ区分がある場合(=バックアップからの発行の場合)
                    if (backupDivision != null)
                    {
                        // 選択したバックアップ元を発行元とする。
                        envDetail.FromPath = backupDivision;

                        // バックアップ元のディレクトリが存在する場合、エラーを返す。
                        if (!Directory.Exists(backupDivision))
                        {
                            // バックアップ元のディレクトリが存在しない場合、エラーを返す。
                            message = "選択したバックアップディレクトリが存在しません。再度お試しください。";
                            Logger.Info(message);
                            ModelState.AddModelError(string.Empty, message);
                            return publishModel;
                        }
                    }

                    // 発行処理。PublishControlの処理結果がtrueの場合のみ、完了メッセージを返す。
                    // (PublishControlの結果がfalseの場合はエラー発生箇所で格納したエラーメッセージを表示する)

                    // 商材名
                    string productName = envDetail.viewName;
                    // 発行元(商材から選択した場合は【仮本番】になる)
                    string fromPath = envDetail.FromPath;
                    // 発行先(商材から選択した場合は【本番】になる)
                    string toPath = Common.GetDirectoryPath(envDetail.ToPath);

                    // 最新バックアップ(ロールバック時にはここでバックアップしたソースが元になる)
                    if (LatestBackup(productName, toPath))
                    {
                        // 発行
                        if (Publisher(productName, fromPath, toPath))
                        {
                            // バックアップ
                            if (Backup(productName, toPath))
                            {
                                // 発行元、発行先の情報を画面のフォームに返す
                                ModelState.SetModelValue("FromPath", new ValueProviderResult(envDetail.FromPath, envDetail.FromPath, null));
                                ModelState.SetModelValue("ToPath", new ValueProviderResult(envDetail.ToPath, envDetail.ToPath, null));

                                // 完了メッセージを返す
                                message = "発行が完了しました。商材区分：" + envDetail.viewName + "、発行元(From)：" + envDetail.FromPath + "、発行先(To)：" + envDetail.ToPath;
                                Logger.Info(message);
                                // メール送信
                                Common.SendMail(message, ": publish complete: " + envDetail.viewName);

                                ModelState.AddModelError(string.Empty, message);
                            }
                        }
                    }
                }
                else
                {
                    // 商材情報がない場合、エラーを返す。
                    message = "商材区分が正しくありません。";
                    Logger.Info(message);
                    ModelState.AddModelError(string.Empty, message);
                }

                Logger.Info("### End Publish ### -- guid：" + guid);
            }
            catch (Exception e)
            {
                message = "発行処理中にエラーが発生しました。";
                string subject = "";

                // 商材の情報がある場合、内容を表示する。
                if (envDetail != null)
                {
                    message += "商材区分：" + envDetail.viewName + "、発行元(From)：" + envDetail.FromPath + "、発行先(To)：" + envDetail.ToPath + "。";
                    subject = envDetail.viewName;
                }

                // メール送信
                Common.SendMail(message, ": catch error: " + subject);

                Logger.Info(message + "StackTrace:" + e.StackTrace + "、message : " + e.Message);
                ModelState.AddModelError(string.Empty, message);
            }
            finally
            {
                // mutexを開放する(次の起動ok)
                mutex.Close();
            }

            return publishModel;
        }

        /// <summary>
        /// 最新バックアップ処理
        /// ※最新バックアップ(発行直前の状態をバックアップ)は常に一つだけにする。
        /// ## 最新バックアップ先のフォーマット
        /// バックアップ先の基本パス\{発行先(toPath)の最後のディレクトリ名}\LatestBackup\
        /// </summary>
        /// <param name="productName">商材名</param>
        /// <param name="toPath">バックアップする発行先のパス</param>
        /// <returns>処理結果(true:正常/false:異常)</returns>
        private bool LatestBackup(string productName, string toPath)
        {
            // バックアップする発行先のパス
            string sourcePath = string.Empty;
            // バックアップ先のパス
            string targetPath = string.Empty;
            string message = string.Empty;

            try
            {
                // バックアップする発行先のパス
                sourcePath = toPath;

                // バックアップ先の基本パスを取得
                string backupDirectory = ConfigurationManager.AppSettings["BackupDirectory"];

                // 発行先(toPath)の最後のディレクトリ名 = 現在のインスタンスの名前を取得
                string toDirectory = new DirectoryInfo(sourcePath).Name;

                // バックアップ先のパス
                targetPath = backupDirectory + toDirectory + @"\" + "LatestBackup";

                // フォルダがある場合、フォルダ削除
                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, true);
                }

                // バックアップディレクトリを作成する
                DirectoryInfo directoryInfo = Directory.CreateDirectory(targetPath);

                // バックアップ実行 :: ファイル＋フォルダのコピー処理はxcopyコマンドを利用
                System.Diagnostics.Process tempBackupProcess = System.Diagnostics.Process.Start("xcopy.exe", string.Format("/s /e /y {0} {1}", sourcePath, targetPath));
                // バックアップが終了するまで待機
                tempBackupProcess.WaitForExit();

                // バックアップ終了後、ロールバックのため、バックアップしたディレクトリをセッションに保存する。
                Session["LatestBackupPath"] = targetPath;

                Logger.Info("最新バックアップ処理が完了しました。商材区分：" + productName);
            }
            catch (Exception e)
            {
                message = "発行前の最新バックアップ処理中にエラーが発生したため、処理を中止しました。商材区分：" + productName + "、バックアップ元：" + sourcePath + "、バックアップ先：" + targetPath + "。";

                // メール送信
                Common.SendMail(message, ": catch error: " + productName);

                Logger.Info(message + "Message : " + e.Message + "StackTrace : " + e.StackTrace);
                ModelState.AddModelError(string.Empty, message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// バックアップ処理
        /// 発行後に発行先のファイルを日時付けでバックアップする
        /// # バックアップ先のフォーマット
        /// バックアップ先の基本パス\{発行先(toPath)の最後のディレクトリ名}\{yyyyMMddHHmmss形式の日付}\
        /// </summary>
        /// <param name="productName">商材名</param>
        /// <param name="toPath">バックアップする発行先のパス</param>
        /// <returns>処理結果(true:正常/false:異常)</returns>
        private bool Backup(string productName, string toPath)
        {
            // バックアップする発行先のパス
            string sourcePath = string.Empty;
            // バックアップ先のパス
            string targetPath = string.Empty;
            string message = string.Empty;

            try
            {
                // バックアップする発行先のパス
                sourcePath = toPath;

                // バックアップ先の基本パスを取得
                string backupDirectory = ConfigurationManager.AppSettings["BackupDirectory"];

                // 発行先(toPath)の最後のディレクトリ名 = 現在のインスタンスの名前を取得
                var toPathInfo = new DirectoryInfo(toPath);
                string toDirectory = toPathInfo.Name;

                // バックアップ先のパス
                targetPath = backupDirectory + toDirectory + @"\" + DateTime.Now.ToString("yyyyMMddHHmmss");

                // フォルダがない場合、新規作成
                if (!Directory.Exists(targetPath))
                {
                    // バックアップディレクトリを作成する
                    DirectoryInfo directoryInfo = Directory.CreateDirectory(targetPath);
                }

                // バックアップ実行 :: ファイル＋フォルダのコピー処理はxcopyコマンドを利用
                System.Diagnostics.Process backupProcess = System.Diagnostics.Process.Start("xcopy.exe", string.Format("/s /e /y {0} {1}", sourcePath, targetPath));
                // バックアップが終了するまで待機
                backupProcess.WaitForExit();

                // 最近バックアップ先のパス
                string latestBackupPath = backupDirectory + toDirectory + @"\" + "LatestBackup";

                // フォルダがある場合、フォルダ削除
                if (Directory.Exists(latestBackupPath))
                {
                    Directory.Delete(latestBackupPath, true);
                }

                Logger.Info("バックアップ処理が完了しました。商材区分：" + productName);
            }
            catch (Exception e)
            {
                message = "発行後のバックアップ処理中にエラーが発生したため、処理を中止しました。商材区分：" + productName + "、バックアップ元：" + sourcePath + "、バックアップ先：" + targetPath + "。";

                // メール送信
                Common.SendMail(message, ": catch error: " + productName);

                Logger.Info(message + "Message : " + e.Message + "StackTrace : " + e.StackTrace);
                ModelState.AddModelError(string.Empty, message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// 発行処理
        /// </summary>
        /// <param name="productName">商材名</param>
        /// <param name="sourcePath">発行元</param>
        /// <param name="targetPath">発行先</param>
        /// <returns>処理結果(true:正常/false:異常)</returns>
        private bool Publisher(string productName, string sourcePath, string targetPath)
        {
            bool resultValue = true;

            string message = string.Empty;

            // 発行先フォルダがない場合、新規作成
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // 発行先にコピーするapp_offline.htmのパスを取得
            string appOffLineFile = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["AppOffLine"];
            string destFile = Path.Combine(targetPath, Path.GetFileName(appOffLineFile));

            // appOffLineFileのパスにファイルが存在するか確認
            if (System.IO.File.Exists(appOffLineFile))
            {
                try
                {
                    //app_offline.htmをコピー
                    System.IO.File.Copy(appOffLineFile, destFile, true);
                }
                catch (Exception e)
                {
                    message = "app_offline.htmをコピー中にエラーが発生したため、処理を中止しました。商材区分：" + productName + "、発行元(From) : " + sourcePath + "、発行先(To) : " + targetPath + "。";
                    Logger.Info(message + "Message : " + e.Message + "StackTrace : " + e.StackTrace);

                    // メール送信
                    Common.SendMail(message, ": catch error: " + productName);

                    ModelState.AddModelError(string.Empty, message);

                    return false;
                }
            }
            else
            {
                message = "発行先にコピーするapp_offline.htmが見当たりません。web.configのappSettingsの値を確認してください。";
                Logger.Info(message);

                // メール送信
                Common.SendMail(message, ": catch error: " + productName);
                ModelState.AddModelError(string.Empty, message);

                return false;
            }

            Logger.Info("発行先にapp_offline.htmをコピーしました。商材区分：" + productName);

            try
            {
                // 発行先からapp_offline.htm以外の全てのファイルを削除する。
                Common.DeleteDirectory(targetPath, destFile);

                Logger.Info("発行先のファイル削除が完了しました。商材区分：" + productName);
            }
            catch (Exception e)
            {
                message = "発行先のファイル削除中にエラーが発生したため、処理を中止しました。商材区分：" + productName + "、発行元(From) : " + sourcePath + "、発行先(To) : " + targetPath + "。";
                Logger.Info(message + "Message : " + e.Message + "StackTrace : " + e.StackTrace);

                // メール送信
                Common.SendMail(message, ": catch error: " + productName);
                ModelState.AddModelError(string.Empty, message);

                return false;
            }

            try
            {
                // 発行(発行元→発行先)
                System.Diagnostics.Process publisherProcess = System.Diagnostics.Process.Start("xcopy.exe", string.Format("/s /e /y {0} {1}", sourcePath, targetPath));
                // 発行が終了するまで待機
                publisherProcess.WaitForExit();

                // ログとメール送信
                message = "発行処理が完了しました。商材区分：" + productName + "、発行元(From)：" + sourcePath + "、発行先(To)：" + targetPath;
                Logger.Info(message);
            }
            catch (Exception e)
            {
                // 発行処理時に例外エラーが発生した場合、ロールバックする。
                message = "発行処理中にエラーが発生したため、処理を中止しました。商材区分：" + productName;
                Logger.Info(message + "、Message : " + e.Message + "StackTrace : " + e.StackTrace);
                // メール送信
                Common.SendMail(message, ": catch error: " + productName);
                ModelState.AddModelError(string.Empty, message);

                // ロールバック(発行元(From)→発行先(To)へ発行中にエラーが発生した場合、バックアップした内容をToに上書きする)

                // セッションから最新バックアップディレクトリを取得
                string latestBackupPath = (string)Session["LatestBackupPath"];

                Logger.Info("ロールバックを開始します。ロールバックソース：" + latestBackupPath);

                // 発行先からapp_offline.htm以外の全てのファイルを削除する。
                Common.DeleteDirectory(targetPath, destFile);

                // ロールバック実行(最新バックアップ → 発行先)
                System.Diagnostics.Process publisherProcess = System.Diagnostics.Process.Start("xcopy.exe", string.Format("/s /e /y {0} {1}", latestBackupPath, targetPath));
                // ロールバックが終了するまで待機
                publisherProcess.WaitForExit();

                // ロールバック完了メッセージを設定
                message = "発行処理中にエラーが発生したため、ロールバックしました。商材区分：" + productName + "、発行元(From) : " + sourcePath + "、発行先(To) : " + targetPath + "、ロールバックソース : " + latestBackupPath;
                Logger.Info(message);
                // メール送信
                Common.SendMail(message, ": rollback end : " + productName);
                ModelState.AddModelError(string.Empty, message);

                return false;
            }
            finally
            {
                // 発行、若しくはロールバック後、app_offline.htmを削除する
                if (System.IO.File.Exists(destFile))
                {
                    try
                    {
                        //app_offline.htmを削除
                        System.IO.File.Delete(destFile);
                    }
                    catch (Exception e)
                    {
                        message = "app_offline.htmを削除中にエラーが発生したため、処理を中止しました。商材区分：" + productName + "、発行元(From) : " + sourcePath + "、発行先(To) ：" + targetPath + "。";
                        Logger.Info(message + "Message : " + e.Message + "StackTrace : " + e.StackTrace);

                        // メール送信
                        Common.SendMail(message, ": catch error: " + productName);

                        ModelState.AddModelError(string.Empty, message);

                        // finally内ではreturnすることが出来ないため、フラグに値を持たせて全ての処理が終わったらfalseを返す
                        resultValue = false;
                    }
                }
                Logger.Info("発行先のapp_offline.htmファイルを削除しました。商材区分：" + productName);
            }

            return resultValue;
        }
    }
}