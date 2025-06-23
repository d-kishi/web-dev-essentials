/**
 * カテゴリ階層表示用JavaScript
 * ツリー構造での階層表示、展開/縮小、キーボードナビゲーション機能を提供
 */

/**
 * カテゴリ階層の初期化
 */
function initializeCategoryHierarchy() {
    setupTreeInteractions();
    setupKeyboardNavigation();
    setupTreeNodeEventHandlers();
}

/**
 * ツリー操作の設定
 */
function setupTreeInteractions() {
    // showActionsの値は外部から設定される
    if (window.showCategoryActions) {
        setupTreeSorting();
    }
}

/**
 * ツリーノード展開/縮小
 * @param {string} nodeId - ノードのID
 */
function toggleTreeNode(nodeId) {
    const node = document.getElementById(nodeId);
    if (!node) return;
    
    const toggle = node.querySelector('.tree-toggle');
    const children = node.querySelector('.tree-children');
    
    if (!children) return;
    
    const isExpanded = children.style.display === 'block';
    
    if (isExpanded) {
        // 縮小
        children.style.display = 'none';
        if (toggle) {
            toggle.textContent = '▶';
            toggle.title = '展開';
        }
        node.classList.add('collapsed');
    } else {
        // 展開
        children.style.display = 'block';
        if (toggle) {
            toggle.textContent = '▼';
            toggle.title = '縮小';
        }
        node.classList.remove('collapsed');
    }
    
    // アニメーション効果
    children.style.maxHeight = isExpanded ? '0' : children.scrollHeight + 'px';
}

/**
 * すべてのノードを展開
 */
function expandAllNodes() {
    document.querySelectorAll('.tree-item.expandable').forEach(node => {
        const children = node.querySelector('.tree-children');
        const toggle = node.querySelector('.tree-toggle');
        
        if (children && children.style.display !== 'block') {
            children.style.display = 'block';
            if (toggle) {
                toggle.textContent = '▼';
                toggle.title = '縮小';
            }
            node.classList.remove('collapsed');
        }
    });
}

/**
 * すべてのノードを縮小
 */
function collapseAllNodes() {
    document.querySelectorAll('.tree-item.expandable').forEach(node => {
        const children = node.querySelector('.tree-children');
        const toggle = node.querySelector('.tree-toggle');
        
        if (children && children.style.display === 'block') {
            children.style.display = 'none';
            if (toggle) {
                toggle.textContent = '▶';
                toggle.title = '展開';
            }
            node.classList.add('collapsed');
        }
    });
}

/**
 * カテゴリ選択（選択可能モードの場合）
 * @param {number} categoryId - カテゴリID
 * @param {string} categoryName - カテゴリ名
 * @param {string} fullPath - 階層パス
 */
function selectCategory(categoryId, categoryName, fullPath) {
    // isCategorySelectableの値は外部から設定される
    if (window.isCategorySelectable) {
        // 他の選択を解除
        document.querySelectorAll('.tree-label.selected').forEach(label => {
            label.classList.remove('selected');
        });
        
        // 新しい選択を設定
        const targetLabel = event.target.closest('.tree-label');
        if (targetLabel) {
            targetLabel.classList.add('selected');
        }
        
        // 選択値を隠しフィールドに設定
        const hiddenField = document.querySelector('input[type="hidden"][name*="CategoryId"]');
        if (hiddenField) {
            hiddenField.value = categoryId;
        }
        
        // 選択イベントを発火
        const selectEvent = new CustomEvent('categorySelected', {
            detail: {
                id: categoryId,
                name: categoryName,
                fullPath: fullPath
            }
        });
        document.dispatchEvent(selectEvent);
    }
}

/**
 * 説明表示切り替え
 * @param {number} categoryId - カテゴリID
 */
function toggleDescription(categoryId) {
    const description = document.getElementById(`desc-${categoryId}`);
    if (!description) return;
    
    const isVisible = description.style.display === 'block';
    
    description.style.display = isVisible ? 'none' : 'block';
    
    // ボタンの状態更新
    if (event && event.target) {
        const button = event.target;
        button.title = isVisible ? '説明を表示' : '説明を非表示';
        button.style.opacity = isVisible ? '0.7' : '1';
    }
}

/**
 * キーボードナビゲーションの設定
 */
function setupKeyboardNavigation() {
    document.addEventListener('keydown', function(event) {
        const focusedElement = document.activeElement;
        
        if (focusedElement.classList.contains('tree-label')) {
            const currentNode = focusedElement.closest('.tree-item');
            
            switch (event.key) {
                case 'ArrowUp':
                    event.preventDefault();
                    navigateToPrevious(currentNode);
                    break;
                case 'ArrowDown':
                    event.preventDefault();
                    navigateToNext(currentNode);
                    break;
                case 'ArrowRight':
                    event.preventDefault();
                    expandNode(currentNode);
                    break;
                case 'ArrowLeft':
                    event.preventDefault();
                    collapseNode(currentNode);
                    break;
                case 'Enter':
                case ' ':
                    event.preventDefault();
                    if (window.isCategorySelectable) {
                        focusedElement.click();
                    }
                    break;
            }
        }
    });
}

