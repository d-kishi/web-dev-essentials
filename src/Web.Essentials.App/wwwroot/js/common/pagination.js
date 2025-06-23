/**
 * ページング共通処理
 * ファイル: wwwroot/js/common/pagination.js
 */

/**
 * ページング表示・制御コンポーネント
 */
class PaginationComponent {
    /**
     * コンストラクタ
     * @param {HTMLElement} container - 表示コンテナ
     * @param {Object} paginationData - ページング情報
     * @param {Function} onPageChange - ページ変更時のコールバック
     */
    constructor(container, paginationData, onPageChange) {
        this.container = container;
        this.paginationData = paginationData;
        this.onPageChange = onPageChange;
        this.maxVisiblePages = 7; // 表示する最大ページ数
    }

    /**
     * ページングのレンダリング
     */
    render() {
        if (!this.container || !this.paginationData) return;

        const { currentPage, totalPages, totalCount, pageSize } = this.paginationData;

        if (totalPages <= 1) {
            this.container.innerHTML = '';
            return;
        }

        const pageNumbers = this.calculateVisiblePages(currentPage, totalPages);
        
        this.container.innerHTML = `
            <nav aria-label="ページネーション" class="pagination-nav">
                <div class="pagination-info">
                    <span>全 ${totalCount.toLocaleString()} 件中 ${this.getDisplayRange()} 件を表示</span>
                </div>
                <ul class="pagination">
                    ${this.renderPreviousButton(currentPage)}
                    ${pageNumbers.map(page => this.renderPageButton(page, currentPage)).join('')}
                    ${this.renderNextButton(currentPage, totalPages)}
                </ul>
            </nav>
        `;

        this.setupEventListeners();
    }

    /**
     * 表示する範囲の計算
     * @returns {string} 表示範囲の文字列
     */
    getDisplayRange() {
        const { currentPage, totalCount, pageSize } = this.paginationData;
        const startItem = (currentPage - 1) * pageSize + 1;
        const endItem = Math.min(currentPage * pageSize, totalCount);
        return `${startItem.toLocaleString()} - ${endItem.toLocaleString()}`;
    }

    /**
     * 表示するページ番号の計算
     * @param {number} currentPage - 現在のページ
     * @param {number} totalPages - 総ページ数
     * @returns {Array} 表示するページ番号の配列
     */
    calculateVisiblePages(currentPage, totalPages) {
        const maxVisible = this.maxVisiblePages;
        const halfVisible = Math.floor(maxVisible / 2);

        let startPage = Math.max(1, currentPage - halfVisible);
        let endPage = Math.min(totalPages, startPage + maxVisible - 1);

        // 終端調整
        if (endPage - startPage + 1 < maxVisible) {
            startPage = Math.max(1, endPage - maxVisible + 1);
        }

        const pages = [];
        
        // 最初のページ
        if (startPage > 1) {
            pages.push(1);
            if (startPage > 2) {
                pages.push('...');
            }
        }

        // 中間ページ
        for (let i = startPage; i <= endPage; i++) {
            pages.push(i);
        }

        // 最後のページ
        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                pages.push('...');
            }
            pages.push(totalPages);
        }

        return pages;
    }

    /**
     * 前へボタンのレンダリング
     * @param {number} currentPage - 現在のページ
     * @returns {string} HTML文字列
     */
    renderPreviousButton(currentPage) {
        const disabled = currentPage <= 1;
        return `
            <li class="page-item ${disabled ? 'disabled' : ''}">
                <button class="page-link" 
                        data-page="${currentPage - 1}" 
                        ${disabled ? 'disabled' : ''}
                        aria-label="前のページ">
                    <span aria-hidden="true">&laquo;</span>
                    <span class="sr-only">前のページ</span>
                </button>
            </li>
        `;
    }

    /**
     * 次へボタンのレンダリング
     * @param {number} currentPage - 現在のページ
     * @param {number} totalPages - 総ページ数
     * @returns {string} HTML文字列
     */
    renderNextButton(currentPage, totalPages) {
        const disabled = currentPage >= totalPages;
        return `
            <li class="page-item ${disabled ? 'disabled' : ''}">
                <button class="page-link" 
                        data-page="${currentPage + 1}" 
                        ${disabled ? 'disabled' : ''}
                        aria-label="次のページ">
                    <span aria-hidden="true">&raquo;</span>
                    <span class="sr-only">次のページ</span>
                </button>
            </li>
        `;
    }

    /**
     * ページボタンのレンダリング
     * @param {number|string} page - ページ番号または"..."
     * @param {number} currentPage - 現在のページ
     * @returns {string} HTML文字列
     */
    renderPageButton(page, currentPage) {
        if (page === '...') {
            return `
                <li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>
            `;
        }

        const isActive = page === currentPage;
        return `
            <li class="page-item ${isActive ? 'active' : ''}">
                <button class="page-link" 
                        data-page="${page}"
                        ${isActive ? 'aria-current="page"' : ''}>
                    ${page}
                </button>
            </li>
        `;
    }

    /**
     * イベントリスナーの設定
     */
    setupEventListeners() {
        this.container.querySelectorAll('.page-link[data-page]').forEach(button => {
            button.addEventListener('click', (event) => {
                event.preventDefault();
                const page = parseInt(event.target.getAttribute('data-page'));
                if (page && !isNaN(page) && this.onPageChange) {
                    this.onPageChange(page);
                }
            });
        });
    }
}

// グローバル公開
window.PaginationComponent = PaginationComponent;