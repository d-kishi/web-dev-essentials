/* ======================================
   コンポーネント用JavaScript - Pure JavaScript
   各種UIコンポーネントの動作を制御
   ====================================== */

/**
 * 確認モーダル管理
 */
const ConfirmationModal = {
    /**
     * 確認モーダル表示
     */
    show(title, message, details = '', okButtonText = '実行', onConfirm = null) {
        const modal = Utils.element('#confirmationModal');
        if (!modal) return;
        
        // 内容設定
        Utils.text('#confirmationModalTitle', title);
        Utils.text('#confirmationModalMessage', message);
        Utils.text('#confirmationModalOkButton', okButtonText);
        
        // 詳細情報設定
        const detailsEl = Utils.element('#confirmationModalDetails');
        if (details) {
            Utils.html(detailsEl, details);
            Utils.show(detailsEl);
        } else {
            Utils.hide(detailsEl);
        }
        
        // 確認ボタンのイベント設定
        const okButton = Utils.element('#confirmationModalOkButton');
        if (okButton) {
            okButton.onclick = () => {
                this.hide();
                if (onConfirm && typeof onConfirm === 'function') {
                    onConfirm();
                }
            };
        }
        
        // モーダル表示
        Utils.show(modal, 'flex');
    },
    
    /**
     * 確認モーダル非表示
     */
    hide() {
        const modal = Utils.element('#confirmationModal');
        if (modal) {
            Utils.hide(modal);
        }
    }
};

/**
 * メッセージクローズ処理
 */
function closeMessage(messageId) {
    const messageEl = Utils.element(`#${messageId}`);
    if (messageEl) {
        Utils.fadeOut(messageEl, 300);
    }
}

/**
 * 確認モーダル表示（グローバル関数）
 */
function showConfirmationModal(title, message, details = '', okButtonText = '実行', onConfirm = null) {
    ConfirmationModal.show(title, message, details, okButtonText, onConfirm);
}

/**
 * 確認モーダル非表示（グローバル関数）
 */
function closeConfirmationModal() {
    ConfirmationModal.hide();
}

/**
 * 削除確認処理の共通化
 */
function confirmDelete(itemType, itemName, deleteAction) {
    const title = `${itemType}削除の確認`;
    const message = `${itemType}「${itemName}」を削除しますか？`;
    const details = '<strong>注意:</strong> 削除すると元に戻せません。';
    
    showConfirmationModal(title, message, details, '削除', deleteAction);
}

/**
 * ページング処理
 */
const Pagination = {
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    
    /**
     * ページ読み込み
     */
    loadPage(page) {
        this.currentPage = page;
        if (typeof window.loadProductList === 'function') {
            window.loadProductList();
        } else if (typeof window.loadCategoryList === 'function') {
            window.loadCategoryList();
        }
    },
    
    /**
     * ページサイズ変更
     */
    changePageSize(pageSize) {
        this.pageSize = parseInt(pageSize);
        this.currentPage = 1; // 1ページ目に戻る
        this.loadPage(1);
    },
    
    /**
     * ページング情報更新
     */
    updatePagination(pagingData) {
        if (!pagingData) return;
        
        this.currentPage = pagingData.currentPage;
        this.pageSize = pagingData.pageSize;
        this.totalCount = pagingData.totalCount;
        
        // ページング表示を更新
        this.renderPagination(pagingData);
    },
    
    /**
     * ページング表示の描画
     */
    renderPagination(pagingData) {
        const container = Utils.element('#paginationContainer');
        if (!container) return;
        
        if (pagingData.totalPages <= 1) {
            Utils.hide(container);
            return;
        }
        
        Utils.show(container);
        
        // ページング情報
        const infoEl = container.querySelector('.pagination-info .pagination-text');
        if (infoEl) {
            const start = (pagingData.currentPage - 1) * pagingData.pageSize + 1;
            const end = Math.min(pagingData.currentPage * pagingData.pageSize, pagingData.totalCount);
            infoEl.textContent = `${pagingData.totalCount} 件中 ${start} - ${end} 件を表示`;
        }
        
        // ページサイズ選択
        const pageSizeSelect = container.querySelector('#pageSizeSelect');
        if (pageSizeSelect) {
            pageSizeSelect.value = pagingData.pageSize;
        }
    }
};

