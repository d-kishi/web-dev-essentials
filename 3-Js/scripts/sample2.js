/**
 * ユーザー情報を格納するクラス
 */
class User {
  constructor() {
    /** ユーザ名 */
    this.username = '';
    /** メールアドレス */
    this.email = '';
    /** パスワード */
    this.password = '';
    /** 生年月日 */
    this.birthdate = '';
    /** 都道府県 */
    this.prefecture = '';
    /** 市区町村 */
    this.city = '';
    /** 住所 */
    this.address = '';
    /** 性別 */
    this.gender = '';
  }

  /** 有効性確認 */
  validate() {
    const errors = [];
    
    if (!this.username) errors.push('username');
    if (!this.email) errors.push('email');
    if (!this.birthdate) errors.push('birthdate');
    if (!this.gender) errors.push('gender');
    if (!this.prefecture) errors.push('prefecture');
    if (!this.city) errors.push('city');
    if (!this.address) errors.push('address');
    if (!this.password) errors.push('password'); // パスワード検証追加
    
    return errors;
  }
}

// ユーザーオブジェクトの初期化
const user = new User();

/**
 * DOM読み込み完了後に実行
 * @remark
 * jQueryの`$(document).ready()`と同じタイミングで実行されます
 */
document.addEventListener('DOMContentLoaded', () => {
  /**
   * ユーザ名入力時の処理
   * @param {Event} e - イベントオブジェクト
   */
  document.querySelector('[name="username"]').addEventListener('input', (e) => {
    user.username = e.target.value;
    e.target.classList.remove('onError');
  });

  // メール入力時の処理
  document.querySelector('[name="email"]').addEventListener('input', (e) => {
    user.email = e.target.value;
    e.target.classList.remove('onError');
  });

  /**
   * パスワード入力時の処理
   * @param {Event} e - イベントオブジェクト
   */
  document.querySelector('[name="password"]').addEventListener('input', (e) => {
    user.password = e.target.value;
    e.target.classList.remove('onError');
  });

  /**
   * 生年月日入力時の処理
   * @param {Event} e - イベントオブジェクト
   */
  document.querySelector('[name="birthdate"]').addEventListener('input', (e) => {
    user.birthdate = e.target.value;
    e.target.classList.remove('onError');
  });

  /**
   * 都道府県入力時の処理
   * @param {Event} e - イベントオブジェクト
   */
  document.querySelector('[name="prefecture"]').addEventListener('change', (e) => {
    user.prefecture = e.target.value;
    e.target.classList.remove('onError');
  });

  /**
   * 市区町村入力時の処理
   * @param {Event} e - イベントオブジェクト
   */
  document.querySelector('[name="city"]').addEventListener('input', (e) => {
    user.city = e.target.value;
    e.target.classList.remove('onError');
  });

  /**
   * 住所入力時の処理
   * @param {Event} e - イベントオブジェクト
   */
  document.querySelector('[name="address"]').addEventListener('input', (e) => {
    user.address = e.target.value;
    e.target.classList.remove('onError');
  });

  /**
   * 性別選択時の処理
   * @param {Event} e - イベントオブジェクト
   */
  document.querySelector('[name="gender"]').addEventListener('change', (e) => {
    user.gender = e.target.value;
    e.target.classList.remove('onError');
  });

  /**
   * 登録ボタンクリック時の処理
   * @param {Event} e - イベントオブジェクト
   * @remark
   * 事例は「登録ボタンクリック」イベントをハンドルしているが、submitボタンなので以下のようにformのsubmitをハンドルしてもOK。
   * ```document.querySelector('.wrapper form').addEventListener('submit', (e) => { ... })```
   */
  document.querySelector('.wrapper form button[type="submit"]').addEventListener('click', (e) => {
    // フォーム送信を防止（※実際の実装ではこういう事はしない）
    e.preventDefault();
    
    // バリデーション実行
    const errors = user.validate();
    
    // エラークラスをリセット
    document.querySelectorAll('.onError').forEach(el => {
      el.classList.remove('onError');
    });
    
    // パスワード確認の一致検証
    const confirmPassword = document.querySelector('[name="confirm-password"]').value;
    let passwordMismatch = false;
    
    if (user.password && confirmPassword && user.password !== confirmPassword) {
      // パスワードと確認用パスワードが双方入力されいて、かつ一致しない場合
      document.querySelector('[name="password"]').classList.add('onError');
      document.querySelector('[name="confirm-password"]').classList.add('onError');
      passwordMismatch = true;
    }
    
    // エラーがある場合、該当要素にエラークラスを追加
    if (errors.length > 0) {
      errors.forEach(fieldName => {
        document.querySelector(`[name="${fieldName}"]`).classList.add('onError');
      });
    }
    
    // 結果の通知
    if (errors.length > 0 && passwordMismatch) {
      alert(`入力エラーが${errors.length}件あります。また、パスワードと確認用パスワードが一致しません。`);
    } else if (errors.length > 0) {
      alert(`入力エラーが${errors.length}件あります。`);
    } else if (passwordMismatch) {
      alert('パスワードと確認用パスワードが一致しません。');
    } else {
      alert('登録が完了しました！');
    }
  });
});