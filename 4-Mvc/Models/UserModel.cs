using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Essentials.Mvc.Models
{
    public class UserModel
    {
        [Required(ErrorMessage = "ユーザー名を入力してください")]
        [Display(Name = "ユーザー名")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "メールアドレスを入力してください")]
        [EmailAddress(ErrorMessage = "有効なメールアドレス形式で入力してください")]
        [Display(Name = "メールアドレス")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "パスワードを入力してください")]
        [StringLength(100, ErrorMessage = "パスワードは{2}文字以上{1}文字以下で入力してください", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "生年月日を入力してください")]
        [DataType(DataType.Date)]
        [Display(Name = "生年月日")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "性別を選択してください")]
        [Display(Name = "性別")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "住所情報を入力してください")]
        [Display(Name = "住所情報")]
        public AddressInfo Address { get; set; } = new AddressInfo();

        public List<string> Prefectures => new List<string>
        {
          "北海道", "青森県", "岩手県", "宮城県", "秋田県", "山形県", "福島県",
          "茨城県", "栃木県", "群馬県", "埼玉県", "千葉県", "東京都", "神奈川県",
          "新潟県", "富山県", "石川県", "福井県", "山梨県", "長野県", "岐阜県",
          "静岡県", "愛知県", "三重県", "滋賀県", "京都府", "大阪府", "兵庫県",
          "奈良県", "和歌山県", "鳥取県", "島根県", "岡山県", "広島県", "山口県",
          "徳島県", "香川県", "愛媛県", "高知県", "福岡県", "佐賀県", "長崎県",
          "熊本県", "大分県", "宮崎県", "鹿児島県", "沖縄県"
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
