namespace Web.Essentials.App.ViewModels
{
    /// <summary>
    /// カテゴリ選択用ViewModel
    /// インターフェースとの互換性のために使用
    /// </summary>
    public class CategorySelectViewModel
    {
        /// <summary>
        /// カテゴリID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// カテゴリ名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 完全パス（階層構造を含む）
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// 階層レベル
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// インデント付きの表示名
        /// </summary>
        public string DisplayName => new string('　', Level) + Name;
    }
}