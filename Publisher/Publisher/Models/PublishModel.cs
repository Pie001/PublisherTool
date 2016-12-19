using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Web.Mvc;
using Publisher.Validation;

namespace Publisher.Models
{
    /// <summary>
    /// 発行モデル
    /// </summary>
    public class PublishModel
    {
        /// <summary>
        /// 商材区分
        /// </summary>
        [NotValue("0", ErrorMessage = "商材を選択してください。")]
        [Display(Name = "商材区分")]
        public string ProductDivision
        {
            get;
            set;
        }

        /// <summary>
        /// バックアップ区分
        /// </summary>
        [NotValue("0", ErrorMessage = "バックアップ区分を選択してください。")]
        [Display(Name = "バックアップ区分")]
        public string BackupDivision
        {
            get;
            set;
        }

        /// <summary>
        /// From(発行元)
        /// バリデーションチェック：必須
        /// </summary>
        [Required(ErrorMessage = "From(発行元)が空白です。商材区分とサーバー区分を正しく選択してください。")]
        [Display(Name = "From(発行元)")]
        public string FromPath
        {
            get;
            set;
        }

        /// <summary>
        /// To(発行先)
        /// バリデーションチェック：必須、発行元との比較
        /// </summary>
        [Required(ErrorMessage = "To(発行先)が空白です。商材区分とサーバー区分を正しく選択してください。")]
        [Display(Name = "To(発行先)")]
        //[NotEqual("FromPath", ErrorMessage = "発行先が発行元と同一です。発行先を正しく指定してください。")]
        public string ToPath
        {
            get;
            set;
        }

        /// <summary>
        /// 商材一覧(プルダウン表示用)
        /// ※商材一覧は取得だけ必要なため、setは不要
        /// </summary>
        public SelectList ProductList
        {
            get
            {
                return new SelectList(ReadProductCsv(), "Value", "ViewName", "0");
            }
        }

        /// <summary>
        /// guid
        /// (F5対策)
        /// </summary>
        public string Guid
        {
            get;
            set;
        }

        /// <summary>
        /// ProductCSVファイルから商材情報を取得
        /// </summary>
        /// <returns>商材一覧</returns>
        public static List<EnvDetail> ReadProductCsv()
        {
            List<EnvDetail> envDetailList = new List<EnvDetail>();
            int count = 0;

            // csvファイルを開く
            using (var sr = new System.IO.StreamReader(AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["ServerListCsv"]))
            //using (var sr = new System.IO.StreamReader(ConfigurationManager.AppSettings["ProductListCsv"]))
            {
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    EnvDetail envDetail = new EnvDetail();

                    // ファイルから一行読み込む
                    var line = sr.ReadLine();

                    // csvの１行目はヘッダなので読み込まない。２行目から読み込む。
                    if (count == 0)
                    {
                        // プルダウン表示名
                        envDetail.viewName = "Please select product";
                        // プルダウンのvalue
                        envDetail.value = Convert.ToString(count);
                        envDetailList.Add(envDetail);

                        count++;
                        continue;
                    }

                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    var values = line.Split(',');

                    // プルダウン表示名
                    envDetail.viewName = values[0];
                    // プルダウンのvalue
                    envDetail.value = values[1];

                    // 既に格納済みの商材情報か確認する。
                    // viewNameとvalueが同じのものがない場合(検索件数：0件)、一覧に格納する。
                    if (0 == envDetailList.Where(t => t.viewName == values[0] && t.value == values[1]).Count())
                    {                
                        envDetailList.Add(envDetail);
                        count++;
                    }
                }
            }

            return envDetailList;
        }

        /// <summary>
        /// ServerListCsvファイルからサーバー情報を全て取得
        /// </summary>
        /// <returns>サーバー一覧</returns>
        public static List<EnvDetail> ReadServerCsv()
        {
            List<EnvDetail> envDetailList = new List<EnvDetail>();

            int count = 0;

            // csvファイルを開く
            using (var sr = new System.IO.StreamReader(AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["ServerListCsv"]))
            {
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    EnvDetail envDetail = new EnvDetail();

                    // ファイルから一行読み込む
                    var line = sr.ReadLine();

                    // csvの１行目はヘッダなので読み込まない。２行目から読み込む。
                    if (count == 0)
                    {
                        count++;
                        continue;
                    }

                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    var values = line.Split(',');

                    // プルダウン表示名
                    envDetail.viewName = values[3];
                    // プルダウンのvalue
                    envDetail.value = values[4] + "," + values[5] + "," + values[1];// 0:仮本番パス、1:本番パス、2:商材区分番号
                    // 商材のhttpHeader
                    envDetail.httpHeader = values[0];
                    // 仮本番のパス
                    envDetail.FromPath = values[4];
                    // 本番のパス
                    envDetail.ToPath = values[5];

                    envDetailList.Add(envDetail);
                    count++;
                }
            }

            return envDetailList;
        }

        /// <summary>
        /// 商材区分に当てはまるServerListを返す
        /// </summary>
        /// <param name="selectValue">商材区分</param>
        /// <returns>サーバー一覧</returns>
        public static List<EnvDetail> GetServerList(string selectValue)
        {
            List<EnvDetail> envDetailList = new List<EnvDetail>();

            int count = 0;

            // csvファイルを開く
            using (var sr = new System.IO.StreamReader(AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["ServerListCsv"]))
            {
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    EnvDetail envDetail = new EnvDetail();

                    // ファイルから一行読み込む
                    var line = sr.ReadLine();

                    // csvの１行目はヘッダなので読み込まない。２行目から読み込む。
                    if (count == 0)
                    {
                        count++;
                        continue;
                    }

                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    var values = line.Split(',');

                    // 選択した商材配下のサーバー一覧のみ表示
                    if (selectValue == values[1])
                    {
                        // プルダウン表示名
                        envDetail.viewName = values[3];
                        // プルダウンのvalue
                        envDetail.value = values[4] + "," + values[5] + "," + values[1];// 0:仮本番パス、1:本番パス、2:商材区分番号
                        // 商材のhttpHeader
                        envDetail.httpHeader = values[0];
                        // 仮本番のパス
                        envDetail.FromPath = values[4];
                        // 本番のパス
                        envDetail.ToPath = values[5];

                        envDetailList.Add(envDetail);
                    }
                    count++;
                }
            }

            return envDetailList;
        }
    }
}