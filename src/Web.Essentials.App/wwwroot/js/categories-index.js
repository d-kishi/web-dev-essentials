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
    sortBy: null // ソート機能は使用しない
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
    
    // ソート機能は使用しないためコメントアウト
    // setupSortFeatures();
    
    // ビュー切り替え機能は使用しないためコメントアウト
    // setupViewToggle();
    
    // 階層表示の初期化
    setupTreeView();
    
    // カテゴリ削除の初期化
    setupCategoryDeletion();
    
    // 初期データの読み込み
    loadInitialData();
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
 * 初期データの読み込み
 * サーバーサイドから渡されたJSONデータを使用してツリー構造を生成
 */
function loadInitialData() {
    const dataScript = document.getElementById('initial-categories-data');
    if (dataScript) {
        try {
            const initialData = JSON.parse(dataScript.textContent);
            if (initialData && initialData.length > 0) {
                // 検索キーワードが空の場合として処理（通常表示）
                currentSearchParams.searchKeyword = '';
                updateCategoryList({ categories: initialData });
            }
        } catch (error) {
            console.error('初期データの読み込みエラー:', error);
        }
    }
}

// 階層構造表示では展開・縮小機能は使用しません

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
        sortBy: null // ソート機能は使用しない
    };
    loadCategoryList();
}

// ページング機能は階層表示では使用しないためコメントアウト

// ソート機能は使用しないためコメントアウト

/**
 * カテゴリ一覧をAjaxで読み込み
 * APIからカテゴリデータを取得して画面を更新
 */
