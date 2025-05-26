using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Essentials.Mvc.Models;

namespace Web.Essentials.Mvc.Controllers
{
    /// <summary>
    /// ユーザ処理コントローラ
    /// </summary>
    public class UserController : Controller
    {
        /// <summary>
        /// 新規ユーザ登録画面を表示する
        /// </summary>
        /// <remarks>
        /// Get: UserController/Create
        /// </remarks>
        /// <returns>
        /// <see cref="UserModel"/> の空のインスタンスを使って新規ユーザー作成ビューを表示する <see cref="ActionResult"/> を返します。
        /// </returns>
        public ActionResult Create()
        {
            return View(new UserModel());
        }

        /// <summary>
        /// 新規ユーザを登録する
        /// </summary
        /// <remarks>
        /// Post: UserController/Create
        /// </remarks>
        /// <param name="userModel">ユーザ情報モデル</param>
        /// <returns>単純にPostしたモデルを返す</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserModel userModel)
        {
            // モデルの検証を行うには以下のようにModelStateを使用します。
            if (!ModelState.IsValid)
            {
                // モデルの検証に失敗した場合、入力内容を保持して再表示
                return View(userModel);
            }

            return View(userModel);
        }
    }
}
