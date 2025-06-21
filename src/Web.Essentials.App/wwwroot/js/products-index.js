/**
 * 商品一覧画面用JavaScript
 * 検索、ページング、ソート、表示切替、削除機能を提供
 */

// 現在の検索条件を保持するグローバル変数
let currentSearchParams = {
    searchKeyword: '',
    categoryId: null,
    status: null,
    page: 1,
    pageSize: 10,
    sortBy: 'updatedat_desc'
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
    
    // 商品削除の初期化
    setupProductDeletion();
    
    // ページング機能の初期化
    setupPagination();
}

/**
 * 検索実行
 * フォームの入力値を取得して検索を実行
 */
function performSearch() {
    const searchKeyword = document.getElementById('searchKeyword').value;
    const categoryId = document.getElementById('categoryFilter').value;
    const status = document.getElementById('statusFilter').value;
    
    currentSearchParams.searchKeyword = searchKeyword;
    currentSearchParams.categoryId = categoryId || null;
    currentSearchParams.status = status || null;
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
        status: null,
        page: 1,
        pageSize: currentSearchParams.pageSize,
        sortBy: 'updatedat_desc'
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
        if (currentSearchParams.status) {
            params.append('status', currentSearchParams.status);
        }
        params.append('page', currentSearchParams.page);
        params.append('pageSize', currentSearchParams.pageSize);
        params.append('sortBy', currentSearchParams.sortBy);
        
        console.log('APIリクエストURL:', `/api/products?${params.toString()}`);
        console.log('現在のソートパラメーター:', currentSearchParams.sortBy);
        
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
        if (data.products.length === 0) {
            // データが0件の場合のメッセージ表示
            const noDataRow = document.createElement('tr');
            noDataRow.className = 'no-data-row';
            noDataRow.innerHTML = `
                <td colspan="8" class="no-data-cell">
                    <div class="no-data-message">
                        <div class="no-data-icon">📦</div>
                        <h3>商品が見つかりません</h3>
                        <p>検索条件を変更するか、新しい商品を登録してください。</p>
                        <a href="/Products/Create" class="btn btn-primary">商品を登録する</a>
                    </div>
                </td>
            `;
            tbody.appendChild(noDataRow);
        } else {
            data.products.forEach(product => {
                const row = createProductRow(product);
                tbody.appendChild(row);
            });
        }
    }
    
    // ソートインジケーターの更新
    updateSortIndicators();
    
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
        <td class="col-status">
            <span class="status-badge status-${product.status}">${product.statusName}</span>
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
                <button type="button" class="btn btn-danger btn-sm product-delete-button" data-product-id="${product.id}" data-product-name="${product.name.replace(/"/g, '&quot;').replace(/'/g, '&#39;')}" title="削除">削除</button>
            </div>
        </td>
    `;
    
    // 削除ボタンのイベントリスナーを設定
    const deleteButton = row.querySelector('.product-delete-button');
    if (deleteButton) {
        deleteButton.addEventListener('click', function() {
            const productId = parseInt(this.getAttribute('data-product-id'));
            const productName = this.getAttribute('data-product-name');
            confirmDeleteProduct(productId, productName);
        });
    }
    
    return row;
}

/**
 * ページング情報の更新
 * @param {Object} paging - ページング情報
 */
function updatePagination(paging) {
    const container = document.getElementById('paginationContainer');
    if (!container) return;
    
    // ページング情報に基づいてHTMLを再構築
    container.innerHTML = createPaginationHtml(paging);
    
    // 表示件数セレクトボックスの値を更新
    const pageSizeSelect = container.querySelector('#pageSizeSelect');
    if (pageSizeSelect) {
        pageSizeSelect.value = currentSearchParams.pageSize;
    }
    
    // イベントリスナーを再設定
    setupPagination();
}

/**
 * ページングHTMLの生成
 * @param {Object} paging - ページング情報
 * @returns {string} 生成されたHTML文字列
 */
function createPaginationHtml(paging) {
    if (!paging || paging.totalPages === 0) {
        return '<div class="pagination-container"><div class="pagination-info"><span class="pagination-text">検索結果が見つかりませんでした</span></div></div>';
    }
    
    if (paging.totalPages === 1) {
        return `
        <div class="pagination-container">
            <div class="pagination-info">
                <span class="pagination-text">${paging.totalCount} 件中 1 - ${paging.totalCount} 件を表示</span>
            </div>
            <div class="page-size-selector">
                <label for="pageSizeSelect">表示件数:</label>
                <select id="pageSizeSelect" class="page-size-select" data-action="change-page-size">
                    <option value="10" ${paging.pageSize === 10 ? 'selected' : ''}>10件</option>
                    <option value="20" ${paging.pageSize === 20 ? 'selected' : ''}>20件</option>
                    <option value="50" ${paging.pageSize === 50 ? 'selected' : ''}>50件</option>
                    <option value="100" ${paging.pageSize === 100 ? 'selected' : ''}>100件</option>
                </select>
            </div>
        </div>`;
    }
    
    const startItem = (paging.currentPage - 1) * paging.pageSize + 1;
    const endItem = Math.min(paging.currentPage * paging.pageSize, paging.totalCount);
    
    let html = `
    <div class="pagination-container">
        <div class="pagination-info">
            <span class="pagination-text">${paging.totalCount} 件中 ${startItem} - ${endItem} 件を表示</span>
        </div>
        <nav class="pagination-nav" aria-label="ページネーション">
            <ul class="pagination-list">`;
    
    // 最初のページへ
    if (paging.currentPage > 1) {
        html += '<li class="pagination-item"><button type="button" class="pagination-link" data-page="1" data-action="load-page" title="最初のページ">≪</button></li>';
    } else {
        html += '<li class="pagination-item disabled"><span class="pagination-link">≪</span></li>';
    }
    
    // 前のページへ
    if (paging.hasPreviousPage) {
        html += `<li class="pagination-item"><button type="button" class="pagination-link" data-page="${paging.currentPage - 1}" data-action="load-page" title="前のページ">‹</button></li>`;
    } else {
        html += '<li class="pagination-item disabled"><span class="pagination-link">‹</span></li>';
    }
    
    // ページ番号
    const startPage = Math.max(1, paging.currentPage - 2);
    const endPage = Math.min(paging.totalPages, paging.currentPage + 2);
    
    // 最初のページが表示範囲外の場合
    if (startPage > 1) {
        html += '<li class="pagination-item"><button type="button" class="pagination-link" data-page="1" data-action="load-page">1</button></li>';
        if (startPage > 2) {
            html += '<li class="pagination-item disabled"><span class="pagination-link">...</span></li>';
        }
    }
    
    // ページ番号表示
    for (let i = startPage; i <= endPage; i++) {
        if (i === paging.currentPage) {
            html += `<li class="pagination-item active"><span class="pagination-link current">${i}</span></li>`;
        } else {
            html += `<li class="pagination-item"><button type="button" class="pagination-link" data-page="${i}" data-action="load-page">${i}</button></li>`;
        }
    }
    
    // 最後のページが表示範囲外の場合
    if (endPage < paging.totalPages) {
        if (endPage < paging.totalPages - 1) {
            html += '<li class="pagination-item disabled"><span class="pagination-link">...</span></li>';
        }
        html += `<li class="pagination-item"><button type="button" class="pagination-link" data-page="${paging.totalPages}" data-action="load-page">${paging.totalPages}</button></li>`;
    }
    
    // 次のページへ
    if (paging.hasNextPage) {
        html += `<li class="pagination-item"><button type="button" class="pagination-link" data-page="${paging.currentPage + 1}" data-action="load-page" title="次のページ">›</button></li>`;
    } else {
        html += '<li class="pagination-item disabled"><span class="pagination-link">›</span></li>';
    }
    
    // 最後のページへ
    if (paging.currentPage < paging.totalPages) {
        html += `<li class="pagination-item"><button type="button" class="pagination-link" data-page="${paging.totalPages}" data-action="load-page" title="最後のページ">≫</button></li>`;
    } else {
        html += '<li class="pagination-item disabled"><span class="pagination-link">≫</span></li>';
    }
    
    html += `
            </ul>
        </nav>
        <div class="page-size-selector">
            <label for="pageSizeSelect">表示件数:</label>
            <select id="pageSizeSelect" class="page-size-select" data-action="change-page-size">
                <option value="10" ${paging.pageSize === 10 ? 'selected' : ''}>10件</option>
                <option value="20" ${paging.pageSize === 20 ? 'selected' : ''}>20件</option>
                <option value="50" ${paging.pageSize === 50 ? 'selected' : ''}>50件</option>
                <option value="100" ${paging.pageSize === 100 ? 'selected' : ''}>100件</option>
            </select>
        </div>
    </div>`;
    
    return html;
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
    
    // 検索ボタンとリセットボタンのイベントリスナー
    const searchButton = document.querySelector('[data-action="perform-search"]');
    if (searchButton) {
        searchButton.addEventListener('click', performSearch);
    }
    
    const resetButton = document.querySelector('[data-action="reset-search"]');
    if (resetButton) {
        resetButton.addEventListener('click', resetSearch);
    }
}

/**
 * ソート機能の初期化
 */
function setupSortFeatures() {
    const sortableHeaders = document.querySelectorAll('th.sortable');
    sortableHeaders.forEach(header => {
        header.addEventListener('click', function() {
            const column = this.getAttribute('data-column');
            sortByColumn(column);
        });
    });
    
    // 初期表示時のソートインジケーター更新
    updateSortIndicators();
}

/**
 * ページング機能の初期化
 */
function setupPagination() {
    // ページ移動ボタンのイベントリスナー
    const pageButtons = document.querySelectorAll('[data-action="load-page"]');
    pageButtons.forEach(button => {
        button.addEventListener('click', function() {
            const page = parseInt(this.getAttribute('data-page'));
            loadPage(page);
        });
    });
    
    // 表示件数変更のイベントリスナー
    const pageSizeSelect = document.querySelector('[data-action="change-page-size"]');
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener('change', function() {
            const pageSize = parseInt(this.value);
            changePageSize(pageSize);
        });
    }
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
 * カラムソート（テーブルヘッダークリック時）
 * @param {string} column - ソート対象のカラム名
 */
function sortByColumn(column) {
    // 現在のソート状態を確認して昇順/降順を切り替え
    const currentSort = currentSearchParams.sortBy;
    let newSort;
    
    if (currentSort === `${column}_asc`) {
        newSort = `${column}_desc`;
    } else {
        newSort = `${column}_asc`;
    }
    
    console.log('ソート変更:', { column, currentSort, newSort });
    changeSortOrder(newSort);
}

/**
 * ソートインジケーターの更新
 */
function updateSortIndicators() {
    // 全てのソートインジケーターをリセット
    const indicators = document.querySelectorAll('.sort-indicator');
    indicators.forEach(indicator => {
        indicator.textContent = '↕';
    });
    
    // 現在のソート状態に基づいてインジケーターを更新
    const [column, direction] = currentSearchParams.sortBy.split('_');
    const currentIndicator = document.querySelector(`[data-column="${column}"] .sort-indicator`);
    
    if (currentIndicator) {
        currentIndicator.textContent = direction === 'asc' ? '↑' : '↓';
    }
}

// 初期パラメータ設定用の関数（Razorビューから呼び出される）
function setInitialSearchParams(params) {
    currentSearchParams = {
        ...currentSearchParams,
        ...params
    };
}