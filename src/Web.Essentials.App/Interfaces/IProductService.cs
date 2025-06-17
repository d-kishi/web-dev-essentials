using Web.Essentials.App.DTOs;
using Web.Essentials.App.ViewModels;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.Interfaces;

/// <summary>
/// 商品サービスインターフェース
/// </summary>
/// <remarks>
/// 商品に関するビジネスロジックを定義
/// MVCコントローラーとAPIコントローラーの両方で使用
/// </remarks>
public interface IProductService
{
    #region 一覧・検索

    /// <summary>
    /// 商品一覧取得（ページング対応）
    /// </summary>
    /// <param name="searchRequest">検索条件</param>
    /// <returns>商品一覧DTO</returns>
    Task<ProductListDto> GetProductsAsync(ProductSearchRequestDto searchRequest);

    /// <summary>
    /// 商品一覧取得（ViewModel用）
    /// </summary>
    /// <param name="viewModel">検索条件ViewModel</param>
    /// <returns>商品一覧ViewModel</returns>
    Task<ProductIndexViewModel> GetProductIndexAsync(ProductIndexViewModel viewModel);

    #endregion

    #region 詳細・存在確認

    /// <summary>
    /// 商品詳細取得
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品詳細ViewModel</returns>
    Task<ProductDetailsViewModel?> GetProductDetailsAsync(int id);

    /// <summary>
    /// 商品存在確認
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>存在する場合true</returns>
    Task<bool> ExistsAsync(int id);

    #endregion

    #region 作成・更新・削除

    /// <summary>
    /// 商品作成
    /// </summary>
    /// <param name="createModel">作成用ViewModel</param>
    /// <returns>作成された商品ID</returns>
    Task<int> CreateProductAsync(ProductCreateViewModel createModel);

    /// <summary>
    /// 商品更新
    /// </summary>
    /// <param name="editModel">更新用ViewModel</param>
    /// <returns>更新成功の場合true</returns>
    Task<bool> UpdateProductAsync(ProductEditViewModel editModel);

    /// <summary>
    /// 商品削除
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>削除成功の場合true</returns>
    Task<bool> DeleteProductAsync(int id);

    #endregion

    #region 編集用データ取得

    /// <summary>
    /// 商品編集用データ取得
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>編集用ViewModel</returns>
    Task<ProductEditViewModel?> GetProductForEditAsync(int id);

    /// <summary>
    /// 商品作成用データ取得
    /// </summary>
    /// <returns>作成用ViewModel</returns>
    Task<ProductCreateViewModel> GetProductForCreateAsync();

    #endregion

    #region バリデーション

    /// <summary>
    /// JANコード重複チェック
    /// </summary>
    /// <param name="janCode">JANコード</param>
    /// <param name="excludeId">除外する商品ID（編集時）</param>
    /// <returns>重複している場合true</returns>
    Task<bool> IsJanCodeDuplicateAsync(string janCode, int? excludeId = null);

    #endregion

    #region 統計・サマリー

    /// <summary>
    /// 商品統計情報取得
    /// </summary>
    /// <returns>統計情報DTO</returns>
    Task<ProductStatisticsDto> GetStatisticsAsync();

    /// <summary>
    /// カテゴリ別商品数取得
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <returns>商品数</returns>
    Task<int> GetProductCountByCategoryAsync(int categoryId);

    #endregion
}