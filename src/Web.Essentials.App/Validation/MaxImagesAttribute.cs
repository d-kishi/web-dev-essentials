using System.ComponentModel.DataAnnotations;

namespace Web.Essentials.App.Validation;

/// <summary>
/// 最大画像数チェック用バリデーション属性
/// </summary>
/// <remarks>
/// 商品画像のアップロード時に最大件数（5件）を制限する
/// ファイルアップロード配列に対して適用
/// </remarks>
public class MaxImagesAttribute : ValidationAttribute
{
    /// <summary>
    /// 最大画像数
    /// </summary>
    public int MaxCount { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="maxCount">最大画像数</param>
    public MaxImagesAttribute(int maxCount = 5)
    {
        MaxCount = maxCount;
        ErrorMessage = $"画像は最大{maxCount}枚まで登録できます。";
    }

    /// <summary>
    /// バリデーション実行
    /// </summary>
    /// <param name="value">検証対象の値（ファイルリスト）</param>
    /// <param name="validationContext">バリデーションコンテキスト</param>
    /// <returns>バリデーション結果</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        // IFormFileのコレクションかチェック
        if (value is IEnumerable<IFormFile> files)
        {
            var fileCount = files.Count();
            if (fileCount > MaxCount)
            {
                return new ValidationResult(
                    string.Format(ErrorMessage ?? "画像は最大{0}枚まで登録できます。", MaxCount),
                    new[] { validationContext.MemberName! });
            }
        }

        return ValidationResult.Success;
    }
}