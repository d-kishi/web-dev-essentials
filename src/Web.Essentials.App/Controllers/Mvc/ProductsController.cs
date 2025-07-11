using Microsoft.AspNetCore.Mvc;
using Web.Essentials.App.ViewModels;
using Web.Essentials.Domain.Repositories;
using Web.Essentials.Domain.Entities;
using Web.Essentials.App.Interfaces;

namespace Web.Essentials.App.Controllers.Mvc;

/// <summary>
/// 商品管理MVCコントローラー
/// 商品の一覧表示、登録、編集、削除のMVC画面を提供
/// </summary>
public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductImageRepository _productImageRepository;
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// コンストラクタ
    /// 依存性注入によりリポジトリとサービス、ロガーを受け取る
    /// </summary>
    /// <param name="productRepository">商品リポジトリ</param>
    /// <param name="categoryRepository">カテゴリリポジトリ</param>
    /// <param name="productImageRepository">商品画像リポジトリ</param>
    /// <param name="productService">商品サービス</param>
    /// <param name="logger">ロガー</param>
    public ProductsController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProductImageRepository productImageRepository,
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productImageRepository = productImageRepository;
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// 商品一覧画面表示
    /// 初期表示時は全商品を表示
    /// </summary>
    /// <returns>商品一覧ビュー</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            // 全商品とカテゴリを取得
            var (products, totalCount) = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            
            // カテゴリの階層構造を構築するための辞書を作成
            var categoryDict = categories.ToDictionary(c => c.Id, c => c);
            
            // 商品一覧ViewModelに変換
            var viewModel = new ProductIndexViewModel
            {
                Products = products.Select(p => new ProductListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ShortDescription = p.Description?.Length > 100 ? p.Description.Substring(0, 100) + "..." : p.Description,
                    Price = (uint)p.Price,
                    JanCode = p.JanCode,
                    Status = p.Status,
                    MainImagePath = p.ProductImages.FirstOrDefault(img => img.IsMain)?.ImagePath ?? p.ProductImages.FirstOrDefault()?.ImagePath,
                    CategoryNames = p.ProductCategories.Select(pc => pc.Category.Name).ToList(),
                    CategoryName = p.ProductCategories.FirstOrDefault()?.Category.Name ?? "未分類",
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList(),
                SearchKeyword = string.Empty,
                CurrentPage = 1,
                PageSize = 10,
                PagingInfo = new PagingInfo
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / 10),
                    HasPreviousPage = false,
                    HasNextPage = totalCount > 10
                },
                Categories = categories
                    .Select(c => new CategorySelectItem
                    {
                        Id = c.Id,
                        Name = c.Name,
                        FullPath = BuildCategoryFullPath(c, categoryDict),
                        Level = c.Level
                    })
                    .OrderBy(c => c.FullPath) // FullPath（階層構造パス）の昇順でソート
                    .ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品一覧表示中にエラーが発生しました");
            return View("Error");
        }
    }

    /// <summary>
    /// カテゴリの階層構造FullPathを構築
    /// </summary>
    /// <param name="category">対象カテゴリ</param>
    /// <param name="categoryDict">カテゴリ辞書</param>
    /// <returns>階層構造を表すパス（例：「野球 > バット > 硬式バット」）</returns>
    private static string BuildCategoryFullPath(Category category, Dictionary<int, Category> categoryDict)
    {
        var pathParts = new List<string>();
        var current = category;

        // 自分から親に向かって辿る
        while (current != null)
        {
            pathParts.Insert(0, current.Name); // 先頭に挿入
            
            if (current.ParentCategoryId.HasValue && categoryDict.ContainsKey(current.ParentCategoryId.Value))
            {
                current = categoryDict[current.ParentCategoryId.Value];
            }
            else
            {
                current = null; // ルートカテゴリに到達
            }
        }

        return string.Join(" > ", pathParts);
    }

    /// <summary>
    /// 商品詳細表示
    /// 指定されたIDの商品詳細を表示
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品詳細ビュー</returns>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound($"商品ID {id} が見つかりません");
            }

            // 商品画像を取得
            var productImages = await _productImageRepository.GetByProductIdAsync(id);
            
            // 詳細ViewModelに変換
            var viewModel = new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = (uint)product.Price,
                Status = product.Status,
                JanCode = product.JanCode,
                Categories = product.ProductCategories.Select(pc => new CategoryDisplayItem
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    FullPath = pc.Category.Name
                }).ToList(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Images = productImages.Select(img => new ProductImageDisplayItem
                {
                    Id = img.Id,
                    ImagePath = img.ImagePath,
                    DisplayOrder = img.DisplayOrder,
                    AltText = img.AltText,
                    IsMain = img.IsMain
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品詳細表示中にエラーが発生しました。商品ID: {ProductId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// 商品登録画面表示
    /// 新規商品登録フォームを表示
    /// </summary>
    /// <returns>商品登録ビュー</returns>
    public async Task<IActionResult> Create()
    {
        try
        {
            // ProductService を使用してカテゴリを取得（最下位カテゴリのみ、階層構造ラベル付き）
            var viewModel = await _productService.GetProductForCreateAsync();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品登録画面表示中にエラーが発生しました");
            return View("Error");
        }
    }

    /// <summary>
    /// 商品登録処理
    /// フォームから送信された商品情報を登録
    /// </summary>
    /// <param name="viewModel">商品登録ViewModel</param>
    /// <returns>登録成功時は一覧画面へリダイレクト、失敗時は登録画面を再表示</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // バリデーションエラー時はカテゴリ一覧を再取得
                var categories = await _categoryRepository.GetAllAsync();
                viewModel.Categories = categories.Select(c => new CategorySelectItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    FullPath = c.Name,
                    Level = 0
                }).ToList();
                
                return View(viewModel);
            }

            // エンティティに変換
            var product = new Product
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Price = viewModel.Price,
                Status = viewModel.Status,
                JanCode = viewModel.JanCode,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 商品を登録
            await _productRepository.AddAsync(product);

            // カテゴリ関係の設定
            if (viewModel.SelectedCategoryIds?.Any() == true)
            {
                foreach (var categoryId in viewModel.SelectedCategoryIds)
                {
                    var productCategory = new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId,
                        CreatedAt = DateTime.UtcNow
                    };
                    product.ProductCategories.Add(productCategory);
                }
                await _productRepository.UpdateAsync(product);
            }

            // 画像ファイルがアップロードされている場合の処理
            if (viewModel.ImageFiles != null && viewModel.ImageFiles.Any())
            {
                await ProcessNewImageUploadsAsync(product.Id, viewModel.ImageFiles, 1, viewModel.ImageAltTexts, viewModel.ImageIsMainFlags);
            }

            TempData["SuccessMessage"] = "商品が正常に登録されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品登録中にエラーが発生しました");
            ModelState.AddModelError("", "商品登録中にエラーが発生しました");
            
            // エラー時もカテゴリ一覧を再取得（ProductServiceを使用）
            var createViewModel = await _productService.GetProductForCreateAsync();
            viewModel.Categories = createViewModel.Categories;
            
            return View(viewModel);
        }
    }

    /// <summary>
    /// 商品編集画面表示
    /// 指定されたIDの商品編集フォームを表示
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>商品編集ビュー</returns>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            // ProductService を使用してEditViewModel を取得（最下位カテゴリのみ、階層構造ラベル付き）
            var viewModel = await _productService.GetProductForEditAsync(id);
            if (viewModel == null)
            {
                return NotFound($"商品ID {id} が見つかりません");
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品編集画面表示中にエラーが発生しました。商品ID: {ProductId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// 商品編集処理
    /// フォームから送信された商品情報で更新
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <param name="viewModel">商品編集ViewModel</param>
    /// <returns>更新成功時は一覧画面へリダイレクト、失敗時は編集画面を再表示</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductEditViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest("商品IDが一致しません");
        }

        try
        {
            if (!ModelState.IsValid)
            {
                // バリデーションエラー時は必要なデータを再取得
                await ReloadEditViewModelDataAsync(viewModel);
                return View(viewModel);
            }

            // ProductService の安全な更新メソッドを使用
            var updateResult = await _productService.UpdateProductAsync(viewModel);
            if (!updateResult)
            {
                ModelState.AddModelError("", "商品の更新に失敗しました");
                await ReloadEditViewModelDataAsync(viewModel);
                return View(viewModel);
            }

            // 画像の統合保存処理
            await ProcessImageUpdatesAsync(viewModel);

            TempData["SuccessMessage"] = "商品が正常に更新されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品更新中にエラーが発生しました。商品ID: {ProductId}", id);
            ModelState.AddModelError("", "商品更新中にエラーが発生しました");
            
            await ReloadEditViewModelDataAsync(viewModel);
            return View(viewModel);
        }
    }

    /// <summary>
    /// Ajax商品削除処理
    /// 商品一覧画面からのAjax削除リクエストを処理
    /// </summary>
    /// <param name="id">商品ID</param>
    /// <returns>削除成功時は一覧画面へリダイレクト</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound($"商品ID {id} が見つかりません");
            }

            // 関連する商品画像レコードを削除（物理ファイルは残す）
            var productImages = await _productImageRepository.GetByProductIdAsync(id);
            foreach (var image in productImages)
            {
                await _productImageRepository.DeleteAsync(image.Id);
                // 物理ファイルは削除しない（要件により保持）
            }

            // 商品を削除
            await _productRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = "商品が正常に削除されました";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品削除中にエラーが発生しました。商品ID: {ProductId}", id);
            TempData["ErrorMessage"] = "商品削除中にエラーが発生しました";
            return RedirectToAction(nameof(Index));
        }
    }

    #region Private Helper Methods



    /// <summary>
    /// 編集ViewModel用データの再読み込み
    /// バリデーションエラー時などに使用
    /// </summary>
    /// <param name="viewModel">編集ViewModel</param>
    private async Task ReloadEditViewModelDataAsync(ProductEditViewModel viewModel)
    {
        // ProductServiceを使用してカテゴリを取得（最下位のみ、階層構造ラベル付き）
        var editViewModel = await _productService.GetProductForEditAsync(viewModel.Id);
        if (editViewModel != null)
        {
            viewModel.Categories = editViewModel.Categories;
        }

        var productImages = await _productImageRepository.GetByProductIdAsync(viewModel.Id);

        viewModel.ExistingImages = productImages.Select(img => new ProductImageViewModel
        {
            Id = img.Id,
            ProductId = img.ProductId,
            ImagePath = img.ImagePath,
            DisplayOrder = img.DisplayOrder,
            AltText = img.AltText,
            IsMain = img.IsMain,
            CreatedAt = img.CreatedAt
        }).ToList();

        // 既存のカテゴリ選択状態を保持
        if (viewModel.SelectedCategoryIds?.Count == 0)
        {
            var product = await _productRepository.GetByIdAsync(viewModel.Id);
            if (product != null)
            {
                viewModel.SelectedCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList();
            }
        }
    }

    /// <summary>
    /// 画像の統合保存処理（既存画像の更新+新規画像の追加）
    /// </summary>
    /// <param name="viewModel">編集用ビューモデル</param>
    private async Task ProcessImageUpdatesAsync(ProductEditViewModel viewModel)
    {
        // 既存画像の更新処理
        if (!string.IsNullOrEmpty(viewModel.ExistingImageData))
        {
            var existingImageUpdates = System.Text.Json.JsonSerializer.Deserialize<List<ExistingImageUpdateData>>(viewModel.ExistingImageData);
            if (existingImageUpdates != null)
            {
                foreach (var update in existingImageUpdates)
                {
                    var existingImage = await _productImageRepository.GetByIdAsync(update.Id);
                    if (existingImage != null)
                    {
                        existingImage.AltText = update.AltText;
                        existingImage.IsMain = update.IsMain;
                        existingImage.DisplayOrder = update.DisplayOrder;
                        await _productImageRepository.UpdateAsync(existingImage);
                    }
                }
            }
        }

        // 新規画像の追加処理
        if (viewModel.ImageFiles != null && viewModel.ImageFiles.Any())
        {
            await ProcessNewImageUploadsAsync(viewModel.Id, viewModel.ImageFiles, viewModel.NewImageStartOrder, viewModel.ImageAltTexts, viewModel.ImageIsMainFlags);
        }
    }

    /// <summary>
    /// 新規画像のアップロード処理（DisplayOrderを指定開始値から設定）
    /// </summary>
    /// <param name="productId">商品ID</param>
    /// <param name="imageFiles">画像ファイル</param>
    /// <param name="startOrder">開始DisplayOrder</param>
    /// <param name="altTexts">代替テキストのリスト</param>
    /// <param name="isMainFlags">メイン画像フラグのリスト</param>
    private async Task ProcessNewImageUploadsAsync(int productId, IEnumerable<IFormFile> imageFiles, int startOrder, List<string>? altTexts, List<bool>? isMainFlags)
    {
        var uploadPath = Path.Combine("wwwroot", "uploads", "products");
        Directory.CreateDirectory(uploadPath);

        var displayOrder = startOrder;
        var fileIndex = 0;
        
        foreach (var file in imageFiles)
        {
            if (file.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var altText = altTexts != null && fileIndex < altTexts.Count ? altTexts[fileIndex] : "";
                var isMain = isMainFlags != null && fileIndex < isMainFlags.Count ? isMainFlags[fileIndex] : false;

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImagePath = $"/uploads/products/{fileName}",
                    DisplayOrder = displayOrder,
                    AltText = altText,
                    IsMain = isMain,
                    CreatedAt = DateTime.Now
                };

                await _productImageRepository.AddAsync(productImage);
                displayOrder++;
                fileIndex++;
            }
        }
    }

    /// <summary>
    /// 既存画像更新用データクラス
    /// </summary>
    private class ExistingImageUpdateData
    {
        public int Id { get; set; }
        public string AltText { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
    }

    #endregion
}