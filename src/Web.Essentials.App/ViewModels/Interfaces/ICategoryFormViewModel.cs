using System.Collections.Generic;
using Web.Essentials.App.ViewModels;

namespace Web.Essentials.App.ViewModels.Interfaces
{
    /// <summary>
    /// カテゴリフォーム用ViewModelの共通インターフェース
    /// Create/Edit画面で共通利用される部分ビューで使用
    /// </summary>
    public interface ICategoryFormViewModel
    {
        /// <summary>
        /// カテゴリ名
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// カテゴリ説明
        /// </summary>
        string? Description { get; set; }

        /// <summary>
        /// 親カテゴリID
        /// </summary>
        int? ParentCategoryId { get; set; }

        /// <summary>
        /// 利用可能な親カテゴリ一覧
        /// </summary>
        IEnumerable<CategorySelectItem> ParentCategories { get; set; }
    }
}