/**
 * グローバルページング関数
 */
function loadPage(page) {
    Pagination.loadPage(page);
}

function changePageSize(pageSize) {
    Pagination.changePageSize(pageSize);
}

/**
 * 検索機能
 */
const SearchComponent = {
    searchTimeout: null,
    suggestionTimeout: null,
    
    /**
     * 検索フォーム初期化
     */
    initializeSearchForm() {
        const searchInput = Utils.element('#searchKeyword');
        if (searchInput) {
            // リアルタイム検索候補
            Utils.on(searchInput, 'input', Utils.debounce((e) => {
                this.showSuggestions(e.target.value);
            }, 300));
            
            // フォーカス時に候補表示
            Utils.on(searchInput, 'focus', (e) => {
                if (e.target.value) {
                    this.showSuggestions(e.target.value);
                }
            });
            
            // Enterキーで検索実行
            Utils.on(searchInput, 'keydown', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.hideSuggestions();
                    this.performSearch();
                }
            });
        }
        
        // 検索フォーム外クリックで候補非表示
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.search-input-group')) {
                this.hideSuggestions();
            }
        });
        
        // 検索候補選択のイベントリスナー（委譲パターン）
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('suggestion-item')) {
                const suggestion = e.target.getAttribute('data-suggestion');
                if (suggestion) {
                    this.selectSuggestion(suggestion);
                }
            }
        });
        
        // 高度な検索オプション切り替え
        const toggleBtn = Utils.element('.toggle-btn');
        if (toggleBtn) {
            Utils.on(toggleBtn, 'click', this.toggleAdvancedSearch);
        }
    },
    
    /**
     * 検索実行
     */
    performSearch() {
        if (typeof window.performSearch === 'function') {
            window.performSearch();
        }
    },
    
    /**
     * 検索リセット
     */
    resetSearch() {
        const form = Utils.element('#searchForm');
        if (form) {
            form.reset();
        }
        
        this.hideSuggestions();
        
        if (typeof window.resetSearch === 'function') {
            window.resetSearch();
        }
    },
    
    /**
     * 検索候補表示（無効化済み）
     * 各ページで独自の検索機能が実装されているため、この機能は無効化
     */
    async showSuggestions(term) {
        // 検索候補機能は無効化（各ページで独自実装済み）
        this.hideSuggestions();
        return;
    },
    
    /**
     * 検索候補描画
     */
    renderSuggestions(suggestions) {
        const container = Utils.element('#searchSuggestions');
        if (!container) return;
        
        if (!suggestions || suggestions.length === 0) {
            this.hideSuggestions();
            return;
        }
        
        const html = suggestions.map(suggestion => 
            `<div class="suggestion-item" data-suggestion="${Utils.escapeHtml(suggestion)}">${Utils.escapeHtml(suggestion)}</div>`
        ).join('');
        
        Utils.html(container, html);
        Utils.show(container);
    },
    
    /**
     * 検索候補選択
     */
    selectSuggestion(suggestion) {
        const searchInput = Utils.element('#searchKeyword');
        if (searchInput) {
            searchInput.value = suggestion;
            this.hideSuggestions();
            this.performSearch();
        }
    },
    
    /**
     * 検索候補非表示
     */
    hideSuggestions() {
        const container = Utils.element('#searchSuggestions');
        if (container) {
            Utils.hide(container);
        }
    },
    
    /**
     * 高度な検索オプション切り替え
     */
    toggleAdvancedSearch() {
        const options = Utils.element('#advancedSearchOptions');
        const toggleText = Utils.element('#advancedSearchToggleText');
        const toggleIcon = Utils.element('#advancedSearchToggleIcon');
        
        if (options) {
            const isVisible = Utils.css(options, 'display') !== 'none';
            
            if (isVisible) {
                Utils.hide(options);
                if (toggleText) toggleText.textContent = '高度な検索オプションを表示';
                if (toggleIcon) {
                    toggleIcon.textContent = '▼';
                    Utils.removeClass(toggleIcon, 'rotated');
                }
            } else {
                Utils.show(options);
                if (toggleText) toggleText.textContent = '高度な検索オプションを非表示';
                if (toggleIcon) {
                    toggleIcon.textContent = '▲';
                    Utils.addClass(toggleIcon, 'rotated');
                }
            }
        }
    },
    
    /**
     * 検索統計表示
     */
    showSearchStats(totalCount, searchTime = null) {
        const statsEl = Utils.element('#searchStats');
        const statsTextEl = Utils.element('#searchStatsText');
        const searchTimeEl = Utils.element('#searchTime');
        
        if (statsEl && statsTextEl) {
            statsTextEl.textContent = `検索結果: ${totalCount}件`;
            
            if (searchTime && searchTimeEl) {
                searchTimeEl.textContent = `(${searchTime}ms)`;
            }
            
            Utils.show(statsEl);
        }
    }
};

