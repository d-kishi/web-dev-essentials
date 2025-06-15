using Microsoft.AspNetCore.Mvc;
using Web.Essentials.App.ViewModels;
using Web.Essentials.Domain.Repositories;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.Controllers.Mvc;

/// <summary>
/// カテゴリ管理MVCコントローラー
/// カテゴリの一覧表示、登録、編集、削除のMVC画面を提供
/// </summary>
public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryController> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリとロガーを受け取る
    /// </summary>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="logger">ロガー</param>
    public CategoryController(
        ICategoryRepository categoryRepository,
        ILogger<CategoryController> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// カテゴリ一覧画面表示
    /// 初期表示時は全カテゴリを表示
    /// </summary>
    /// <returns>カテゴリ一覧ビュー</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            // 全カテゴリを取得
            var categories = await _categoryRepository.GetAllAsync();
            
            // カテゴリ一覧ViewModelに変換
            var viewModel = new CategoryIndexViewModel
            {
                Categories = categories.Select(c => new CategoryListItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    Level = 0, // Default level
                    SortOrder = 0, // Default sort order
                    FullPath = c.Name, // Simple path
                    ProductCount = 0, // Will be populated by service if needed
                    ChildCount = 0, // Will be populated by service if needed
                    CanDelete = true // Default to true
                }).ToList(),
                SearchKeyword = string.Empty,
                CurrentPage = 1,
                PageSize = 10
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ一覧表示中にエラーが発生しました");
            return View("Error");
        }
    }

    /// <summary>
    /// カテゴリ詳細表示
    /// 指定されたIDのカテゴリ詳細を表示
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>カテゴリ詳細ビュー</returns>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"カテゴリID {id} が見つかりません");
            }
            
            // 詳細ViewModelに変換
            var viewModel = new CategoryDetailsViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ詳細表示中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// カテゴリ登録画面表示
    /// 新規カテゴリ登録フォームを表示
    /// </summary>
    /// <returns>カテゴリ登録ビュー</returns>
    public IActionResult Create()
    {
        var viewModel = new CategoryCreateViewModel();
        return View(viewModel);
    }

    /// <summary>
    /// カテゴリ登録処理
    /// フォームから送信されたカテゴリ情報を登録
    /// </summary>
    /// <param name="viewModel">カテゴリ登録ViewModel</param>
    /// <returns>登録成功時は一覧画面へリダイレクト、失敗時は登録画面を再表示</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // エンティティに変換
            var category = new Category
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // カテゴリを登録
            await _categoryRepository.AddAsync(category);

            TempData["SuccessMessage"] = "カテゴリが正常に登録されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ登録中にエラーが発生しました");
            ModelState.AddModelError("", "カテゴリ登録中にエラーが発生しました");
            return View(viewModel);
        }
    }

    /// <summary>
    /// カテゴリ編集画面表示
    /// 指定されたIDのカテゴリ編集フォームを表示
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>カテゴリ編集ビュー</returns>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"カテゴリID {id} が見つかりません");
            }

            var viewModel = new CategoryEditViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ編集画面表示中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// カテゴリ編集処理
    /// フォームから送信されたカテゴリ情報で更新
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <param name="viewModel">カテゴリ編集ViewModel</param>
    /// <returns>更新成功時は一覧画面へリダイレクト、失敗時は編集画面を再表示</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryEditViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest("カテゴリIDが一致しません");
        }

        try
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"カテゴリID {id} が見つかりません");
            }

            // カテゴリ情報を更新
            category.Name = viewModel.Name;
            category.Description = viewModel.Description;
            category.UpdatedAt = DateTime.Now;

            await _categoryRepository.UpdateAsync(category);

            TempData["SuccessMessage"] = "カテゴリが正常に更新されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ更新中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            ModelState.AddModelError("", "カテゴリ更新中にエラーが発生しました");
            return View(viewModel);
        }
    }

    /// <summary>
    /// カテゴリ削除確認画面表示
    /// 指定されたIDのカテゴリ削除確認画面を表示
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>カテゴリ削除確認ビュー</returns>
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"カテゴリID {id} が見つかりません");
            }
            
            var viewModel = new CategoryDeleteViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除確認画面表示中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// カテゴリ削除処理
    /// 指定されたIDのカテゴリを削除
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>削除成功時は一覧画面へリダイレクト</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound($"カテゴリID {id} が見つかりません");
            }

            // カテゴリを削除
            await _categoryRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = "カテゴリが正常に削除されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            TempData["ErrorMessage"] = "カテゴリ削除中にエラーが発生しました";
            return RedirectToAction(nameof(Index));
        }
    }
}