/** 
 * ボタンがクリックされたときの処理
 * @param {Event} e - イベントオブジェクト
 */
function clickSubmit(e) {
  // クリックされたボタンのテキストを取得
  window.alert(`${e.target.innerText}ボタンがクリックされました。`);
  e.preventDefault(); // フォームの送信をキャンセル
}