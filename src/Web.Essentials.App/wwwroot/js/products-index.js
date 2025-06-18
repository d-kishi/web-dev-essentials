/**
 * 商品一覧画面用JavaScript
 * 検索、ページング、ソート、表示切替、削除機能を提供
 */

// 現在の検索条件を保持するグローバル変数
let currentSearchParams = {
    searchKeyword: '',
    categoryId: null,
    page: 1,
    pageSize: 10,
    sortBy: 'updated_desc'
};

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeProductList();
});

/**
 * 商品一覧画面の初期化
 * 各種機能の初期化を行う
 */
function initializeProductList() {
    // 検索フォームの初期化
    setupSearchForm();
    
    // ソート機能の初期化
    setupSortFeatures();
    
    // ビュー切り替えの初期化
    setupViewToggle();
    
    // 商品削除の初期化
    setupProductDeletion();
}

/**
 * 検索実行
 * フォームの入力値を取得して検索を実行
 */
function performSearch() {
    const searchKeyword = document.getElementById('searchKeyword').value;
    const categoryId = document.getElementById('categoryFilter').value;
    
    currentSearchParams.searchKeyword = searchKeyword;
    currentSearchParams.categoryId = categoryId || null;
    currentSearchParams.page = 1; // 検索時は1ページ目に戻る
    
    loadProductList();
}

/**
 * 検索条件リセット
 * 検索フォームをクリアしてデフォルト状態に戻す
 */
function resetSearch() {
    document.getElementById('searchForm').reset();
    currentSearchParams = {
        searchKeyword: '',
        categoryId: null,
        page: 1,
        pageSize: currentSearchParams.pageSize,
        sortBy: 'updated_desc'
    };
    loadProductList();
}

/**
 * ページ読み込み
 * @param {number} page - 読み込むページ番号
 */
function loadPage(page) {
    currentSearchParams.page = page;
    loadProductList();
}

/**
 * ページサイズ変更
 * @param {number} pageSize - 1ページあたりの表示件数
 */
function changePageSize(pageSize) {
    currentSearchParams.pageSize = parseInt(pageSize);
    currentSearchParams.page = 1; // 1ページ目に戻る
    loadProductList();
}

/**
 * ソート順変更
 * @param {string} sortBy - ソート項目（updated_desc, name_asc等）
 */
function changeSortOrder(sortBy) {
    currentSearchParams.sortBy = sortBy;
    currentSearchParams.page = 1; // 1ページ目に戻る
    loadProductList();
}

/**
 * 商品一覧をAjaxで読み込み
 * APIから商品データを取得して画面を更新
 */
async function loadProductList() {
    try {
        showLoading();
        
        const params = new URLSearchParams();
        if (currentSearchParams.searchKeyword) {
            params.append('searchKeyword', currentSearchParams.searchKeyword);
        }
        if (currentSearchParams.categoryId) {
            params.append('categoryId', currentSearchParams.categoryId);
        }
        params.append('page', currentSearchParams.page);
        params.append('pageSize', currentSearchParams.pageSize);
        params.append('sortBy', currentSearchParams.sortBy);
        
        const response = await fetch(`/api/products?${params.toString()}`);
        const result = await response.json();
        
        if (result.success) {
            updateProductList(result.data);
        } else {
            showError('商品一覧の取得に失敗しました: ' + result.message);
        }
    } catch (error) {
        console.error('商品一覧読み込みエラー:', error);
        showError('商品一覧の読み込み中にエラーが発生しました');
    } finally {
        hideLoading();
    }
}

/**
 * 商品一覧の更新
 * APIから取得したデータで画面を更新
 * @param {Object} data - APIレスポンスデータ
 */
function updateProductList(data) {
    const container = document.getElementById('productListContent');
    if (!container || !data) return;
    
    // テーブルの更新
    const tbody = container.querySelector('table tbody');
    if (tbody && data.products) {
        tbody.innerHTML = '';
        data.products.forEach(product => {
            const row = createProductRow(product);
            tbody.appendChild(row);
        });
    }
    
    // ページング情報の更新
    if (data.paging) {
        updatePagination(data.paging);
    }
}

/**
 * 商品行のDOM要素を作成
 * @param {Object} product - 商品データ
 * @returns {HTMLElement} 作成されたtr要素
 */
