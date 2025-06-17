using System.ComponentModel.DataAnnotations;

namespace Web.Essentials.App.Validation;

/// <summary>
/// 最大ファイルサイズチェック用バリデーション属性
/// </summary>
/// <remarks>
/// アップロードされるファイルのサイズを制限する
/// デフォルトは5MB
/// </remarks>
public class MaxFileSizeAttribute : ValidationAttribute
{
    /// <summary>
    /// 最大ファイルサイズ（バイト）
    /// </summary>
    public long MaxSizeInBytes { get; }

    /// <summary>
    /// 最大ファイルサイズ（MB表示用）
    /// </summary>
    public double MaxSizeInMB => MaxSizeInBytes / (1024.0 * 1024.0);

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="maxSizeInMB">最大ファイルサイズ（MB）</param>
    public MaxFileSizeAttribute(double maxSizeInMB = 5.0)
    {
        MaxSizeInBytes = (long)(maxSizeInMB * 1024 * 1024);
        ErrorMessage = $"ファイルサイズは{maxSizeInMB:F1}MB以下にしてください。";
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

        if (file.Length > MaxSizeInBytes)
        {
            var fileSizeInMB = file.Length / (1024.0 * 1024.0);
            return new ValidationResult(
                $"ファイル '{file.FileName}' のサイズ（{fileSizeInMB:F1}MB）が上限（{MaxSizeInMB:F1}MB）を超えています。",
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}