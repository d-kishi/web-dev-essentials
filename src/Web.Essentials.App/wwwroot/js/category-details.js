/**
 * カテゴリ詳細画面用JavaScript
 * カテゴリ詳細表示・削除・複製機能を提供
 */

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeCategoryDetails();
});

/**
 * カテゴリ詳細の初期化
 */
function initializeCategoryDetails() {
    // タブ機能の初期化
    setupTabs();
    
    // ボタンイベントハンドラーの設定
    setupButtonEventHandlers();
}

/**
 * タブ表示切り替え
 * @param {string} tabName - 表示するタブ名
 */
function showTab(tabName) {
    // すべてのタブボタンとコンテンツを非アクティブに
    document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
    
    // 選択されたタブをアクティブに
    const tabButton = document.querySelector(`[data-tab="${tabName}"]`);
    const tabContent = document.getElementById(tabName + 'Tab');
    
    if (tabButton) tabButton.classList.add('active');
    if (tabContent) tabContent.classList.add('active');
}

/**
 * カテゴリ削除確認
 * @param {number} categoryId - カテゴリID
 * @param {string} categoryName - カテゴリ名
 * @param {number} productCount - 関連商品数
 * @param {boolean} hasChildren - 子カテゴリ存在フラグ
 */
function confirmDeleteCategory(categoryId, categoryName, productCount, hasChildren) {
    if (productCount > 0 || hasChildren) {
        let message = '';
        if (productCount > 0 && hasChildren) {
            message = `カテゴリ「${categoryName}」には${productCount}個の商品と子カテゴリが存在するため削除できません。`;
        } else if (productCount > 0) {
            message = `カテゴリ「${categoryName}」には${productCount}個の商品が存在するため削除できません。`;
        } else if (hasChildren) {
            message = `カテゴリ「${categoryName}」には子カテゴリが存在するため削除できません。`;
        }
        
        showError(message + '\n関連する商品や子カテゴリを先に削除または移動してください。');
        return;
    }
    
    showConfirmationModal(
        'カテゴリ削除の確認',
        `カテゴリ「${categoryName}」を削除しますか？`,
        '削除すると元に戻せません。',
        '削除',
        () => deleteCategory(categoryId)
    );
}

/**
 * カテゴリ削除実行
 * @param {number} categoryId - カテゴリID
 */
async function deleteCategory(categoryId) {
    try {
        showLoadingModal('カテゴリを削除しています...');
        
        const response = await fetch(`/Categories/Delete/${categoryId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            showSuccess('カテゴリが正常に削除されました');
            
            // カテゴリ一覧に戻る
            setTimeout(() => {
                window.location.href = '/Categories';
            }, 1500);
        } else {
            const errorText = await response.text();
            showError('カテゴリの削除に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('カテゴリ削除エラー:', error);
        showError('カテゴリ削除中にエラーが発生しました');
    }
}

/**
 * カテゴリ複製
 * @param {number} categoryId - カテゴリID
 */
async function duplicateCategory(categoryId) {
    try {
        showLoadingModal('カテゴリを複製しています...');
        
        const response = await fetch(`/Categories/Duplicate/${categoryId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            const result = await response.json();
            showSuccess('カテゴリが複製されました');
            
            // 複製されたカテゴリの編集画面に移動
            setTimeout(() => {
                window.location.href = `/Categories/Edit/${result.newCategoryId}`;
            }, 1500);
        } else {
            const errorText = await response.text();
            showError('カテゴリの複製に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('カテゴリ複製エラー:', error);
        showError('カテゴリ複製中にエラーが発生しました');
    }
}

/**
 * ボタンイベントハンドラーの設定
 */
function setupButtonEventHandlers() {
    // 削除ボタン（IDとdata属性ベース）
    const deleteButton = document.getElementById('deleteButton');
    if (deleteButton) {
        deleteButton.addEventListener('click', () => {
            const categoryId = deleteButton.dataset.categoryId;
            const categoryName = deleteButton.dataset.categoryName;
            const productCount = deleteButton.dataset.productCount;
            const hasChildren = deleteButton.dataset.hasChildren;
            
            confirmDeleteCategory(
                parseInt(categoryId), 
                categoryName, 
                parseInt(productCount), 
                hasChildren === 'true'
            );
        });
    }
    
    // 複製ボタン（IDとdata属性ベース）
    const duplicateButton = document.getElementById('duplicateButton');
    if (duplicateButton) {
        duplicateButton.addEventListener('click', () => {
            const categoryId = duplicateButton.dataset.categoryId;
            duplicateCategory(parseInt(categoryId));
        });
    }
    
    // タブボタン（クラスとdata属性ベース）
    const tabButtons = document.querySelectorAll('.tab-button');
    tabButtons.forEach(button => {
        const tabName = button.dataset.tab;
        if (tabName) {
            button.addEventListener('click', () => showTab(tabName));
        }
    });
}

/**
 * タブ機能の初期化
 */
function setupTabs() {
    // 初期タブを表示
    const activeTab = document.querySelector('.tab-button.active');
    if (activeTab) {
        const tabName = activeTab.getAttribute('data-tab');
        if (tabName) {
            showTab(tabName);
        }
    }
}

// 互換性のためのグローバル関数公開（段階的に削除予定）
// 新しいID/class/data属性アプローチに移行済み
window.showTab = showTab;
window.confirmDeleteCategory = confirmDeleteCategory;
window.deleteCategory = deleteCategory;
window.duplicateCategory = duplicateCategory;