using Web.Essentials.App.ViewModels;
using Web.Essentials.App.Interfaces;
using Web.Essentials.Domain.Entities;
using Web.Essentials.Domain.Repositories;

namespace Web.Essentials.App.Services;

/// <summary>
/// カテゴリ階層構造処理専用サービス
/// カテゴリの階層構造の構築と変換処理を担当
/// 単一責任の原則に基づき、階層構造処理のみに集中
/// </summary>
public class CategoryHierarchyService : ICategoryHierarchyService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryHierarchyService> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリとロガーを受け取る
    /// </summary>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="logger">ロガー</param>
    public CategoryHierarchyService(
        ICategoryRepository categoryRepository,
        ILogger<CategoryHierarchyService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategorySelectItem>> GetCategorySelectItemsAsync()
    {
        try
        {
            _logger.LogInformation("カテゴリ選択用アイテム一覧取得実行");

            var allCategories = await _categoryRepository.GetAllAsync();
            var categoryDict = allCategories.ToDictionary(c => c.Id, c => c);

            var selectItems = new List<CategorySelectItem>();

            // ルートカテゴリから階層順に処理
            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null)
                                              .OrderBy(c => c.SortOrder)
                                              .ThenBy(c => c.Name);

            foreach (var rootCategory in rootCategories)
            {
                AddCategorySelectItemsRecursively(rootCategory, categoryDict, selectItems, 0);
            }

            _logger.LogInformation("カテゴリ選択用アイテム一覧取得完了。取得件数: {Count}", selectItems.Count);
            return selectItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ選択用アイテム一覧取得中にエラーが発生しました");
            throw;
        }
    }

    /// <inheritdoc />
    public string BuildCategoryFullPath(Category category, Dictionary<int, Category> categoryDict)
    {
        if (category == null)
        {
            return string.Empty;
        }

        var pathSegments = new List<string>();
        var currentCategory = category;

        // 現在のカテゴリから親カテゴリを遡って階層パスを構築
        while (currentCategory != null)
        {
            pathSegments.Insert(0, currentCategory.Name);

            if (currentCategory.ParentCategoryId.HasValue && 
                categoryDict.TryGetValue(currentCategory.ParentCategoryId.Value, out var parentCategory))
            {
                currentCategory = parentCategory;
            }
            else
            {
                currentCategory = null;
            }
        }

        return string.Join(" > ", pathSegments);
    }

    /// <inheritdoc />
    public Task<List<Category>> BuildCategoryHierarchyAsync(IEnumerable<Category> categories)
    {
        try
        {
            _logger.LogInformation("カテゴリ階層構造構築実行");

            var categoryList = categories.ToList();
            var categoryDict = categoryList.ToDictionary(c => c.Id, c => c);

            // 子カテゴリリストを初期化
            foreach (var category in categoryList)
            {
                category.ChildCategories = new List<Category>();
            }

            // 親子関係を構築
            foreach (var category in categoryList)
            {
                if (category.ParentCategoryId.HasValue && 
                    categoryDict.TryGetValue(category.ParentCategoryId.Value, out var parentCategory))
                {
                    parentCategory.ChildCategories.Add(category);
                }
            }

            // ルートカテゴリのみを返す（子カテゴリは各カテゴリのChildCategoriesプロパティで参照）
            var rootCategories = categoryList
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToList();

            // 各階層でソートを実行
            SortCategoryHierarchy(rootCategories);

            _logger.LogInformation("カテゴリ階層構造構築完了。ルートカテゴリ数: {Count}", rootCategories.Count);
            return Task.FromResult(rootCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ階層構造構築中にエラーが発生しました");
            throw;
        }
    }

    /// <inheritdoc />
    public List<Category> GetCategoryPath(int categoryId, Dictionary<int, Category> categoryDict)
    {
        var path = new List<Category>();

        if (!categoryDict.TryGetValue(categoryId, out var currentCategory))
        {
            return path;
        }

        // 現在のカテゴリから親カテゴリを遡ってパスを構築
        while (currentCategory != null)
        {
            path.Insert(0, currentCategory);

            if (currentCategory.ParentCategoryId.HasValue && 
                categoryDict.TryGetValue(currentCategory.ParentCategoryId.Value, out var parentCategory))
            {
                currentCategory = parentCategory;
            }
            else
            {
                currentCategory = null;
            }
        }

        return path;
    }

    /// <inheritdoc />
    public int CalculateCategoryLevel(int categoryId, Dictionary<int, Category> categoryDict)
    {
        if (!categoryDict.TryGetValue(categoryId, out var category))
        {
            return 0;
        }

        var level = 0;
        var currentCategory = category;

        // 親カテゴリを遡ってレベルを計算
        while (currentCategory?.ParentCategoryId.HasValue == true)
        {
            level++;
            
            if (categoryDict.TryGetValue(currentCategory.ParentCategoryId.Value, out var parentCategory))
            {
                currentCategory = parentCategory;
            }
            else
            {
                break;
            }
        }

        return level;
    }

    /// <summary>
    /// カテゴリ選択アイテムを再帰的に追加する
    /// 階層構造を保ったまま、表示用のインデント情報も含めてCategorySelectItemを構築。
    /// 完全パスの生成、レベル別インデント、子カテゴリの再帰処理を実行。
    /// ソートオーダーと名前で各階層をソート
    /// </summary>
    /// <param name="category">処理対象のカテゴリエンティティ</param>
    /// <param name="categoryDict">高速検索用の全カテゴリマID辞書</param>
    /// <param name="selectItems">選択アイテムの追加先リスト</param>
    /// <param name="level">現在の階層レベル（0がルート）</param>
    private void AddCategorySelectItemsRecursively(
        Category category, 
        Dictionary<int, Category> categoryDict, 
        List<CategorySelectItem> selectItems, 
        int level)
    {
        // 現在のカテゴリを追加
        var fullPath = BuildCategoryFullPath(category, categoryDict);
        var indent = new string(' ', level * 2); // レベル毎に2つの空白でインデント

        selectItems.Add(new CategorySelectItem
        {
            Id = category.Id,
            Name = category.Name,
            FullPath = fullPath,
            Level = level
        });

        // 子カテゴリを再帰的に処理
        var childCategories = categoryDict.Values
            .Where(c => c.ParentCategoryId == category.Id)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name);

        foreach (var childCategory in childCategories)
        {
            AddCategorySelectItemsRecursively(childCategory, categoryDict, selectItems, level + 1);
        }
    }

    /// <summary>
    /// カテゴリ階層を再帰的にソートする
    /// 各階層でSortOrder、次にNameでソートし、子カテゴリに対しても再帰的に同様のソートを適用。
    /// 階層構造全体で一貫した表示順序を保証
    /// </summary>
    /// <param name="categories">ソート対象のカテゴリリスト（子カテゴリも含む）</param>
    private void SortCategoryHierarchy(List<Category> categories)
    {
        foreach (var category in categories)
        {
            if (category.ChildCategories.Any())
            {
                category.ChildCategories = category.ChildCategories
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .ToList();

                // 子カテゴリも再帰的にソート
                SortCategoryHierarchy((List<Category>)category.ChildCategories);
            }
        }
    }
}