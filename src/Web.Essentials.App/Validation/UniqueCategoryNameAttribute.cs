using System.ComponentModel.DataAnnotations;
using Web.Essentials.App.Services;

namespace Web.Essentials.App.Validation;

/// <summary>
/// カテゴリ名重複チェック用バリデーション属性
/// </summary>
/// <remarks>
/// データベース内でのカテゴリ名重複をチェックし、一意性を保証する
/// 編集時は自分自身を除外して重複チェックを行う
/// </remarks>
public class UniqueCategoryNameAttribute : ValidationAttribute
{
    /// <summary>
    /// 除外対象のカテゴリIDプロパティ名
    /// </summary>
    /// <remarks>
    /// 編集時に自分自身のIDを除外するために使用
    /// 通常は "Id" を指定
    /// </remarks>
    public string ExcludeIdProperty { get; set; } = "Id";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public UniqueCategoryNameAttribute()
    {
        ErrorMessage = "このカテゴリ名は既に登録されています。";
    }

    /// <summary>
    /// バリデーション実行
    /// </summary>
    /// <param name="value">検証対象の値（カテゴリ名）</param>
    /// <param name="validationContext">バリデーションコンテキスト</param>
    /// <returns>バリデーション結果</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // カテゴリ名が未入力の場合はスキップ（必須チェックは別途実装）
        if (value == null || string.IsNullOrEmpty(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var categoryName = value.ToString()!;

        // CategoryServiceを取得
        var categoryService = validationContext.GetService<CategoryService>();
        if (categoryService == null)
        {
            throw new InvalidOperationException("CategoryService が DI コンテナに登録されていません。");
        }

        // 除外IDを取得（編集時の自分自身のID）
        int? excludeId = null;
        var excludeIdProperty = validationContext.ObjectType.GetProperty(ExcludeIdProperty);
        if (excludeIdProperty != null)
        {
            var excludeIdValue = excludeIdProperty.GetValue(validationContext.ObjectInstance);
            if (excludeIdValue != null && int.TryParse(excludeIdValue.ToString(), out var id) && id > 0)
            {
                excludeId = id;
            }
        }

        // 非同期処理を同期的に実行（バリデーション属性の制約）
        var isDuplicate = Task.Run(async () => 
            await categoryService.IsCategoryNameDuplicateAsync(categoryName, excludeId)).Result;

        if (isDuplicate)
        {
            return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}