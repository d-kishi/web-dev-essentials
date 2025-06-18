/**
 * カテゴリ一覧画面用JavaScript
 * 検索、階層表示、ソート、表示切替、削除機能を提供
 */

// 現在の検索条件を保持するグローバル変数
let currentSearchParams = {
    searchKeyword: '',
    level: null,
    parentId: null,
    page: 1,
    pageSize: 20,
    sortBy: 'name_asc'
};

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeCategoryList();
});

/**
 * カテゴリ一覧画面の初期化
 * 各種機能の初期化を行う
 */
function initializeCategoryList() {
    // 検索フォームの初期化
    setupSearchForm();
    
    // ソート機能の初期化
    setupSortFeatures();
    
    // ビュー切り替えの初期化
    setupViewToggle();
    
    // 階層表示の初期化
    setupTreeView();
    
    // カテゴリ削除の初期化
    setupCategoryDeletion();
}

/**
 * ビュー切り替え（階層/テーブル）
 * @param {string} viewType - 表示タイプ（'tree' または 'table'）
 */
function switchView(viewType) {
    const treeView = document.getElementById('treeView');
    const tableView = document.getElementById('tableView');
    const viewButtons = document.querySelectorAll('.view-toggle');
    
    // ボタンの状態更新
    viewButtons.forEach(btn => btn.classList.remove('active'));
    const activeButton = document.querySelector(`[data-view="${viewType}"]`);
    if (activeButton) activeButton.classList.add('active');
    
    // ビューの切り替え
    if (viewType === 'tree') {
        if (treeView) treeView.style.display = 'block';
        if (tableView) tableView.style.display = 'none';
    } else if (viewType === 'table') {
        if (treeView) treeView.style.display = 'none';
        if (tableView) tableView.style.display = 'block';
    }
}

/**
 * 階層ツリーをすべて展開
 */
function expandAll() {
    const collapsedItems = document.querySelectorAll('.tree-item.expandable.collapsed');
    collapsedItems.forEach(item => {
        toggleTreeItem(item);
    });
}

/**
 * 階層ツリーをすべて折りたたみ
 */
function collapseAll() {
    const expandedItems = document.querySelectorAll('.tree-item.expandable:not(.collapsed)');
    expandedItems.forEach(item => {
        toggleTreeItem(item);
    });
}

/**
 * 検索実行
 * フォームの入力値を取得して検索を実行
 */
function performSearch() {
    const searchKeyword = document.getElementById('searchKeyword').value;
    
    currentSearchParams.searchKeyword = searchKeyword;
    currentSearchParams.page = 1; // 検索時は1ページ目に戻る
    
    loadCategoryList();
}

/**
 * 検索条件リセット
 * 検索フォームをクリアしてデフォルト状態に戻す
 */
function resetSearch() {
    document.getElementById('searchForm').reset();
    currentSearchParams = {
        searchKeyword: '',
        level: null,
        parentId: null,
        page: 1,
        pageSize: currentSearchParams.pageSize,
        sortBy: 'name_asc'
    };
    loadCategoryList();
}

/**
 * ページ読み込み
 * @param {number} page - 読み込むページ番号
 */
function loadPage(page) {
    currentSearchParams.page = page;
    loadCategoryList();
}

/**
 * ページサイズ変更
 * @param {number} pageSize - 1ページあたりの表示件数
 */
function changePageSize(pageSize) {
    currentSearchParams.pageSize = parseInt(pageSize);
    currentSearchParams.page = 1; // 1ページ目に戻る
    loadCategoryList();
}

/**
 * ソート順変更
 * @param {string} sortBy - ソート項目（name_asc, level_asc等）
 */
function changeSortOrder(sortBy) {
    currentSearchParams.sortBy = sortBy;
    currentSearchParams.page = 1; // 1ページ目に戻る
    loadCategoryList();
}

/**
 * カテゴリ一覧をAjaxで読み込み
 * APIからカテゴリデータを取得して画面を更新
 */
async function loadCategoryList() {
    try {
        showLoading();
        
        const params = new URLSearchParams();
        if (currentSearchParams.searchKeyword) {
            params.append('nameTerm', currentSearchParams.searchKeyword);
        }
        if (currentSearchParams.level !== null) {
            params.append('level', currentSearchParams.level);
        }
        if (currentSearchParams.parentId !== null) {
            params.append('parentId', currentSearchParams.parentId);
        }
        params.append('includeProductCount', 'true');
        
        const response = await fetch(`/api/categories?${params.toString()}`);
        const result = await response.json();
        
        if (result.success) {
            updateCategoryList(result.data);
        } else {
            showError('カテゴリ一覧の取得に失敗しました: ' + result.message);
        }
    } catch (error) {
        console.error('カテゴリ一覧読み込みエラー:', error);
        showError('カテゴリ一覧の読み込み中にエラーが発生しました');
    } finally {
        hideLoading();
    }
}

