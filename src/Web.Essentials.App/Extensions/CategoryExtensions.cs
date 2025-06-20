using Web.Essentials.App.ViewModels;

namespace Web.Essentials.App.Extensions;

/// <summary>
/// カテゴリ関連の拡張メソッド
/// </summary>
public static class CategoryExtensions
{
    /// <summary>
    /// CategorySelectItemをCategoryTreeNodeViewModelに変換
    /// </summary>
    /// <param name="item">変換元のCategorySelectItem</param>
    /// <returns>変換されたCategoryTreeNodeViewModel</returns>
    public static CategoryTreeNodeViewModel ToCategoryTreeNodeViewModel(this CategorySelectItem item)
    {
        return new CategoryTreeNodeViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Level = item.Level,
            FullPath = item.FullPath,
            ProductCount = item.ProductCount,
            ChildCategories = item.ChildCategories?.Select(child => child.ToCategoryTreeNodeViewModel()) ?? Enumerable.Empty<CategoryTreeNodeViewModel>()
        };
    }

    /// <summary>
    /// CategorySelectItemのコレクションをCategoryTreeNodeViewModelのコレクションに変換
    /// </summary>
    /// <param name="items">変換元のCategorySelectItemコレクション</param>
    /// <returns>変換されたCategoryTreeNodeViewModelコレクション</returns>
    public static IEnumerable<CategoryTreeNodeViewModel> ToCategoryTreeNodeViewModels(this IEnumerable<CategorySelectItem> items)
    {
        return items?.Select(item => item.ToCategoryTreeNodeViewModel()) ?? Enumerable.Empty<CategoryTreeNodeViewModel>();
    }
}