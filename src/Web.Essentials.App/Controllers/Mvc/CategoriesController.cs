using Microsoft.AspNetCore.Mvc;
using Web.Essentials.App.ViewModels;
using Web.Essentials.App.Interfaces;
using Web.Essentials.Domain.Repositories;
using Web.Essentials.Domain.Entities;

namespace Web.Essentials.App.Controllers.Mvc;

/// <summary>
/// カテゴリ管理MVCコントローラー
/// カテゴリの一覧表示、登録、編集、削除のMVC画面を提供
/// ルーティング: /Categories/*
/// </summary>
public class CategoriesController : Controller
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリ、サービス、ロガーを受け取る
    /// </summary>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="categoryService">カテゴリサービス</param>
    /// <param name="logger">ロガー</param>
    public CategoriesController(
        ICategoryRepository categoryRepository,
        ICategoryService categoryService,
        ILogger<CategoriesController> logger)
    {
        _categoryRepository = categoryRepository;
        _categoryService = categoryService;
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
    /// カテゴリ登録画面表示
    /// 新規カテゴリ登録フォームを表示
    /// </summary>
    /// <returns>カテゴリ登録ビュー</returns>
    public async Task<IActionResult> Create()
    {
        try
        {
            var viewModel = await _categoryService.GetCategoryForCreateAsync();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ登録画面表示中にエラーが発生しました");
            return View("Error");
        }
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
                // Ajaxリクエストの場合はJSONエラーレスポンスを返す
                if (Request.Headers.Accept.ToString().Contains("application/json") || 
                    Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Message = x.Value.Errors.First().ErrorMessage })
                        .ToList();
                    
                    return Json(new { success = false, errors = errors });
                }

                // 通常のリクエストの場合はビューを返す
                var errorViewModel = await _categoryService.GetCategoryForCreateAsync();
                errorViewModel.Name = viewModel.Name;
                errorViewModel.Description = viewModel.Description;
                errorViewModel.ParentCategoryId = viewModel.ParentCategoryId;
                return View(errorViewModel);
            }

            // カテゴリサービス経由で登録
            var categoryId = await _categoryService.CreateCategoryAsync(viewModel);

            // Ajaxリクエストの場合はJSONレスポンスを返す
            if (Request.Headers.Accept.ToString().Contains("application/json") || 
                Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, categoryId = categoryId, message = "カテゴリが正常に登録されました" });
            }

            // 通常のリクエストの場合はリダイレクト
            TempData["SuccessMessage"] = "カテゴリが正常に登録されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ登録中にエラーが発生しました");
            
            // Ajaxリクエストの場合はJSONエラーレスポンスを返す
            if (Request.Headers.Accept.ToString().Contains("application/json") || 
                Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "カテゴリ登録中にエラーが発生しました" });
            }

            // 通常のリクエストの場合はエラービューを返す
            ModelState.AddModelError("", "カテゴリ登録中にエラーが発生しました");
            var errorViewModel = await _categoryService.GetCategoryForCreateAsync();
            errorViewModel.Name = viewModel.Name;
            errorViewModel.Description = viewModel.Description;
            errorViewModel.ParentCategoryId = viewModel.ParentCategoryId;
            return View(errorViewModel);
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
            var viewModel = await _categoryService.GetCategoryForEditAsync(id);
            if (viewModel == null)
            {
                return NotFound($"カテゴリID {id} が見つかりません");
            }

            // 親カテゴリ選択肢を設定
            var parentCategories = await _categoryService.GetCategorySelectItemsAsync();
            viewModel.ParentCategories = parentCategories;

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
            // Ajaxリクエストの場合はJSONエラーレスポンスを返す
            if (Request.Headers.Accept.ToString().Contains("application/json") || 
                Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "カテゴリIDが一致しません" });
            }
            return BadRequest("カテゴリIDが一致しません");
        }

        try
        {
            if (!ModelState.IsValid)
            {
                // Ajaxリクエストの場合はJSONエラーレスポンスを返す
                if (Request.Headers.Accept.ToString().Contains("application/json") || 
                    Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Message = x.Value.Errors.First().ErrorMessage })
                        .ToList();
                    
                    return Json(new { success = false, errors = errors });
                }

                // 通常のリクエストの場合はビューを返す
                var parentCategories = await _categoryService.GetCategorySelectItemsAsync();
                viewModel.ParentCategories = parentCategories;
                return View(viewModel);
            }

            // カテゴリサービス経由で更新
            var success = await _categoryService.UpdateCategoryAsync(viewModel);
            if (!success)
            {
                // Ajaxリクエストの場合はJSONエラーレスポンスを返す
                if (Request.Headers.Accept.ToString().Contains("application/json") || 
                    Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"カテゴリID {id} が見つかりません" });
                }
                return NotFound($"カテゴリID {id} が見つかりません");
            }

            // Ajaxリクエストの場合はJSONレスポンスを返す
            if (Request.Headers.Accept.ToString().Contains("application/json") || 
                Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, categoryId = id, message = "カテゴリが正常に更新されました" });
            }

            // 通常のリクエストの場合はリダイレクト
            TempData["SuccessMessage"] = "カテゴリが正常に更新されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ更新中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            
            // Ajaxリクエストの場合はJSONエラーレスポンスを返す
            if (Request.Headers.Accept.ToString().Contains("application/json") || 
                Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "カテゴリ更新中にエラーが発生しました" });
            }

            // 通常のリクエストの場合はエラービューを返す
            ModelState.AddModelError("", "カテゴリ更新中にエラーが発生しました");
            var parentCategories = await _categoryService.GetCategorySelectItemsAsync();
            viewModel.ParentCategories = parentCategories;
            return View(viewModel);
        }
    }


    /// <summary>
    /// カテゴリ削除処理
    /// 指定されたIDのカテゴリを削除
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>削除成功時は一覧画面へリダイレクト</returns>
    [HttpPost]
    [Route("Categories/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                if (Request.Headers.Accept.ToString().Contains("application/json") || 
                    Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"カテゴリID {id} が見つかりません" });
                }
                return NotFound($"カテゴリID {id} が見つかりません");
            }

            // カテゴリを削除
            await _categoryRepository.DeleteAsync(id);

            if (Request.Headers.Accept.ToString().Contains("application/json") || 
                Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "カテゴリが正常に削除されました" });
            }

            TempData["SuccessMessage"] = "カテゴリが正常に削除されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除中にエラーが発生しました。カテゴリID: {CategoryId}", id);
            
            if (Request.Headers.Accept.ToString().Contains("application/json") || 
                Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "カテゴリ削除中にエラーが発生しました" });
            }
            
            TempData["ErrorMessage"] = "カテゴリ削除中にエラーが発生しました";
            return RedirectToAction(nameof(Index));
        }
    }
}