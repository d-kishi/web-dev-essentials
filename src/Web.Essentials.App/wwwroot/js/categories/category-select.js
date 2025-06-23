/**
 * カテゴリ選択用JavaScript
 * カテゴリ選択コンポーネントの機能を提供
 */

// グローバル変数
let categoryData = [];
let currentMode = 'select';
let selectedCategories = [];

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    // カテゴリデータがグローバル変数に設定されるまで待つ
    if (window.categorySelectData) {
        categoryData = window.categorySelectData;
        initializeCategorySelect();
    }
});

/**
 * カテゴリ選択の初期化
 */
function initializeCategorySelect() {
    setupCategoryModes();
    setupTreeInteraction();
    updateCategorySelection();
    
    // 検索入力の初期化
    const searchInput = document.querySelector('.category-search-input');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            filterCategories(this.value);
        });
    }
    
    // モード切り替えボタンの初期化
    document.querySelectorAll('.mode-tab').forEach(tab => {
        tab.addEventListener('click', function() {
            const mode = this.getAttribute('data-mode');
            switchCategoryMode(mode);
        });
    });
    
    // セレクトボックスの変更イベント
    const selectElement = document.querySelector('select[data-category-select]');
    if (selectElement) {
        selectElement.addEventListener('change', function() {
            updateCategorySelection();
        });
    }
    
    // チェックボックスの変更イベント
    document.querySelectorAll('input[type="checkbox"][data-category-checkbox]').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            updateMultipleSelection();
        });
    });
    
    // 初期選択値の設定
    if (window.initialCategorySelection) {
        setInitialSelection(window.initialCategorySelection);
    }
    
    // クリア選択ボタンのイベントリスナーを設定
    setupClearSelectionButtons();
}

/**
 * モード切り替え
 * @param {string} mode - 切り替えるモード（select, tree, checkbox）
 */
function switchCategoryMode(mode) {
    // タブの状態更新
    document.querySelectorAll('.mode-tab').forEach(tab => {
        tab.classList.remove('active');
    });
    const activeTab = document.querySelector(`[data-mode="${mode}"]`);
    if (activeTab) {
        activeTab.classList.add('active');
    }
    
    // モードコンテンツの表示切り替え
    document.querySelectorAll('.category-mode').forEach(modeDiv => {
        modeDiv.classList.remove('active');
    });
    const activeMode = document.getElementById(mode + 'Mode');
    if (activeMode) {
        activeMode.classList.add('active');
    }
    
    currentMode = mode;
    updateCategorySelection();
}

/**
 * カテゴリフィルタリング
 * @param {string} searchTerm - 検索キーワード
 */
function filterCategories(searchTerm) {
    const term = searchTerm.toLowerCase();
    const fieldName = window.categoryFieldName || 'CategoryId';
    
    if (currentMode === 'select') {
        // 選択リストのフィルタリング
        const select = document.getElementById(fieldName);
        if (select) {
            const options = select.querySelectorAll('option');
            
            options.forEach(option => {
                if (option.value === '') return; // 空のオプションはスキップ
                
                const text = option.textContent.toLowerCase();
                const match = text.includes(term);
                option.style.display = match ? '' : 'none';
            });
        }
    } else if (currentMode === 'tree') {
        // ツリー表示のフィルタリング
        const treeItems = document.querySelectorAll('.tree-item');
        
        treeItems.forEach(item => {
            const label = item.querySelector('.category-name');
            if (label) {
                const text = label.textContent.toLowerCase();
                const match = text.includes(term);
                item.style.display = match ? '' : 'none';
            }
        });
    } else if (currentMode === 'checkbox') {
        // チェックボックス表示のフィルタリング
        const checkboxItems = document.querySelectorAll('.checkbox-item');
        
        checkboxItems.forEach(item => {
            const label = item.querySelector('label');
            if (label) {
                const text = label.textContent.toLowerCase();
                const match = text.includes(term);
                item.style.display = match ? '' : 'none';
            }
        });
    }
}

/**
 * カテゴリモードの設定
 */
function setupCategoryModes() {
    // モードタブのクリックイベントは初期化で設定済み
}

/**
 * ツリーインタラクションの設定
 */
function setupTreeInteraction() {
    // ツリートグルボタンの設定
    document.querySelectorAll('.tree-toggle').forEach(toggle => {
        toggle.addEventListener('click', function() {
            const treeItem = this.closest('.tree-item');
            const children = treeItem.querySelector('.tree-children');
            
            if (children) {
                const isExpanded = !children.classList.contains('collapsed');
                
                if (isExpanded) {
                    children.classList.add('collapsed');
                    this.textContent = '▶';
                } else {
                    children.classList.remove('collapsed');
                    this.textContent = '▼';
                }
            }
        });
    });
    
    // ツリーアイテムの選択
    document.querySelectorAll('.tree-label.selectable').forEach(label => {
        label.addEventListener('click', function() {
            const categoryId = this.getAttribute('data-category-id');
            const categoryName = this.textContent.trim();
            
            if (categoryId) {
                selectCategory(parseInt(categoryId), categoryName);
            }
        });
    });
}

/**
 * カテゴリ選択
 * @param {number} categoryId - カテゴリID
 * @param {string} categoryName - カテゴリ名
 * @param {string} fullPath - フルパス
 */