function createProductRow(product) {
    const row = document.createElement('tr');
    row.className = 'product-row';
    row.setAttribute('data-product-id', product.id);
    
    row.innerHTML = `
        <td class="col-thumbnail">
            <div class="thumbnail-container">
                <img src="/images/no-image.png" alt="商品画像" class="product-thumbnail" />
            </div>
        </td>
        <td class="col-name">
            <div class="product-name">
                <a href="/Products/Details/${product.id}" class="product-link">
                    ${product.name}
                </a>
            </div>
            ${product.description ? `<div class="product-description">${product.description.length > 50 ? product.description.substring(0, 50) + '...' : product.description}</div>` : ''}
        </td>
        <td class="col-category">
            <span class="category-badge">${product.categories ? product.categories.map(c => c.name).join(', ') : ''}</span>
        </td>
        <td class="col-price">
            <span class="price-value">¥${product.price.toLocaleString()}</span>
        </td>
        <td class="col-jan">
            ${product.janCode ? `<code class="jan-code">${product.janCode}</code>` : '<span class="no-data">-</span>'}
        </td>
        <td class="col-updated">
            <time class="updated-time" datetime="${new Date(product.updatedAt).toISOString().split('T')[0]}">
                ${new Date(product.updatedAt).toLocaleDateString('ja-JP')}
            </time>
        </td>
        <td class="col-actions">
            <div class="action-buttons">
                <a href="/Products/Details/${product.id}" class="btn btn-info btn-sm" title="詳細表示">詳細</a>
                <a href="/Products/Edit/${product.id}" class="btn btn-warning btn-sm" title="編集">編集</a>
                <button type="button" class="btn btn-danger btn-sm" onclick="confirmDeleteProduct(${product.id}, '${product.name}')" title="削除">削除</button>
            </div>
        </td>
    `;
    
    return row;
}

/**
 * ページング情報の更新
 * @param {Object} paging - ページング情報
 */
function updatePagination(paging) {
    // 簡易実装（後で詳細実装予定）
    console.log('Pagination:', paging);
}

/**
 * 商品削除確認ダイアログの表示
 * @param {number} productId - 削除対象の商品ID
 * @param {string} productName - 削除対象の商品名
 */
function confirmDeleteProduct(productId, productName) {
    showConfirmationModal(
        '商品削除の確認',
        `商品「${productName}」を削除しますか？`,
        '削除すると元に戻せません。',
        '削除',
        () => deleteProduct(productId)
    );
}

/**
 * 商品削除の実行
 * @param {number} productId - 削除対象の商品ID
 */
async function deleteProduct(productId) {
    try {
        const response = await fetch(`/Products/Delete/${productId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });
        
        if (response.ok) {
            showSuccess('商品が正常に削除されました');
            loadProductList(); // 一覧を再読み込み
        } else {
            showError('商品の削除に失敗しました');
        }
    } catch (error) {
        console.error('商品削除エラー:', error);
        showError('商品削除中にエラーが発生しました');
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
            loadProductList();
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
 * 商品削除機能の初期化
 */
function setupProductDeletion() {
    // 削除ボタンのイベントハンドラーは動的コンテンツのため、
    // updateProductList内で設定
}

/**
 * ローディング表示の表示
 */
function showLoading() {
    const loadingElement = document.getElementById('productListLoading');
    const contentElement = document.getElementById('productListContent');
    
    if (loadingElement) loadingElement.style.display = 'block';
    if (contentElement) contentElement.style.opacity = '0.5';
}

/**
 * ローディング表示の非表示
 */
function hideLoading() {
    const loadingElement = document.getElementById('productListLoading');
    const contentElement = document.getElementById('productListContent');
    
    if (loadingElement) loadingElement.style.display = 'none';
    if (contentElement) contentElement.style.opacity = '1';
}

/**
 * ビュー切り替え（テーブル/グリッド）
 * @param {string} viewType - 表示タイプ（'table' または 'grid'）
 */
function switchView(viewType) {
    const tableView = document.getElementById('tableView');
    const gridView = document.getElementById('gridView');
    const viewButtons = document.querySelectorAll('.view-toggle');
    
    // ボタンの状態更新
    viewButtons.forEach(btn => btn.classList.remove('active'));
    const activeButton = document.querySelector(`[data-view="${viewType}"]`);
    if (activeButton) activeButton.classList.add('active');
    
    // ビューの切り替え
    if (viewType === 'table') {
        if (tableView) tableView.style.display = 'block';
        if (gridView) gridView.style.display = 'none';
    } else if (viewType === 'grid') {
        if (tableView) tableView.style.display = 'none';
        if (gridView) gridView.style.display = 'block';
    }
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