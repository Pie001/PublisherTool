using Publisher.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using System.Net;
using System.Web.Script.Serialization;

namespace Publisher.Controllers
{
    /// <summary>
    /// Buildコントロール
    /// </summary>
    public class BuildController : Controller
    {
        /// <summary>
        /// log4net logger
        /// </summary>
        private static readonly log4net.ILog Logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Build/Indexの初期表示
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult Index()
        {
            // トークンの比較(F5押下による呼び出しか確認)
            BuildModel buildModel = new BuildModel();

            return View(SetToken(buildModel));
        }

        /// <summary>
        /// Build/IndexのSubmit処理(Buildボタン押下後の処理)
        /// </summary>
        /// <param name="model">Publisherモデル</param>
        /// <param name="productDivision">商材区分</param>
        /// <param name="guid">guid(トークン用)</param>
        /// <returns>ActionResult</returns>
        [STAThread]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(BuildModel buildModel, string productDivision, string guid)
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
                // 処理
            }

            // 問題が発生した場合はフォームを再表示する
            // トークン処理(F5防止のため)
            return View(SetToken(buildModel));
        }

        /// <summary>
        /// トークン処理(F5防止のため)
        /// </summary>
        /// <param name="publishModel">publishModel to join</param>
        /// <returns>publishModel</returns>
        private BuildModel SetToken(BuildModel buildModel)
        {
            // GUID生成＆hiddenに格納
            string guid = Guid.NewGuid().ToString();
            buildModel.Guid = guid;
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

            return buildModel;
        }


        
    }
}