function selectCategory(categoryId, categoryName, fullPath) {
    const fieldName = window.categoryFieldName || 'CategoryId';
    const hiddenInput = document.querySelector(`input[name="${fieldName}"]`);
    const selectElement = document.querySelector(`select[name="${fieldName}"]`);
    
    if (hiddenInput) {
        hiddenInput.value = categoryId;
    }
    
    if (selectElement) {
        selectElement.value = categoryId;
    }
    
    // 選択状態の更新
    updateSelectedCategoryDisplay(categoryId, categoryName, fullPath);
    updateCategorySelection();
}

/**
 * 初期選択値の設定
 * @param {number} categoryId - 初期選択するカテゴリID
 */
function setInitialSelection(categoryId) {
    if (!categoryId) return;
    
    // カテゴリデータから該当カテゴリを検索
    const category = findCategoryById(categoryId);
    if (category) {
        selectCategory(categoryId, category.name, category.fullPath);
    }
}

/**
 * カテゴリIDでカテゴリを検索
 * @param {number} categoryId - 検索するカテゴリID
 * @returns {Object|null} 見つかったカテゴリオブジェクト
 */
function findCategoryById(categoryId) {
    function searchInCategories(categories) {
        for (const category of categories) {
            if (category.id === categoryId) {
                return category;
            }
            if (category.children && category.children.length > 0) {
                const found = searchInCategories(category.children);
                if (found) return found;
            }
        }
        return null;
    }
    
    return searchInCategories(categoryData);
}

/**
 * 選択されたカテゴリの表示を更新
 * @param {number} categoryId - カテゴリID
 * @param {string} categoryName - カテゴリ名
 * @param {string} fullPath - フルパス
 */
function updateSelectedCategoryDisplay(categoryId, categoryName, fullPath) {
    const displayElement = document.querySelector('.selected-category-display');
    if (displayElement) {
        if (categoryId) {
            displayElement.innerHTML = `
                <div class="selected-category">
                    <span class="category-name">${categoryName}</span>
                    <span class="category-path">${fullPath || ''}</span>
                    <button type="button" class="clear-selection">×</button>
                </div>
            `;
            displayElement.style.display = 'block';
        } else {
            displayElement.style.display = 'none';
        }
    }
}

/**
 * クリア選択ボタンのイベントリスナー設定
 */
function setupClearSelectionButtons() {
    // 既存のクリア選択ボタンにイベントリスナーを設定（委譲）
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('clear-selection')) {
            clearCategorySelection();
        }
    });
}

/**
 * カテゴリ選択のクリア
 */
function clearCategorySelection() {
    const fieldName = window.categoryFieldName || 'CategoryId';
    const hiddenInput = document.querySelector(`input[name="${fieldName}"]`);
    const selectElement = document.querySelector(`select[name="${fieldName}"]`);
    
    if (hiddenInput) {
        hiddenInput.value = '';
    }
    
    if (selectElement) {
        selectElement.value = '';
    }
    
    updateSelectedCategoryDisplay(null, '', '');
    updateCategorySelection();
}

/**
 * カテゴリ選択状態の更新
 */
function updateCategorySelection() {
    const fieldName = window.categoryFieldName || 'CategoryId';
    const selectedValue = document.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"]`)?.value;
    
    // ツリー表示での選択状態を更新
    document.querySelectorAll('.tree-label').forEach(label => {
        const categoryId = label.getAttribute('data-category-id');
        if (categoryId === selectedValue) {
            label.classList.add('selected');
        } else {
            label.classList.remove('selected');
        }
    });
    
    // バリデーション
    validateCategorySelection();
}

/**
 * 複数選択の更新（チェックボックスモード用）
 */
function updateMultipleSelection() {
    const checkedBoxes = document.querySelectorAll('input[type="checkbox"][data-category-checkbox]:checked');
    selectedCategories = Array.from(checkedBoxes).map(checkbox => ({
        id: parseInt(checkbox.value),
        name: checkbox.getAttribute('data-category-name') || ''
    }));
    
    // 隠しフィールドの更新
    updateHiddenMultipleSelection();
}

/**
 * 複数選択の隠しフィールド更新
 */
function updateHiddenMultipleSelection() {
    const container = document.querySelector('.selected-categories-hidden');
    if (container) {
        container.innerHTML = '';
        
        selectedCategories.forEach(category => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'SelectedCategoryIds';
            input.value = category.id;
            container.appendChild(input);
        });
    }
}

/**
 * カテゴリ選択のバリデーション
 * @returns {boolean} バリデーション結果
 */
function validateCategorySelection() {
    const fieldName = window.categoryFieldName || 'CategoryId';
    const isRequired = window.categoryFieldRequired || false;
    const selectedValue = document.querySelector(`input[name="${fieldName}"], select[name="${fieldName}"]`)?.value;
    
    const isValid = !isRequired || (selectedValue && selectedValue !== '');
    
    // バリデーションメッセージの表示
    const validationElement = document.querySelector('.category-validation-message');
    if (validationElement) {
        if (!isValid) {
            validationElement.textContent = 'カテゴリを選択してください';
            validationElement.style.display = 'block';
        } else {
            validationElement.style.display = 'none';
        }
    }
    
    return isValid;
}

// グローバル関数として公開
window.switchCategoryMode = switchCategoryMode;
window.filterCategories = filterCategories;
window.selectCategory = selectCategory;
window.clearCategorySelection = clearCategorySelection;
window.updateCategorySelection = updateCategorySelection;
window.updateMultipleSelection = updateMultipleSelection;