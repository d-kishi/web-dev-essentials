using System.ComponentModel.DataAnnotations;

namespace Web.Essentials.App.Validation;

/// <summary>
/// 許可する画像拡張子チェック用バリデーション属性
/// </summary>
/// <remarks>
/// アップロードされるファイルの拡張子を制限する
/// JPEG, PNG, GIF のみを許可
/// </remarks>
public class AllowedImageExtensionsAttribute : ValidationAttribute
{
    /// <summary>
    /// 許可する拡張子一覧
    /// </summary>
    public string[] AllowedExtensions { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="allowedExtensions">許可する拡張子（デフォルト: jpg, jpeg, png, gif）</param>
    public AllowedImageExtensionsAttribute(params string[] allowedExtensions)
    {
        AllowedExtensions = allowedExtensions.Length > 0 
            ? allowedExtensions 
            : new[] { ".jpg", ".jpeg", ".png", ".gif" };
        
        ErrorMessage = $"許可されている画像形式: {string.Join(", ", AllowedExtensions)}";
    }

    /// <summary>
    /// バリデーション実行
    /// </summary>
    /// <param name="value">検証対象の値（ファイルまたはファイルリスト）</param>
    /// <param name="validationContext">バリデーションコンテキスト</param>
    /// <returns>バリデーション結果</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        // 単一ファイルの場合
        if (value is IFormFile file)
        {
            return ValidateFile(file, validationContext);
        }

        // ファイルコレクションの場合
        if (value is IEnumerable<IFormFile> files)
        {
            foreach (var currentFile in files)
            {
                if (currentFile != null)
                {
                    var result = ValidateFile(currentFile, validationContext);
                    if (result != ValidationResult.Success)
                    {
                        return result;
                    }
                }
            }
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// 個別ファイルのバリデーション
    /// </summary>
    /// <param name="file">検証対象ファイル</param>
    /// <param name="validationContext">バリデーションコンテキスト</param>
    /// <returns>バリデーション結果</returns>
    private ValidationResult? ValidateFile(IFormFile file, ValidationContext validationContext)
    {
        if (file.Length == 0)
        {
            return ValidationResult.Success;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!AllowedExtensions.Select(ext => ext.ToLowerInvariant()).Contains(extension))
        {
            return new ValidationResult(
                ErrorMessage ?? $"許可されていない画像形式です: {extension}",
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}