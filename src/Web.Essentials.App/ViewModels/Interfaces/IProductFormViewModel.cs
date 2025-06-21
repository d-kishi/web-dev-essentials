using System.Collections.Generic;
using Web.Essentials.App.ViewModels;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.ViewModels.Interfaces
{
    /// <summary>
    /// 商品フォーム用ViewModelの共通インターフェース
    /// Create/Edit画面で共通利用される部分ビューで使用
    /// </summary>
    public interface IProductFormViewModel
    {
        /// <summary>
        /// 商品名
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 価格
        /// </summary>
        uint Price { get; set; }

        /// <summary>
        /// 商品説明
        /// </summary>
        string? Description { get; set; }

        /// <summary>
        /// JANコード
        /// </summary>
        string? JanCode { get; set; }

        /// <summary>
        /// 商品ステータス
        /// </summary>
        ProductStatus Status { get; set; }

        /// <summary>
        /// 選択されたカテゴリID一覧（多対多対応）
        /// </summary>
        List<int> SelectedCategoryIds { get; set; }

        /// <summary>
        /// 利用可能なカテゴリ一覧
        /// </summary>
        IEnumerable<CategorySelectItem> Categories { get; set; }
    }
}