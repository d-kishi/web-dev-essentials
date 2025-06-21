using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Essentials.App.Extensions;

/// <summary>
/// Enum型の拡張メソッド
/// </summary>
/// <remarks>
/// Enum値の表示テキスト取得やSelectListItem変換機能を提供
/// DisplayAttributeを使用したUI表示テキストの取得に対応
/// </remarks>
public static class EnumExtensions
{
    /// <summary>
    /// Enum値のDisplayAttribute.Nameを取得
    /// </summary>
    /// <param name="enumValue">Enum値</param>
    /// <returns>DisplayAttribute.Nameまたは Enum値の文字列表現</returns>
    /// <remarks>
    /// DisplayAttributeが定義されている場合はそのNameを返し、
    /// 定義されていない場合はEnum値の文字列表現を返す
    /// </remarks>
    public static string GetDisplayName(this Enum enumValue)
    {
        var enumType = enumValue.GetType();
        var memberInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
        
        if (memberInfo == null)
            return enumValue.ToString();
            
        var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? enumValue.ToString();
    }

    /// <summary>
    /// Enum型からSelectListItemのリストを生成
    /// </summary>
    /// <typeparam name="TEnum">Enum型</typeparam>
    /// <param name="selectedValue">選択されている値（任意）</param>
    /// <returns>SelectListItemのリスト</returns>
    /// <remarks>
    /// 指定されたEnum型のすべての値に対してSelectListItemを生成
    /// DisplayAttributeが定義されている場合はそのNameをTextとして使用
    /// </remarks>
    public static List<SelectListItem> ToSelectListItems<TEnum>(TEnum? selectedValue = null)
        where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);
        var selectItems = new List<SelectListItem>();

        foreach (TEnum enumValue in Enum.GetValues<TEnum>())
        {
            var isSelected = selectedValue.HasValue && selectedValue.Value.Equals(enumValue);
            
            selectItems.Add(new SelectListItem
            {
                Value = Convert.ToInt32(enumValue).ToString(),
                Text = enumValue.GetDisplayName(),
                Selected = isSelected
            });
        }

        return selectItems;
    }

    /// <summary>
    /// Enum型からSelectListItemのリストを生成（選択値なし）
    /// </summary>
    /// <typeparam name="TEnum">Enum型</typeparam>
    /// <returns>SelectListItemのリスト</returns>
    /// <remarks>
    /// 選択されている値を指定しない場合のオーバーロード
    /// </remarks>
    public static List<SelectListItem> ToSelectListItems<TEnum>()
        where TEnum : struct, Enum
    {
        return ToSelectListItems<TEnum>(null);
    }
}