/**
 * テーブル機能
 */
const TableComponent = {
    currentSort: { column: null, direction: 'asc' },
    
    /**
     * ソート機能初期化
     */
    initializeSort() {
        Utils.onAll('.sortable', 'click', (e) => {
            const th = e.currentTarget;
            const column = th.textContent.trim();
            this.sort(column, th);
        });
    },
    
    /**
     * ソート実行
     */
    sort(column, headerElement) {
        let direction = 'asc';
        
        if (this.currentSort.column === column) {
            direction = this.currentSort.direction === 'asc' ? 'desc' : 'asc';
        }
        
        this.currentSort = { column, direction };
        
        // ソート表示更新
        this.updateSortIndicators(headerElement, direction);
        
        // ソート実行（実装は各ページで）
        if (typeof window.sortTable === 'function') {
            window.sortTable(column, direction);
        }
    },
    
    /**
     * ソート表示更新
     */
    updateSortIndicators(activeHeader, direction) {
        // すべてのソート表示をリセット
        Utils.element$('.sort-indicator').forEach(indicator => {
            indicator.textContent = '';
        });
        
        // アクティブなソート表示
        const indicator = activeHeader.querySelector('.sort-indicator');
        if (indicator) {
            indicator.textContent = direction === 'asc' ? '▲' : '▼';
        }
    }
};

/**
 * 表示切り替え機能
 */
const ViewToggle = {
    currentView: 'table',
    
    /**
     * 表示切り替え初期化
     */
    initialize() {
        Utils.onAll('.view-toggle', 'click', (e) => {
            const view = Utils.attr(e.currentTarget, 'data-view');
            this.switchView(view);
        });
    },
    
    /**
     * 表示切り替え
     */
    switchView(view) {
        this.currentView = view;
        
        // ボタンの状態更新
        Utils.element$('.view-toggle').forEach(btn => {
            Utils.removeClass(btn, 'active');
        });
        
        const activeBtn = Utils.element(`.view-toggle[data-view="${view}"]`);
        if (activeBtn) {
            Utils.addClass(activeBtn, 'active');
        }
        
        // 表示の切り替え
        const tableView = Utils.element('#tableView');
        const gridView = Utils.element('#gridView');
        
        if (view === 'table') {
            if (tableView) Utils.show(tableView);
            if (gridView) Utils.hide(gridView);
        } else if (view === 'grid') {
            if (tableView) Utils.hide(tableView);
            if (gridView) Utils.show(gridView);
            this.loadGridView();
        }
    },
    
    /**
     * グリッド表示用データ読み込み
     */
    async loadGridView() {
        const gridContainer = Utils.element('#productGrid');
        if (!gridContainer) return;
        
        // グリッド表示用のデータ読み込み処理
        // 実装は各ページで
        if (typeof window.loadGridData === 'function') {
            window.loadGridData();
        }
    }
};

/**
 * フォーム機能
 */
