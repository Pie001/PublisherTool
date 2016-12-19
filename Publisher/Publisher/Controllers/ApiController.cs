using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Publisher.Models;

namespace Publisher.Controllers
{
    public class ApiController : Controller
    {
        //
        // GET: /Api/

        // GET /Api/ProductListSelect/?selectValue=<selectValue>

        /// <summary>
        /// select値から商材の仮本番＆本番のパスをjsonで返す
        /// </summary>
        /// <param name="selectValue">selectValue</param>
        /// <returns>json(商材詳細)</returns>
        [HttpGet]
        public JsonResult ProductListSelect(string selectValue)
        {
            List<EnvDetail> envPathList = null;

            // 0以外を選択した場合、商材のパスを取得する
            if (selectValue != "0")
            {
                envPathList = PublishModel.GetServerList(selectValue);
            }

            // APIとして下記を設定してJsonで返す
            // JsonRequestBehavior.AllowGet > クライアントからの HTTP GET 要求を許可します。
            return Json(envPathList, JsonRequestBehavior.AllowGet);
        }

        // GET /Api/BackupSelect/?toPath=<toPath>

        /// <summary>
        /// select値から取得した商材のバックアップパスの一覧をjsonで返す
        /// </summary>
        /// <param name="toPath">toPath</param>
        /// <returns>json(商材詳細)</returns>
        [HttpGet]
        public JsonResult BackupSelect(string toPath)
        {
            List<EnvDetail> envPathList = PublishModel.ReadServerCsv();

            EnvDetail envPath = envPathList.Find(r => r.ToPath == toPath);

            List<BackupPathDetail> backupPathList = null;

            // selectIndexに当てはまる商材の情報がない場合、nullを返す
            if (envPath != null)
            {
                string backupDirectory = ConfigurationManager.AppSettings["BackupDirectory"];

                // 発行先(toPath)の最後のディレクトリ名 = 現在のインスタンスの名前を取得
                string toDirectory = new DirectoryInfo(envPath.ToPath).Name;

                // バックアップ先のパス
                string backupPath = backupDirectory + toDirectory;

                // ディレクトリが存在する場合、ディレクトリ一覧を取得する
                if (Directory.Exists(backupPath))
                {
                    backupPathList = new List<BackupPathDetail>();

                    DirectoryInfo backupDir = new DirectoryInfo(backupPath);

                    int count = 1;
                    foreach (DirectoryInfo dir in backupDir.GetDirectories())
                    {
                        // ディレクトリ名が"LatestBackup"(ロールバック用の臨時バックアップ)の場合、表示しない
                        if (dir.Name != "LatestBackup")
                        {
                            BackupPathDetail bp = new BackupPathDetail();
                            bp.viewName = dir.Name;
                            bp.value = Convert.ToString(count);
                            bp.BackupPath = backupPath + @"\" + dir.Name;
                            bp.CreationTime = dir.CreationTime;
                            backupPathList.Add(bp);
                            count++;
                        }
                    }

                    // フォルダの作成日付を基準にソート
                    backupPathList.Sort(CreationTimeComparer);
                }
            }
            // APIとして下記を設定してJsonで返す
            // JsonRequestBehavior.AllowGet > クライアントからの HTTP GET 要求を許可します。
            return Json(backupPathList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 日時の降順ソート用メソッド
        /// (新しいバックアップが上位にくるようにする)
        /// </summary>
        /// <param name="x">The parameter is not userd. 作成日付</param>
        /// <param name="y">作成日付</param>
        /// <returns>比較結果</returns>
        private static int CreationTimeComparer(BackupPathDetail x, BackupPathDetail y)
        {
            return y.CreationTime.CompareTo(x.CreationTime);
        }
    }
}