async function loadCategoryList() {
    try {
        showLoading();
        
        const params = new URLSearchParams();
        if (currentSearchParams.searchKeyword) {
            params.append('searchKeyword', currentSearchParams.searchKeyword);
        }
        if (currentSearchParams.level !== null) {
            params.append('level', currentSearchParams.level);
        }
        if (currentSearchParams.parentId !== null) {
            params.append('parentId', currentSearchParams.parentId);
        }
        // ページングパラメーターは不要（階層表示では全データを取得）
        // params.append('page', currentSearchParams.page);
        // params.append('pageSize', currentSearchParams.pageSize);
        
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
 * @param {Object} data - APIレスポンスデータ
 */
function updateCategoryList(data) {
    const treeContainer = document.getElementById('categoryTree');
    if (!treeContainer || !data) return;
    
    // 検索結果の場合はdata.categories、そうでない場合は配列として扱う
    const categories = data.categories || data;
    
    if (!Array.isArray(categories) || categories.length === 0) {
        // データが0件の場合のメッセージ表示
        treeContainer.innerHTML = `
            <div class="no-data-message">
                <div class="no-data-icon">🏷️</div>
                <h3>カテゴリが見つかりません</h3>
                <p>検索条件を変更するか、新しいカテゴリを登録してください。</p>
                <a href="/Categories/Create" class="btn btn-primary">カテゴリを登録する</a>
            </div>
        `;
    } else {
        // カテゴリ一覧をツリー構造で表示
        treeContainer.innerHTML = '';
        
        // 検索結果と通常表示の両方で同じツリー構造を使用
        // カテゴリを階層構造に沿ってソート（親→子の順序）
        const sortedCategories = sortCategoriesHierarchically(categories);
        
        // 各カテゴリの深度を計算
        const categoryDepths = new Map();
        
        sortedCategories.forEach(category => {
            // 親カテゴリとのレベル差から深度を計算
            if (category.parentCategoryId) {
                const parentDepth = categoryDepths.get(category.parentCategoryId) || 0;
                categoryDepths.set(category.id, parentDepth + 1);
            } else {
                categoryDepths.set(category.id, 0);
            }
            
            const depth = categoryDepths.get(category.id);
            const treeNode = createHierarchyNode(category, depth, currentSearchParams.searchKeyword);
            treeContainer.appendChild(treeNode);
        });
        
        // 階層表示の再初期化
        setupTreeView();
    }
}

/**
 * カテゴリを階層構造に沿ってソート
 * Windowsエクスプローラのように親カテゴリの直後に子カテゴリが表示されるようにソート
 * @param {Array} categories - カテゴリ配列
 * @returns {Array} ソート済みカテゴリ配列
 */
function sortCategoriesHierarchically(categories) {
    if (!Array.isArray(categories) || categories.length === 0) {
        return [];
    }
    
    // カテゴリをIDでマップ化
    const categoryMap = new Map();
    categories.forEach(cat => {
        categoryMap.set(cat.id, { ...cat, children: [] });
    });
    
    // 子カテゴリを親に関連付け
    const rootCategories = [];
    categories.forEach(cat => {
        if (cat.parentCategoryId && categoryMap.has(cat.parentCategoryId)) {
            categoryMap.get(cat.parentCategoryId).children.push(categoryMap.get(cat.id));
        } else {
            rootCategories.push(categoryMap.get(cat.id));
        }
    });
    
    // 階層順でフラット化
    function flattenHierarchy(cats) {
        const result = [];
        cats.sort((a, b) => a.name.localeCompare(b.name, 'ja')); // 名前順にソート
        
        cats.forEach(cat => {
            result.push(cat);
            if (cat.children.length > 0) {
                result.push(...flattenHierarchy(cat.children));
            }
        });
        return result;
    }
    
    return flattenHierarchy(rootCategories);
}

/**
 * 階層表示用のツリーノードを作成（統一版）
 * @param {Object} category - カテゴリデータ
 * @param {number} depth - 階層の深さ
 * @param {string} searchKeyword - 検索キーワード（検索結果か通常表示かの判定用）
 * @returns {HTMLElement} 作成されたツリーノード要素
 */
function createHierarchyNode(category, depth = 0, searchKeyword = '') {
    const treeItem = document.createElement('div');
    
    // 検索結果か通常表示かでクラス名を決定
    if (searchKeyword) {
        treeItem.className = 'tree-item search-result';
    } else {
        treeItem.className = 'tree-item hierarchy-item';
    }
    
    treeItem.setAttribute('data-category-id', category.id);
    treeItem.setAttribute('data-level', category.level || 0);
    
    const hasChildren = category.hasChildren || false;
    
    // ツリー線の作成（Windowsエクスプローラ風）
    let treeLines = '';
    for (let i = 0; i < depth; i++) {
        if (i === depth - 1) {
            // 最後のレベルは分岐線
            treeLines += '<span class="tree-line tree-branch"></span>';
        } else {
            // 中間レベルは縦線
            treeLines += '<span class="tree-line tree-vertical"></span>';
        }
    }
    
    treeItem.innerHTML = `
        <div class="tree-item-content">
            <div class="tree-indent">
                ${treeLines}
            </div>
            <span class="tree-icon">${hasChildren ? '📂' : '🏷️'}</span>
            <div class="category-info">
                <div class="category-main">
                    <span class="level-badge">L${category.level || 0}</span>
                    <a href="/Categories/Details/${category.id}" class="category-name">${category.name}</a>
                    ${category.description ? `<span class="category-description">- ${category.description}</span>` : ''}
                </div>
                <div class="category-meta">
                    <span class="product-count">商品数: ${category.productCount || 0}</span>
                    <span class="updated-date">更新: ${new Date(category.updatedAt).toLocaleDateString('ja-JP')}</span>
                    <div class="category-actions">
                        <a href="/Categories/Edit/${category.id}" class="btn btn-sm btn-warning">編集</a>
                        <button type="button" class="btn btn-sm btn-danger" 
                                onclick="confirmDeleteCategory(${category.id}, '${category.name}', ${category.productCount || 0}, ${category.hasChildren ? 'true' : 'false'})"
                                ${(category.productCount > 0 || category.hasChildren) ? 'disabled' : ''}>
                            削除
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    return treeItem;
}

/**
 * 通常表示用のツリーノードを作成（旧版・互換性のため残す）
 * @param {Object} category - カテゴリデータ
 * @returns {HTMLElement} 作成されたツリーノード要素
 */
function createTreeNode(category, depth = 0) {
    const treeItem = document.createElement('div');
    treeItem.className = 'tree-item hierarchy-item';
    treeItem.setAttribute('data-category-id', category.id);
    treeItem.setAttribute('data-level', category.level || 0);
    
    const hasChildren = category.hasChildren || false;
    
    // ツリー線の作成（Windowsエクスプローラ風）
    let treeLines = '';
    for (let i = 0; i < depth; i++) {
        if (i === depth - 1) {
            // 最後のレベルは分岐線
            treeLines += '<span class="tree-line tree-branch"></span>';
        } else {
            // 中間レベルは縦線
            treeLines += '<span class="tree-line tree-vertical"></span>';
        }
    }
    
    treeItem.innerHTML = `
        <div class="tree-item-content">
            <div class="tree-indent">
                ${treeLines}
            </div>
            <span class="tree-icon">${hasChildren ? '📂' : '🏷️'}</span>
            <div class="category-info">
                <div class="category-main">
                    <span class="level-badge">L${category.level || 0}</span>
                    <a href="/Categories/Details/${category.id}" class="category-name">${category.name}</a>
                    ${category.description ? `<span class="category-description">- ${category.description}</span>` : ''}
                </div>
                <div class="category-meta">
                    <span class="product-count">商品数: ${category.productCount || 0}</span>
                    <span class="updated-date">更新: ${new Date(category.updatedAt).toLocaleDateString('ja-JP')}</span>
                    <div class="category-actions">
                        <a href="/Categories/Edit/${category.id}" class="btn btn-sm btn-warning">編集</a>
                        <button type="button" class="btn btn-sm btn-danger" 
                                onclick="confirmDeleteCategory(${category.id}, '${category.name}', ${category.productCount || 0}, ${category.hasChildren ? 'true' : 'false'})"
                                ${(category.productCount > 0 || category.hasChildren) ? 'disabled' : ''}>
                            削除
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    return treeItem;
}

/**
 * 検索結果用のツリーノードを作成（Windowsエクスプローラ風の階層表示）
 * @param {Object} category - カテゴリデータ
 * @returns {HTMLElement} 作成されたツリーノード要素
 */
function createSearchResultNode(category) {
    const treeItem = document.createElement('div');
    treeItem.className = 'tree-item search-result';
    treeItem.setAttribute('data-category-id', category.id);
    treeItem.setAttribute('data-level', category.level || 0);
    
    // 階層レベルに応じたインデントを作成
    const level = category.level || 0;
    const indentSpaces = '　'.repeat(level); // 全角スペースでインデント
    
    // ツリー線の作成（Windowsエクスプローラ風）
    let treeLines = '';
    for (let i = 0; i < level; i++) {
        if (i === level - 1) {
            // 最後のレベルは分岐線
            treeLines += '<span class="tree-line tree-branch"></span>';
        } else {
            // 中間レベルは縦線
            treeLines += '<span class="tree-line tree-vertical"></span>';
        }
    }
    
    treeItem.innerHTML = `
        <div class="tree-item-content">
            <div class="tree-indent">
                ${treeLines}
            </div>
            <span class="tree-icon">${category.hasChildren ? '📂' : '🏷️'}</span>
            <div class="category-info">
                <div class="category-main">
                    <span class="level-badge">L${level}</span>
                    <a href="/Categories/Details/${category.id}" class="category-name">${category.name}</a>
                    ${category.description ? `<span class="category-description">- ${category.description}</span>` : ''}
                </div>
                <div class="category-meta">
                    <span class="product-count">商品数: ${category.productCount || 0}</span>
                    <span class="updated-date">更新: ${new Date(category.updatedAt).toLocaleDateString('ja-JP')}</span>
                    <div class="category-actions">
                        <a href="/Categories/Edit/${category.id}" class="btn btn-sm btn-warning">編集</a>
                        <button type="button" class="btn btn-sm btn-danger" 
                                onclick="confirmDeleteCategory(${category.id}, '${category.name}', ${category.productCount || 0}, ${category.hasChildren ? 'true' : 'false'})"
                                ${(category.productCount > 0 || category.hasChildren) ? 'disabled' : ''}>
                            削除
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    return treeItem;
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
 * 階層構造表示では展開・縮小機能は使用しません
 */
function setupTreeView() {
    // 階層構造表示では特別な初期化処理は不要
}

// 階層構造表示では展開・縮小機能は使用しません

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

// ソート機能は使用しないためコメントアウト

// ビュー切り替え機能は使用しないためコメントアウト
// 階層表示のみを使用

/**
 * カテゴリ削除機能の初期化
 */
function setupCategoryDeletion() {
    // 削除ボタンのイベントハンドラーは動的コンテンツのため、
    // updateCategoryList内で設定
}

// ソート機能は使用しないためコメントアウト

// 初期パラメータ設定用の関数（Razorビューから呼び出される）
function setInitialSearchParams(params) {
    currentSearchParams = {
        ...currentSearchParams,
        ...params
    };
    
    // 階層表示では初期データはサーバーサイドレンダリングで表示されるため、
    // 自動的なデータ取得は行わない
}