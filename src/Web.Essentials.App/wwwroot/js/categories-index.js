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
    
    currentSearchParams.searchKeyword = searchKeyword.trim();
    currentSearchParams.page = 1; // 検索時は1ページ目に戻る
    
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
        level: null,
        parentId: null,
        page: 1,
        pageSize: currentSearchParams.pageSize,
        sortBy: null // ソート機能は使用しない
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
        
        const apiUrl = `/api/categories?${params.toString()}`;
        
        const response = await fetch(apiUrl);
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
        
        // 常に同じツリー構造表示処理を使用
        // 検索キーワードがある場合は、検索にマッチするカテゴリとその関連階層を表示
        // 検索キーワードがない場合は、全カテゴリを階層表示
        let displayCategories = categories;
        
        if (currentSearchParams.searchKeyword && currentSearchParams.searchKeyword.trim() !== '') {
            displayCategories = getSearchResultsWithHierarchy(categories, currentSearchParams.searchKeyword.trim());
        }
        
        // カテゴリを階層構造に沿ってソート（親→子の順序）
        const sortedCategories = sortCategoriesHierarchically(displayCategories);
        
        // 各カテゴリの深度を計算（検索結果でも正しい階層深度を維持）
        const categoryDepths = new Map();
        
        // 全カテゴリから深度を計算（検索結果に含まれていないカテゴリも考慮）
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
        
        // 元の全カテゴリリストを使用して正しい深度を計算
        sortedCategories.forEach(category => {
            const depth = calculateDepth(category.id, categories);
            
            // 常にcreateHierarchyNodeを使用してツリー構造を生成
            const treeNode = createHierarchyNode(category, depth, currentSearchParams.searchKeyword);
            treeContainer.appendChild(treeNode);
        });
        
        // 階層表示の再初期化
        setupTreeView();
    }
}

/**
 * 検索結果から関連する階層全体を取得
 * 検索にマッチするカテゴリとその親・子カテゴリをすべて含む階層構造を返す
 * @param {Array} allCategories - 全カテゴリ配列
 * @param {string} searchKeyword - 検索キーワード
 * @returns {Array} 関連階層を含むカテゴリ配列
 */
function getSearchResultsWithHierarchy(allCategories, searchKeyword) {
    if (!searchKeyword || searchKeyword.trim() === '') {
        return allCategories;
    }
    
    // 検索にマッチするカテゴリを特定
    const matchedCategories = allCategories.filter(category => 
        category.name.toLowerCase().includes(searchKeyword.toLowerCase()) ||
        (category.description && category.description.toLowerCase().includes(searchKeyword.toLowerCase()))
    );
    
    if (matchedCategories.length === 0) {
        return [];
    }
    
    // 関連する全カテゴリを収集するSet
    const relatedCategoryIds = new Set();
    
    // マッチしたカテゴリを追加
    matchedCategories.forEach(cat => relatedCategoryIds.add(cat.id));
    
    // マッチしたカテゴリの親階層をすべて追加
    function addAncestors(categoryId) {
        const category = allCategories.find(c => c.id === categoryId);
        
        if (category && category.parentCategoryId) {
            relatedCategoryIds.add(category.parentCategoryId);
            addAncestors(category.parentCategoryId);
        }
    }
    
    // マッチしたカテゴリの子孫をすべて追加
    function addDescendants(categoryId) {
        const children = allCategories.filter(c => c.parentCategoryId === categoryId);
        children.forEach(child => {
            relatedCategoryIds.add(child.id);
            addDescendants(child.id);
        });
    }
    
    // すべてのマッチカテゴリに対して親と子を追加
    matchedCategories.forEach(category => {
        addAncestors(category.id);
        addDescendants(category.id);
    });
    
    // 関連するすべてのカテゴリを返す
    return allCategories.filter(category => relatedCategoryIds.has(category.id));
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
                    <a href="/Categories/Details/${category.id}" class="category-name">${displayName}</a>
                    ${displayDescription ? `<span class="category-description">- ${displayDescription}</span>` : ''}
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