/**
 * カテゴリ一覧画面用JavaScript
 * 検索、階層表示、ソート、表示切替、削除機能を提供
 */

// 現在の検索条件を保持するグローバル変数
let currentSearchParams = {
    searchKeyword: '',
    deletableOnly: false
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
 * 初期表示でも検索・リセットと同じ処理フロー（APIから取得）を使用
 */
function loadInitialData() {
    // 検索キーワードを明示的に空にして無条件検索を実行
    currentSearchParams.searchKeyword = '';
    
    // 初期表示でも検索・リセットと同じ処理フロー（APIから取得）
    loadCategoryList();
}

// 階層構造表示では展開・縮小機能は使用しません

/**
 * 検索実行
 * フォームの入力値を取得して検索を実行
 */
function performSearch() {
    const searchKeyword = document.getElementById('searchKeyword').value || '';
    const deletableFilter = document.getElementById('deletableFilter');
    const deletableOnly = deletableFilter ? deletableFilter.value === 'true' : false;
    
    currentSearchParams.searchKeyword = searchKeyword.trim();
    currentSearchParams.deletableOnly = deletableOnly;
    
    // キーワード検索、無条件検索に関わらず同じ処理フローでAPIから取得
    loadCategoryList();
}

/**
 * 検索条件リセット
 * 検索フォームをクリアして無条件検索を実行
 */
function resetSearch() {
    const searchForm = document.getElementById('searchForm');
    if (searchForm) {
        searchForm.reset();
    }
    
    currentSearchParams = {
        searchKeyword: '',
        deletableOnly: false
    };
    
    // リセット時は無条件検索と同じ処理フロー（APIから取得）
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
        
        // 検索キーワードがある場合のみパラメータに追加
        if (currentSearchParams.searchKeyword && currentSearchParams.searchKeyword.trim() !== '') {
            params.append('searchKeyword', currentSearchParams.searchKeyword.trim());
        }
        
        // 削除可能フィルターがtrueの場合のみパラメータに追加
        if (currentSearchParams.deletableOnly) {
            params.append('deletableOnly', 'true');
        }
        
        const apiUrl = `/api/categories?${params.toString()}`;
        
        const response = await fetch(apiUrl);
        const result = await response.json();
        
        if (result.success) {
            updateCategoryList(result.data);
        } else {
            showApiError(result.message || 'カテゴリ一覧の取得に失敗しました');
        }
    } catch (error) {
        console.error('カテゴリ一覧読み込みエラー:', error);
        showApiError(error);
    } finally {
        hideLoading();
    }
}

/**
 * カテゴリ一覧の更新
 * APIから取得したデータで画面を更新
 * @param {Array} categories - APIから取得したカテゴリ配列
 */
function updateCategoryList(categories) {
    const treeContainer = document.getElementById('categoryTree');
    if (!treeContainer) return;
    
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
        
        // 検索統計の更新（0件）
        updateSearchStatsDisplay(0, currentSearchParams.searchKeyword);
    } else {
        // カテゴリ一覧をツリー構造で表示
        treeContainer.innerHTML = '';
        
        // バックエンドから取得したカテゴリをそのまま表示
        // （バックエンドでフィルタリング済み）
        const sortedCategories = sortCategoriesHierarchically(categories);
        
        // 各カテゴリの深度を計算
        const categoryDepths = new Map();
        
        function calculateDepth(categoryId, allCategoriesForDepth) {
            if (categoryDepths.has(categoryId)) {
                return categoryDepths.get(categoryId);
            }
            
            const category = allCategoriesForDepth.find(c => c.id === categoryId);
            if (!category || !category.parentCategoryId) {
                categoryDepths.set(categoryId, 0);
                return 0;
            }
            
            const parentDepth = calculateDepth(category.parentCategoryId, allCategoriesForDepth);
            const depth = parentDepth + 1;
            categoryDepths.set(categoryId, depth);
            return depth;
        }
        
        // 各カテゴリのツリーノードを作成
        sortedCategories.forEach(category => {
            const depth = calculateDepth(category.id, categories);
            const treeNode = createHierarchyNode(category, depth, currentSearchParams.searchKeyword);
            treeContainer.appendChild(treeNode);
        });
        
        // 階層表示の再初期化
        setupTreeView();
        
        // 動的に生成されたボタンのイベントリスナーを設定
        setupDynamicEventHandlers();
        
        // 検索統計の更新
        updateSearchStatsDisplay(categories.length, currentSearchParams.searchKeyword);
    }
}

