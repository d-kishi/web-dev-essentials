using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Web.Essentials.Mvc.Models
{
    /// <summary>
    /// ユーザ情報モデル
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// ユーザ名
        /// </summary>
        [Required(ErrorMessage = "ユーザー名を入力してください")]
        [StringLength(40, ErrorMessage = "ユーザ名は{1}文字以下で入力してください")]
        [Display(Name = "ユーザー名")]
        public string Username { get; set; } = "";

        /// <summary>
        /// メールアドレス
        /// </summary>
        [Required(ErrorMessage = "メールアドレスを入力してください")]
        [EmailAddress(ErrorMessage = "有効なメールアドレス形式で入力してください")]
        [StringLength(60, ErrorMessage = "メールアドレス名は{1}文字以下で入力してください")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "パスワードを入力してください")]
        [StringLength(20, ErrorMessage = "パスワードは{2}文字以上{1}文字以下で入力してください", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "確認パスワードを入力してください")]
        [StringLength(20, ErrorMessage = "確認パスワードは{2}文字以上{1}文字以下で入力してください", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "パスワードと確認パスワードが一致しません")]
        [Display(Name = "パスワード確認")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "生年月日を入力してください")]
        [DataType(DataType.Date)]
        [Display(Name = "生年月日")]
        public DateOnly? BirthDate { get; set; } = null;

        [Required(ErrorMessage = "性別を選択してください")]
        [Display(Name = "性別")]
        public Gender Gender { get; set; }

        [Display(Name = "住所情報")]
        public AddressInfo Address { get; set; } = new AddressInfo();

        public List<SelectListItem> Prefectures => new List<SelectListItem>
        {
            new SelectListItem { Value = null, Text = "選択してください" },
            new SelectListItem { Value = "1", Text = "北海道" },
            new SelectListItem { Value = "2", Text = "青森県" },
            new SelectListItem { Value = "3", Text = "岩手県" },
            new SelectListItem { Value = "4", Text = "宮城県" },
            new SelectListItem { Value = "5", Text = "秋田県" },
            new SelectListItem { Value = "6", Text = "山形県" },
            new SelectListItem { Value = "7", Text = "福島県" },
            new SelectListItem { Value = "8", Text = "茨城県" },
            new SelectListItem { Value = "9", Text = "栃木県" },
            new SelectListItem { Value = "10", Text = "群馬県" },
            new SelectListItem { Value = "11", Text = "埼玉県" },
            new SelectListItem { Value = "12", Text = "千葉県" },
            new SelectListItem { Value = "13", Text = "東京都" },
            new SelectListItem { Value = "14", Text = "神奈川県" },
            new SelectListItem { Value = "15", Text = "新潟県" },
            new SelectListItem { Value = "16", Text = "富山県" },
            new SelectListItem { Value = "17", Text = "石川県" },
            new SelectListItem { Value = "18", Text = "福井県" },
            new SelectListItem { Value = "19", Text = "山梨県" },
            new SelectListItem { Value = "20", Text = "長野県" },
            new SelectListItem { Value = "21", Text = "岐阜県" },
            new SelectListItem { Value = "22", Text = "静岡県" },
            new SelectListItem { Value = "23", Text = "愛知県" },
            new SelectListItem { Value = "24", Text = "三重県" },
            new SelectListItem { Value = "25", Text = "滋賀県" },
            new SelectListItem { Value = "26", Text = "京都府" },
            new SelectListItem { Value = "27", Text = "大阪府" },
            new SelectListItem { Value = "28", Text = "兵庫県" },
            new SelectListItem { Value = "29", Text = "奈良県" },
            new SelectListItem { Value = "30", Text = "和歌山県" },
            new SelectListItem { Value = "31", Text = "鳥取県" },
            new SelectListItem { Value = "32", Text = "島根県" },
            new SelectListItem { Value = "33", Text = "岡山県" },
            new SelectListItem { Value = "34", Text = "広島県" },
            new SelectListItem { Value = "35", Text = "山口県" },
            new SelectListItem { Value = "36", Text = "徳島県" },
            new SelectListItem { Value = "37", Text = "香川県" },
            new SelectListItem { Value = "38", Text = "愛媛県" },
            new SelectListItem { Value = "39", Text = "高知県" },
            new SelectListItem { Value = "40", Text = "福岡県" },
            new SelectListItem { Value = "41", Text = "佐賀県" },
            new SelectListItem { Value = "42", Text = "長崎県" },
            new SelectListItem { Value = "43", Text = "熊本県" },
            new SelectListItem { Value = "44", Text = "大分県" },
            new SelectListItem { Value = "45", Text = "宮崎県" },
            new SelectListItem { Value = "46", Text = "鹿児島県" },
            new SelectListItem { Value = "47", Text = "沖縄県" },
        };
    }

    public enum Gender
    {
        [Display(Name = "男性")]
        Male,
        [Display(Name = "女性")]
        Female
    }

    public class AddressInfo
    {
        [Required(ErrorMessage = "都道府県を選択してください")]
        [Display(Name = "都道府県")]
        public string Prefecture { get; set; } = "";

        [Required(ErrorMessage = "市区町村を入力してください")]
        [Display(Name = "市区町村")]
        public string City { get; set; } = "";

        [Required(ErrorMessage = "その他住所を入力してください")]
        [Display(Name = "その他住所")]
        public string AddressLine { get; set; } = "";
    }
}