/**
 * カテゴリ一覧の更新
 * APIから取得したデータで画面を更新
 * @param {Array} data - APIレスポンスデータ（カテゴリ配列）
 */
function updateCategoryList(data) {
    // 実装は後で詳細化（現在は簡易実装）
    console.log('Category list updated:', data);
}

/**
 * カテゴリ削除確認ダイアログの表示
 * @param {number} categoryId - 削除対象のカテゴリID
 * @param {string} categoryName - 削除対象のカテゴリ名
 * @param {number} productCount - 関連商品数
 * @param {boolean} hasChildren - 子カテゴリの有無
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
 * カテゴリ削除の実行
 * @param {number} categoryId - 削除対象のカテゴリID
 */
async function deleteCategory(categoryId) {
    try {
        const response = await fetch(`/Categories/Delete/${categoryId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });
        
        if (response.ok) {
            showSuccess('カテゴリが正常に削除されました');
            loadCategoryList(); // 一覧を再読み込み
        } else {
            const errorText = await response.text();
            showError('カテゴリの削除に失敗しました: ' + errorText);
        }
    } catch (error) {
        console.error('カテゴリ削除エラー:', error);
        showError('カテゴリ削除中にエラーが発生しました');
    }
}

/**
 * ローディング表示の表示
 */
function showLoading() {
    const loadingElement = document.getElementById('categoryListLoading');
    const contentElement = document.getElementById('categoryListContent');
    
    if (loadingElement) loadingElement.style.display = 'block';
    if (contentElement) contentElement.style.opacity = '0.5';
}

/**
 * ローディング表示の非表示
 */
function hideLoading() {
    const loadingElement = document.getElementById('categoryListLoading');
    const contentElement = document.getElementById('categoryListContent');
    
    if (loadingElement) loadingElement.style.display = 'none';
    if (contentElement) contentElement.style.opacity = '1';
}

/**
 * 階層表示機能の初期化
 */
function setupTreeView() {
    // ツリーアイテムのクリックイベント設定
    document.addEventListener('click', function(event) {
        const treeToggle = event.target.closest('.tree-toggle');
        if (treeToggle) {
            const treeItem = treeToggle.closest('.tree-item');
            toggleTreeItem(treeItem);
        }
    });
}

/**
 * ツリーアイテムの展開/折りたたみ切り替え
 * @param {HTMLElement} treeItem - 対象のツリーアイテム要素
 */
function toggleTreeItem(treeItem) {
    if (!treeItem) return;
    
    const isCollapsed = treeItem.classList.contains('collapsed');
    const children = treeItem.querySelector('.tree-children');
    const toggle = treeItem.querySelector('.tree-toggle');
    
    if (isCollapsed) {
        // 展開
        treeItem.classList.remove('collapsed');
        if (children) children.style.display = 'block';
        if (toggle) toggle.textContent = '▼';
    } else {
        // 折りたたみ
        treeItem.classList.add('collapsed');
        if (children) children.style.display = 'none';
        if (toggle) toggle.textContent = '▶';
    }
}

/**
 * 検索フォームの初期化
 */
function setupSearchForm() {
    const searchForm = document.getElementById('searchForm');
    if (searchForm) {
        searchForm.addEventListener('submit', function(e) {
            e.preventDefault();
            performSearch();
        });
    }
}

/**
 * ソート機能の初期化
 */
function setupSortFeatures() {
    const sortButtons = document.querySelectorAll('[data-sort]');
    sortButtons.forEach(button => {
        button.addEventListener('click', function() {
            const sortBy = this.getAttribute('data-sort');
            currentSearchParams.sortBy = sortBy;
            loadCategoryList();
        });
    });
}

/**
 * ビュー切り替え機能の初期化
 */
function setupViewToggle() {
    const viewButtons = document.querySelectorAll('.view-toggle');
    viewButtons.forEach(button => {
        button.addEventListener('click', function() {
            viewButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
        });
    });
}

/**
 * カテゴリ削除機能の初期化
 */
function setupCategoryDeletion() {
    // 削除ボタンのイベントハンドラーは動的コンテンツのため、
    // updateCategoryList内で設定
}

/**
 * カラムソート（テーブルヘッダークリック時）
 * @param {string} column - ソート対象のカラム名
 */
function sortBy(column) {
    // 現在のソート状態を確認して昇順/降順を切り替え
    const currentSort = currentSearchParams.sortBy;
    let newSort;
    
    if (currentSort === `${column}_asc`) {
        newSort = `${column}_desc`;
    } else {
        newSort = `${column}_asc`;
    }
    
    changeSortOrder(newSort);
}

// 初期パラメータ設定用の関数（Razorビューから呼び出される）
function setInitialSearchParams(params) {
    currentSearchParams = {
        ...currentSearchParams,
        ...params
    };
}