/**
 * 検索統計の更新
 * @param {string} searchValue - 検索キーワード
 */
function updateSearchStats(searchValue) {
    // 実際の検索統計はupdateCategoryList内で呼ばれるupdateSearchStatsDisplayで行う
    // この関数は検索開始時の統計表示制御用
    const searchStatsElement = document.getElementById('searchStats');
    if (searchStatsElement) {
        if (searchValue && searchValue.trim() !== '') {
            searchStatsElement.style.display = 'block';
        } else {
            searchStatsElement.style.display = 'none';
        }
    }
}

/**
 * 検索統計表示の更新
 * @param {number} resultCount - 検索結果件数
 * @param {string} searchKeyword - 検索キーワード
 */
function updateSearchStatsDisplay(resultCount, searchKeyword) {
    const searchStatsElement = document.getElementById('searchStats');
    const searchStatsText = document.getElementById('searchStatsText');
    const searchTime = document.getElementById('searchTime');
    
    if (searchStatsElement && searchStatsText) {
        if (searchKeyword && searchKeyword.trim() !== '') {
            // 検索キーワードがある場合は統計を表示
            searchStatsText.textContent = `検索結果: ${resultCount}件`;
            if (searchTime) {
                const now = new Date();
                searchTime.textContent = `(${now.toLocaleTimeString('ja-JP')})`;
            }
            searchStatsElement.style.display = 'block';
        } else {
            // 検索キーワードがない場合は統計を非表示
            searchStatsElement.style.display = 'none';
        }
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
    
    // 常に階層構造表示クラスを適用（検索結果でもツリー構造を維持）
    treeItem.className = 'tree-item hierarchy-item';
    if (searchKeyword && searchKeyword.trim() !== '') {
        // 検索結果の場合は追加クラスを付与
        treeItem.classList.add('search-result');
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
    
    // 検索キーワードがある場合は、マッチするカテゴリのみハイライト処理を適用
    let displayName = category.name;
    let displayDescription = category.description || '';
    
    if (searchKeyword && searchKeyword.trim() !== '') {
        // このカテゴリが検索キーワードにマッチするかチェック
        const keyword = searchKeyword.trim().toLowerCase();
        const nameMatches = category.name.toLowerCase().includes(keyword);
        const descMatches = category.description && category.description.toLowerCase().includes(keyword);
        
        if (nameMatches) {
            displayName = highlightSearchTerm(category.name, searchKeyword.trim());
        }
        
        if (descMatches && category.description) {
            displayDescription = highlightSearchTerm(category.description, searchKeyword.trim());
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
                    <a href="/Categories/Edit/${category.id}" class="category-name">${displayName}</a>
                    ${displayDescription ? `<span class="category-description">- ${displayDescription}</span>` : ''}
                </div>
                <div class="category-meta">
                    <span class="product-count">商品数: ${category.productCount || 0}</span>
                    <span class="updated-date">更新: ${new Date(category.updatedAt).toLocaleDateString('ja-JP')}</span>
                    <div class="category-actions">
                        <a href="/Categories/Edit/${category.id}" class="btn btn-sm btn-warning">編集</a>
                        <button type="button" class="btn btn-sm btn-danger category-delete-button" 
                                data-category-id="${category.id}" 
                                data-category-name="${category.name}" 
                                data-product-count="${category.productCount || 0}" 
                                data-has-children="${category.hasChildren ? 'true' : 'false'}"
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
                    <a href="/Categories/Edit/${category.id}" class="category-name">${category.name}</a>
                    ${category.description ? `<span class="category-description">- ${category.description}</span>` : ''}
                </div>
                <div class="category-meta">
                    <span class="product-count">商品数: ${category.productCount || 0}</span>
                    <span class="updated-date">更新: ${new Date(category.updatedAt).toLocaleDateString('ja-JP')}</span>
                    <div class="category-actions">
                        <a href="/Categories/Edit/${category.id}" class="btn btn-sm btn-warning">編集</a>
                        <button type="button" class="btn btn-sm btn-danger category-delete-button" 
                                data-category-id="${category.id}" 
                                data-category-name="${category.name}" 
                                data-product-count="${category.productCount || 0}" 
                                data-has-children="${category.hasChildren ? 'true' : 'false'}"
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
                    <a href="/Categories/Edit/${category.id}" class="category-name">${category.name}</a>
                    ${category.description ? `<span class="category-description">- ${category.description}</span>` : ''}
                </div>
                <div class="category-meta">
                    <span class="product-count">商品数: ${category.productCount || 0}</span>
                    <span class="updated-date">更新: ${new Date(category.updatedAt).toLocaleDateString('ja-JP')}</span>
                    <div class="category-actions">
                        <a href="/Categories/Edit/${category.id}" class="btn btn-sm btn-warning">編集</a>
                        <button type="button" class="btn btn-sm btn-danger category-delete-button" 
                                data-category-id="${category.id}" 
                                data-category-name="${category.name}" 
                                data-product-count="${category.productCount || 0}" 
                                data-has-children="${category.hasChildren ? 'true' : 'false'}"
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
 * カテゴリ削除確認モーダルの表示
 * @param {number} categoryId - 削除対象のカテゴリID
 * @param {string} categoryName - 削除対象のカテゴリ名
 * @param {number} productCount - 関連商品数
 * @param {boolean} hasChildren - 子カテゴリの有無
 */
function confirmDeleteCategory(categoryId, categoryName, productCount, hasChildren) {
    const modal = document.getElementById('deleteCategoryModal');
    const categoryNameElement = document.getElementById('deleteCategoryName');
    const productCountElement = document.getElementById('deleteCategoryProductCount');
    const warningSection = document.querySelector('.delete-warning');
    const warningList = document.getElementById('deleteWarningList');
    const confirmButton = document.getElementById('confirmDeleteButton');
    
    if (!modal || !categoryNameElement || !productCountElement) {
        console.error('削除モーダルの要素が見つかりません');
        return;
    }
    
    // カテゴリ情報を設定
    categoryNameElement.textContent = categoryName;
    productCountElement.textContent = `${productCount}個`;
    
    // 削除可能かチェック
    const canDelete = productCount === 0 && !hasChildren;
    
    if (!canDelete) {
        // 削除不可の場合：警告表示、削除ボタン無効化
        const warnings = [];
        if (productCount > 0) {
            warnings.push(`${productCount}個の商品が存在します`);
        }
        if (hasChildren) {
            warnings.push('子カテゴリが存在します');
        }
        
        warningList.innerHTML = warnings.map(warning => `<li>${warning}</li>`).join('');
        warningSection.style.display = 'block';
        confirmButton.disabled = true;
        confirmButton.classList.add('disabled');
    } else {
        // 削除可能の場合：警告非表示、削除ボタン有効化
        warningSection.style.display = 'none';
        confirmButton.disabled = false;
        confirmButton.classList.remove('disabled');
        
        // 削除ボタンのクリックイベントを設定（前のイベントをクリア）
        const newConfirmButton = confirmButton.cloneNode(true);
        confirmButton.parentNode.replaceChild(newConfirmButton, confirmButton);
        newConfirmButton.addEventListener('click', () => {
            hideDeleteModal();
            deleteCategory(categoryId);
        });
    }
    
    // モーダル表示
    modal.style.display = 'block';
}

/**
 * 削除確認モーダルを隠す
 */
function hideDeleteModal() {
    const modal = document.getElementById('deleteCategoryModal');
    if (modal) {
        modal.style.display = 'none';
    }
}

/**
 * カテゴリ削除の実行
 * @param {number} categoryId - 削除対象のカテゴリID
 */
async function deleteCategory(categoryId) {
    try {
        // FormDataでAntiforgery tokenを送信
        const formData = new FormData();
        const token = getAntiForgeryToken();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }
        
        const response = await fetch(`/Categories/Delete/${categoryId}`, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'application/json'
            },
            body: formData
        });
        
        // レスポンスがJSONかチェック
        const contentType = response.headers.get('content-type');
        let result;
        
        if (contentType && contentType.includes('application/json')) {
            result = await response.json();
        } else {
            // JSONでない場合はテキストとして取得
            const text = await response.text();
            console.error('Expected JSON response but got:', text);
            
            if (response.ok) {
                result = { success: true, message: 'カテゴリが正常に削除されました' };
            } else {
                result = { success: false, message: `削除に失敗しました (${response.status})` };
            }
        }
        
        if (result.success) {
            showSuccess(result.message || 'カテゴリが正常に削除されました');
            loadCategoryList(); // 一覧を再読み込み
        } else {
            showError(result.message || 'カテゴリの削除に失敗しました');
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

/**
 * 検索語句をハイライト表示
 * @param {string} text - 対象テキスト
 * @param {string} searchTerm - 検索語句
 * @returns {string} ハイライト済みHTML
 */
function highlightSearchTerm(text, searchTerm) {
    if (!text || !searchTerm) {
        return text;
    }
    
    const regex = new RegExp(`(${searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
    return text.replace(regex, '<mark class="search-highlight">$1</mark>');
}

// 階層構造表示では展開・縮小機能は使用しません

/**
 * 検索フォームの初期化
 */
function setupSearchForm() {
    const searchForm = document.getElementById('searchForm');
    const searchInput = document.getElementById('searchKeyword');
    
    if (searchForm) {
        searchForm.addEventListener('submit', function(e) {
            e.preventDefault();
            performSearch();
        });
    }
    
    // リアルタイム検索の設定
    if (searchInput) {
        setupRealtimeSearch(searchInput);
    }
    
    // 検索・リセットボタンの設定
    const searchBtn = document.querySelector('[data-action="perform-search"]');
    const resetBtn = document.querySelector('[data-action="reset-search"]');
    
    if (searchBtn) {
        searchBtn.addEventListener('click', performSearch);
    }
    
    if (resetBtn) {
        resetBtn.addEventListener('click', resetSearch);
    }
}

/**
 * リアルタイム検索の設定
 * 入力中のデバウンス処理でリアルタイム検索を実現
 * @param {HTMLInputElement} searchInput - 検索入力フィールド
 */
function setupRealtimeSearch(searchInput) {
    let debounceTimer = null;
    const debounceDelay = 500; // 500ms後に検索実行
    
    searchInput.addEventListener('input', function(e) {
        const searchValue = e.target.value.trim();
        
        // 前回のタイマーをクリア
        if (debounceTimer) {
            clearTimeout(debounceTimer);
        }
        
        // 新しいタイマーを設定
        debounceTimer = setTimeout(() => {
            // 検索キーワードが変更された場合のみ検索実行
            if (currentSearchParams.searchKeyword !== searchValue) {
                currentSearchParams.searchKeyword = searchValue;
                loadCategoryList();
                
                // 検索統計の更新
                updateSearchStats(searchValue);
            }
        }, debounceDelay);
    });
    
    // Enterキー押下時は即座に検索実行
    searchInput.addEventListener('keydown', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }
            performSearch();
        }
    });
}

// ソート機能は使用しないためコメントアウト

// ビュー切り替え機能は使用しないためコメントアウト
// 階層表示のみを使用

/**
 * カテゴリ削除機能の初期化
 */
function setupCategoryDeletion() {
    // モーダルのクローズボタンのイベントハンドラー設定
    const closeButtons = document.querySelectorAll('[data-action="close-delete-modal"]');
    closeButtons.forEach(button => {
        button.addEventListener('click', hideDeleteModal);
    });
    
    // モーダルの背景クリックで閉じる
    const modal = document.getElementById('deleteCategoryModal');
    if (modal) {
        modal.addEventListener('click', function(e) {
            if (e.target === modal) {
                hideDeleteModal();
            }
        });
    }
    
    // 削除ボタンのイベントハンドラーは動的コンテンツのため、
    // updateCategoryList内で設定
}

/**
 * 動的に生成されたボタンのイベントハンドラーを設定
 */
function setupDynamicEventHandlers() {
    // カテゴリ削除ボタン
    const deleteButtons = document.querySelectorAll('.category-delete-button');
    deleteButtons.forEach(button => {
        const categoryId = parseInt(button.dataset.categoryId);
        const categoryName = button.dataset.categoryName;
        const productCount = parseInt(button.dataset.productCount);
        const hasChildren = button.dataset.hasChildren === 'true';
        
        // 既存のイベントリスナーを削除（重複を避けるため）
        button.replaceWith(button.cloneNode(true));
        const newButton = document.querySelector(`[data-category-id="${categoryId}"]`);
        
        if (newButton) {
            newButton.addEventListener('click', () => {
                confirmDeleteCategory(categoryId, categoryName, productCount, hasChildren);
            });
        }
    });
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