/**
 * 前のノードに移動
 * @param {HTMLElement} currentNode - 現在のノード
 */
function navigateToPrevious(currentNode) {
    const allLabels = Array.from(document.querySelectorAll('.tree-label'));
    const currentLabel = currentNode.querySelector('.tree-label');
    const currentIndex = allLabels.indexOf(currentLabel);
    
    if (currentIndex > 0) {
        allLabels[currentIndex - 1].focus();
    }
}

/**
 * 次のノードに移動
 * @param {HTMLElement} currentNode - 現在のノード
 */
function navigateToNext(currentNode) {
    const allLabels = Array.from(document.querySelectorAll('.tree-label'));
    const currentLabel = currentNode.querySelector('.tree-label');
    const currentIndex = allLabels.indexOf(currentLabel);
    
    if (currentIndex < allLabels.length - 1) {
        allLabels[currentIndex + 1].focus();
    }
}

/**
 * ノード展開
 * @param {HTMLElement} node - 対象ノード
 */
function expandNode(node) {
    const toggle = node.querySelector('.tree-toggle');
    const children = node.querySelector('.tree-children');
    
    if (toggle && children && children.style.display !== 'block') {
        toggle.click();
    }
}

/**
 * ノード縮小
 * @param {HTMLElement} node - 対象ノード
 */
function collapseNode(node) {
    const toggle = node.querySelector('.tree-toggle');
    const children = node.querySelector('.tree-children');
    
    if (toggle && children && children.style.display === 'block') {
        toggle.click();
    }
}

/**
 * ツリーソート機能（管理画面用）
 */
function setupTreeSorting() {
    // ドラッグ&ドロップによる並び替え実装
    // 実装は複雑になるため、基本機能のみ
    console.log('Tree sorting setup - 実装予定');
}

/**
 * カテゴリ削除確認
 * @param {number} categoryId - カテゴリID
 * @param {string} categoryName - カテゴリ名
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
        
        showError(message);
        return;
    }
    
    if (confirm(`カテゴリ「${categoryName}」を削除しますか？\n\nこの操作は元に戻せません。`)) {
        // 削除処理を実行
        deleteCategory(categoryId);
    }
}

/**
 * カテゴリ削除実行
 * @param {number} categoryId - カテゴリID
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
            
            // ノードを画面から削除
            const node = document.querySelector(`[data-category-id="${categoryId}"]`);
            if (node) {
                node.remove();
            }
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
 * ツリーノードのイベントハンドラーを設定
 */
function setupTreeNodeEventHandlers() {
    // ツリーノード展開/縮小ボタン（クラスとdata属性ベース）
    const toggleButtons = document.querySelectorAll('.tree-toggle');
    toggleButtons.forEach(button => {
        button.addEventListener('click', () => {
            const nodeId = button.dataset.nodeId;
            if (nodeId) {
                toggleTreeNode(nodeId);
            }
        });
    });

    // カテゴリ選択ボタン（クラスとdata属性ベース）
    const selectButtons = document.querySelectorAll('.category-select-button');
    selectButtons.forEach(button => {
        button.addEventListener('click', () => {
            const categoryId = button.dataset.categoryId;
            const categoryName = button.dataset.categoryName;
            const categoryPath = button.dataset.categoryPath;
            
            if (categoryId) {
                selectCategory(parseInt(categoryId), categoryName || '', categoryPath || '');
            }
        });
    });

    // 説明表示切り替えボタン（クラスとdata属性ベース）
    const descToggleButtons = document.querySelectorAll('.description-toggle');
    descToggleButtons.forEach(button => {
        button.addEventListener('click', () => {
            const categoryId = button.dataset.categoryId;
            if (categoryId) {
                toggleDescription(categoryId);
            }
        });
    });

    // カテゴリ削除ボタン（クラスとdata属性ベース）
    const deleteButtons = document.querySelectorAll('.category-delete-button');
    deleteButtons.forEach(button => {
        button.addEventListener('click', () => {
            const categoryId = button.dataset.categoryId;
            const categoryName = button.dataset.categoryName;
            const productCount = button.dataset.productCount;
            const hasChildren = button.dataset.hasChildren;
            
            if (categoryId) {
                confirmDeleteCategory(
                    parseInt(categoryId), 
                    categoryName || '', 
                    parseInt(productCount) || 0, 
                    hasChildren === 'true'
                );
            }
        });
    });
}

// 初期化（ページ読み込み時に呼び出し）
// 新しいID/class/data属性アプローチに移行済み
document.addEventListener('DOMContentLoaded', function() {
    initializeCategoryHierarchy();
});