const FormComponent = {
    /**
     * フォームバリデーション設定
     */
    setupFormValidation() {
        // リアルタイムバリデーション
        Utils.onAll('input, select, textarea', 'blur', (e) => {
            this.validateField(e.target);
        });
        
        // 文字数カウント
        Utils.onAll('textarea[maxlength]', 'input', (e) => {
            this.updateCharacterCount(e.target);
        });
    },
    
    /**
     * フィールドバリデーション
     */
    validateField(field) {
        const value = field.value.trim();
        const fieldName = field.name;
        let isValid = true;
        let errorMessage = '';
        
        // 必須チェック
        if (field.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = 'この項目は必須です';
        }
        
        // 型別バリデーション
        if (isValid && value) {
            switch (field.type) {
                case 'email':
                    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
                        isValid = false;
                        errorMessage = '正しいメールアドレスを入力してください';
                    }
                    break;
                    
                case 'number':
                    const min = field.getAttribute('min');
                    const max = field.getAttribute('max');
                    const numValue = parseFloat(value);
                    
                    if (isNaN(numValue)) {
                        isValid = false;
                        errorMessage = '数値を入力してください';
                    } else if (min && numValue < parseFloat(min)) {
                        isValid = false;
                        errorMessage = `${min}以上の値を入力してください`;
                    } else if (max && numValue > parseFloat(max)) {
                        isValid = false;
                        errorMessage = `${max}以下の値を入力してください`;
                    }
                    break;
            }
        }
        
        // パターンチェック
        if (isValid && value && field.pattern) {
            const regex = new RegExp(field.pattern);
            if (!regex.test(value)) {
                isValid = false;
                errorMessage = field.title || 'パターンに一致しません';
            }
        }
        
        // 結果表示
        this.showFieldValidation(field, isValid, errorMessage);
        
        return isValid;
    },
    
    /**
     * フィールドバリデーション結果表示
     */
    showFieldValidation(field, isValid, errorMessage = '') {
        // エラー表示要素を取得または作成
        let errorEl = field.parentNode.querySelector('.validation-error');
        
        if (isValid) {
            Utils.removeClass(field, 'error');
            if (errorEl) {
                errorEl.textContent = '';
                Utils.hide(errorEl);
            }
        } else {
            Utils.addClass(field, 'error');
            if (!errorEl) {
                errorEl = document.createElement('span');
                errorEl.className = 'validation-error';
                field.parentNode.appendChild(errorEl);
            }
            errorEl.textContent = errorMessage;
            Utils.show(errorEl);
        }
    },
    
    /**
     * 文字数カウント更新
     */
    updateCharacterCount(textarea) {
        const maxLength = parseInt(textarea.getAttribute('maxlength'));
        const currentLength = textarea.value.length;
        
        // カウント表示要素を探す
        const countElements = textarea.parentNode.querySelectorAll('.character-count span');
        if (countElements.length > 0) {
            const countEl = countElements[0];
            countEl.textContent = currentLength;
            
            // 警告表示
            const countContainer = countEl.closest('.character-count');
            if (countContainer) {
                Utils.removeClass(countContainer, 'warning', 'danger');
                
                if (currentLength >= maxLength * 0.9) {
                    Utils.addClass(countContainer, currentLength >= maxLength ? 'danger' : 'warning');
                }
            }
        }
    }
};

/**
 * 画像アップロード機能
 */
