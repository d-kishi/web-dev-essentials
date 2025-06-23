/**
 * 削除確認画面用JavaScript
 * カテゴリ・商品削除画面で共通利用される削除確認機能を提供
 */

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeDeleteConfirmation();
});

/**
 * 削除確認の初期化
 * 各種機能の初期化を行う
 */
function initializeDeleteConfirmation() {
    // チェックボックスの状態監視
    setupConfirmationValidation();
    
    // フォーム送信の処理
    setupFormSubmission();
    
    // ページ離脱警告
    setupUnloadWarning();
    
    // ESCキーでキャンセル
    setupEscapeKeyHandler();
    
    // ボタンイベントリスナーの設定
    setupButtonEventListeners();
}

/**
 * 確認チェックボックスの検証設定
 */
function setupConfirmationValidation() {
    const confirmUnderstand = document.getElementById('confirmUnderstand');
    const confirmName = document.getElementById('confirmCategoryName') || document.getElementById('confirmProductName');
    const deleteButton = document.getElementById('deleteButton');
    
    if (!confirmUnderstand || !confirmName || !deleteButton) return;
    
    function validateConfirmations() {
        const allChecked = confirmUnderstand.checked && confirmName.checked;
        deleteButton.disabled = !allChecked;
        
        if (allChecked) {
            deleteButton.classList.remove('btn-disabled');
        } else {
            deleteButton.classList.add('btn-disabled');
        }
    }
    
    confirmUnderstand.addEventListener('change', validateConfirmations);
    confirmName.addEventListener('change', validateConfirmations);
}

/**
 * フォーム送信の設定
 */
function setupFormSubmission() {
    const deleteForm = document.getElementById('deleteForm');
    
    if (!deleteForm) return;
    
    deleteForm.addEventListener('submit', function(event) {
        event.preventDefault();
        
        // 最終確認ダイアログ
        showFinalConfirmation();
    });
}

/**
 * 最終確認ダイアログ
 */
function showFinalConfirmation() {
    // グローバル変数から取得（各ページで設定される）
    const itemName = window.deleteTargetName || '対象アイテム';
    const itemType = window.deleteTargetType || 'アイテム';
    const confirmMessage = `本当に${itemType}「${itemName}」を削除しますか？\n\nこの操作は元に戻すことができません。`;
    
    showConfirmationModal(
        '最終確認',
        confirmMessage,
        `削除後は${itemType}に関連するすべてのデータが失われます。`,
        '削除する',
        () => executeDelete()
    );
}

/**
 * 削除実行
 */
async function executeDelete() {
    try {
        const itemType = window.deleteTargetType || 'アイテム';
        showLoadingModal(`${itemType}を削除しています...\nしばらくお待ちください。`);
        
        const formData = new FormData(document.getElementById('deleteForm'));
        
        // 削除URLもグローバル変数から取得
        const deleteUrl = window.deleteUrl;
        if (!deleteUrl) {
            throw new Error('削除URLが設定されていません');
        }
        
        const response = await fetch(deleteUrl, {
            method: 'POST',
            body: formData
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            // 削除成功
            const itemName = window.deleteTargetName || '対象アイテム';
            const itemType = window.deleteTargetType || 'アイテム';
            const redirectUrl = window.deleteSuccessRedirectUrl || '/';
            
            showSuccessModal(
                '削除完了',
                `${itemType}「${itemName}」が正常に削除されました。`,
                () => {
                    window.location.href = redirectUrl;
                }
            );
        } else {
            // 削除失敗
            const errorText = await response.text();
            showError('削除に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('削除エラー:', error);
        showError('削除処理中にエラーが発生しました');
    }
}

/**
 * ページ離脱警告の設定
 */
function setupUnloadWarning() {
    let formInteracted = false;
    
    // フォーム操作の検知
    document.querySelectorAll('input, textarea').forEach(element => {
        element.addEventListener('input', () => {
            formInteracted = true;
        });
        element.addEventListener('change', () => {
            formInteracted = true;
        });
    });
    
    // ページ離脱時の警告
    window.addEventListener('beforeunload', function(event) {
        if (formInteracted) {
            event.preventDefault();
            event.returnValue = '削除確認画面から離れますか？';
        }
    });
}

/**
 * ESCキーでキャンセル
 */
function setupEscapeKeyHandler() {
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape') {
            const cancelUrl = window.deleteCancelUrl;
            if (cancelUrl && confirm('削除確認をキャンセルして詳細画面に戻りますか？')) {
                window.location.href = cancelUrl;
            }
        }
    });
}

/**
 * 成功モーダル表示
 * @param {string} title - モーダルタイトル
 * @param {string} message - メッセージ
 * @param {Function} callback - 確認ボタン押下時のコールバック
 */
function showSuccessModal(title, message, callback) {
    const modal = document.createElement('div');
    modal.className = 'modal-overlay success-modal';
    modal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h3 class="modal-title">${title}</h3>
                </div>
                <div class="modal-body">
                    <div class="success-icon">✅</div>
                    <p class="success-message">${message}</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary">
                        確認
                    </button>
                </div>
            </div>
        </div>
    `;
    
    // コールバック関数をグローバルに設定
    window.deleteSuccessCallback = callback;
    
    if (document.body) {
        document.body.appendChild(modal);
    }
    modal.style.display = 'block';
}

/**
 * エラーメッセージ表示
 * @param {string} message - エラーメッセージ
 */
function showError(message) {
    const errorContainer = document.createElement('div');
    errorContainer.className = 'alert alert-danger error-message';
    errorContainer.innerHTML = `
        <div class="alert-icon">❌</div>
        <div class="alert-content">
            <strong>エラー</strong>
            <p>${message}</p>
        </div>
        <button type="button" class="alert-close">×</button>
    `;
    
    // ページ上部に挿入
    const pageHeader = document.querySelector('.page-header');
    if (pageHeader) {
        pageHeader.insertAdjacentElement('afterend', errorContainer);
    } else {
        if (document.body && document.body.firstChild) {
            document.body.insertBefore(errorContainer, document.body.firstChild);
        }
    }
    
    // 自動削除（10秒後）
    setTimeout(() => {
        if (errorContainer.parentElement) {
            errorContainer.remove();
        }
    }, 10000);
}

/**
 * ボタンイベントリスナーの設定
 */
function setupButtonEventListeners() {
    // モーダル確認ボタン（委譲パターン）
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('btn') && 
            e.target.classList.contains('btn-primary') && 
            e.target.closest('.modal-overlay')) {
            const modal = e.target.closest('.modal-overlay');
            if (modal) {
                modal.remove();
                if (window.deleteSuccessCallback) {
                    window.deleteSuccessCallback();
                }
            }
        }
    });
    
    // アラート閉じるボタン（委譲パターン）
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('alert-close')) {
            const alertElement = e.target.closest('.alert');
            if (alertElement) {
                alertElement.remove();
            }
        }
    });
}