const ImageUpload = {
    /**
     * 画像アップロード初期化
     */
    initialize() {
        const uploadArea = Utils.element('.image-upload-area');
        if (uploadArea) {
            // ドラッグ&ドロップ設定
            this.setupDragAndDrop(uploadArea);
        }
        
        // ファイル選択時の処理
        Utils.onAll('input[type="file"][accept*="image"]', 'change', (e) => {
            this.handleFileSelect(e.target);
        });
        
        // プレビュー削除ボタンのイベントリスナー（委譲パターン）
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('preview-action-btn')) {
                const previewItem = e.target.closest('.image-preview-item');
                if (previewItem) {
                    previewItem.remove();
                }
            }
        });
    },
    
    /**
     * ドラッグ&ドロップ設定
     */
    setupDragAndDrop(uploadArea) {
        uploadArea.addEventListener('dragover', (e) => {
            e.preventDefault();
            Utils.addClass(uploadArea, 'dragover');
        });
        
        uploadArea.addEventListener('dragleave', (e) => {
            e.preventDefault();
            Utils.removeClass(uploadArea, 'dragover');
        });
        
        uploadArea.addEventListener('drop', (e) => {
            e.preventDefault();
            Utils.removeClass(uploadArea, 'dragover');
            
            const files = Array.from(e.dataTransfer.files).filter(file => 
                file.type.startsWith('image/')
            );
            
            if (files.length > 0) {
                this.handleFiles(files);
            }
        });
    },
    
    /**
     * ファイル選択処理
     */
    handleFileSelect(input) {
        const files = Array.from(input.files);
        this.handleFiles(files);
    },
    
    /**
     * ファイル処理
     */
    handleFiles(files) {
        const previewArea = Utils.element('#imagePreviewArea');
        const previewList = Utils.element('#imagePreviewList');
        
        if (!previewArea || !previewList) return;
        
        files.forEach((file, index) => {
            this.createPreview(file, previewList);
        });
        
        Utils.show(previewArea);
    },
    
    /**
     * プレビュー作成
     */
    createPreview(file, container) {
        const reader = new FileReader();
        
        reader.onload = (e) => {
            const previewItem = document.createElement('div');
            previewItem.className = 'image-preview-item';
            previewItem.innerHTML = `
                <img src="${e.target.result}" alt="プレビュー画像" class="preview-image">
                <div class="preview-info">
                    <div class="preview-name">${Utils.escapeHtml(file.name)}</div>
                    <div class="preview-size">${this.formatFileSize(file.size)}</div>
                </div>
                <div class="preview-actions">
                    <button type="button" class="preview-action-btn" title="削除">
                        ×
                    </button>
                </div>
            `;
            
            container.appendChild(previewItem);
        };
        
        reader.readAsDataURL(file);
    },
    
    /**
     * ファイルサイズフォーマット
     */
    formatFileSize(bytes) {
        if (bytes === 0) return '0 B';
        
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        
        return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    }
};

/**
 * グローバル関数
 */
function performSearch() {
    SearchComponent.performSearch();
}

function resetSearch() {
    SearchComponent.resetSearch();
}

function toggleAdvancedSearch() {
    SearchComponent.toggleAdvancedSearch();
}

function switchView(view) {
    ViewToggle.switchView(view);
}

function sortBy(column) {
    // 実装は各ページで
    console.log('Sort by:', column);
}

function updateCharacterCount(element, counterId) {
    const currentLength = element.value.length;
    const counterEl = Utils.element(`#${counterId}`);
    if (counterEl) {
        counterEl.textContent = currentLength;
    }
}

function highlightFieldError(field) {
    Utils.addClass(field, 'error');
}

function showValidationErrors(errors) {
    const summaryEl = Utils.element('#formValidationSummary');
    const listEl = Utils.element('#validationErrorList');
    
    if (summaryEl && listEl) {
        listEl.innerHTML = errors.map(error => 
            `<li>${Utils.escapeHtml(error)}</li>`
        ).join('');
        
        Utils.show(summaryEl);
    }
}

function hideValidationErrors() {
    const summaryEl = Utils.element('#formValidationSummary');
    if (summaryEl) {
        Utils.hide(summaryEl);
    }
    
    // 個別フィールドエラーもクリア
    Utils.elements('.form-input.error, .form-select.error, .form-textarea.error').forEach(field => {
        Utils.removeClass(field, 'error');
    });
    
    Utils.elements('.validation-error').forEach(errorEl => {
        Utils.hide(errorEl);
    });
}

function showValidationMessage(elementId, message, type) {
    const element = Utils.element(`#${elementId}`);
    if (element) {
        element.textContent = message;
        element.className = `field-validation ${type}`;
        Utils.show(element);
    }
}

function hideValidationMessage(elementId) {
    const element = Utils.element(`#${elementId}`);
    if (element) {
        Utils.hide(element);
    }
}

/**
 * フォームバリデーション設定（グローバル関数）
 */
function setupFormValidation() {
    FormComponent.setupFormValidation();
}

// 初期化
document.addEventListener('DOMContentLoaded', function() {
    SearchComponent.initializeSearchForm();
    TableComponent.initializeSort();
    ViewToggle.initialize();
    FormComponent.setupFormValidation();
    ImageUpload.initialize();
});

// グローバル公開
window.ConfirmationModal = ConfirmationModal;
window.SearchComponent = SearchComponent;
window.TableComponent = TableComponent;
window.ViewToggle = ViewToggle;
window.FormComponent = FormComponent;
window.ImageUpload = ImageUpload;
window.Pagination = Pagination;