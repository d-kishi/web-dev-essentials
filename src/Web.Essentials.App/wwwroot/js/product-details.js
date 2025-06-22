/**
 * 商品詳細画面用JavaScript
 * 商品詳細表示・削除・複製・共有機能を提供
 */

/**
 * ページ読み込み時の初期化処理
 */
document.addEventListener('DOMContentLoaded', function() {
    initializeProductDetails();
});

/**
 * 商品詳細の初期化
 */
function initializeProductDetails() {
    // タブ機能の初期化
    setupTabs();
    
    
    // 画像ギャラリーの初期化
    setupImageGallery();
    
    // イベントリスナーの設定
    setupEventListeners();
    
    // ボタンイベントハンドラーの設定
    setupButtonEventHandlers();
}

/**
 * イベントリスナーの設定
 */
function setupEventListeners() {
    // 画像ビューアー関連
    document.addEventListener('click', function(e) {
        const target = e.target.closest('[data-action]');
        if (!target) return;
        
        const action = target.dataset.action;
        
        switch (action) {
            case 'open-image-viewer':
                e.preventDefault();
                openImageViewer(target.dataset.imagePath, target.dataset.altText);
                break;
                
            case 'switch-main-image':
                e.preventDefault();
                switchMainImage(target.dataset.imagePath, target.dataset.altText, target);
                break;
                
            case 'close-image-viewer':
                e.preventDefault();
                closeImageViewer();
                break;
                
            case 'share-product':
                e.preventDefault();
                shareProduct(target.dataset.productId, target.dataset.productName);
                break;
                
            case 'duplicate-product':
                e.preventDefault();
                duplicateProduct(target.dataset.productId);
                break;
                
            case 'copy-to-clipboard':
                e.preventDefault();
                copyToClipboard(target.dataset.copyText);
                break;
        }
    });
    
}

/**
 * 画像ギャラリーの初期化
 */
function setupImageGallery() {
    // 画像インタラクションなどの設定
}

/**
 * タブ機能の初期化
 */
function setupTabs() {
    // タブ関連の初期化処理
}

/**
 * タブ表示切り替え
 * @param {string} tabName - 表示するタブ名
 */
function showTab(tabName) {
    // すべてのタブボタンとコンテンツを非アクティブに
    document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
    
    // 選択されたタブをアクティブに
    const tabButton = document.querySelector(`[data-tab="${tabName}"]`);
    const tabContent = document.getElementById(tabName + 'Tab');
    
    if (tabButton) tabButton.classList.add('active');
    if (tabContent) tabContent.classList.add('active');
    
}

/**
 * メイン画像切り替え
 * @param {string} imagePath - 画像パス
 * @param {string} altText - 代替テキスト
 * @param {HTMLElement} thumbnailElement - サムネイル要素
 */
function switchMainImage(imagePath, altText, thumbnailElement) {
    const mainImage = document.querySelector('.main-product-image');
    if (mainImage) {
        mainImage.src = imagePath;
        mainImage.alt = altText;
    }
    
    // サムネイルのアクティブ状態を更新
    document.querySelectorAll('.thumbnail-item').forEach(item => {
        item.classList.remove('active');
    });
    if (thumbnailElement) {
        thumbnailElement.classList.add('active');
    }
}

/**
 * 画像ビューアーを開く（登録・編集画面と同じスタイル）
 * @param {string} imagePath - 画像パス
 * @param {string} altText - 代替テキスト
 */
function openImageViewer(imagePath, altText) {
    // シンプルなモーダル表示
    const modal = document.createElement('div');
    modal.className = 'image-view-modal';
    modal.style.cssText = `
        position: fixed; top: 0; left: 0; width: 100%; height: 100%;
        background-color: rgba(0,0,0,0.8); z-index: 1000;
        display: flex; align-items: center; justify-content: center;
        cursor: pointer;
    `;
    
    modal.innerHTML = `
        <div style="max-width: 90%; max-height: 90%; position: relative;">
            <img src="${imagePath}" alt="${altText}" style="max-width: 100%; max-height: 100%; border-radius: 8px;">
            <div style="position: absolute; top: -40px; right: 0; color: white; font-size: 24px; cursor: pointer;">&times;</div>
        </div>
    `;
    
    modal.addEventListener('click', function() {
        document.body.removeChild(modal);
        document.body.style.overflow = 'auto';
    });
    
    document.body.appendChild(modal);
    document.body.style.overflow = 'hidden';
}

/**
 * 画像ビューアーを閉じる（互換性のため残す）
 */
function closeImageViewer() {
    const modal = document.querySelector('.image-view-modal');
    if (modal) {
        document.body.removeChild(modal);
        document.body.style.overflow = 'auto';
    }
}

/**
 * 画像モーダル表示（互換性のため残す）
 * @param {string} imageSrc - 画像パス
 * @param {string} altText - 代替テキスト
 */
function showImageModal(imageSrc, altText) {
    openImageViewer(imageSrc, altText);
}

/**
 * 画像モーダルを閉じる（互換性のため残す）
 */
function closeImageModal() {
    closeImageViewer();
}

/**
 * 商品削除確認
 * @param {number} productId - 商品ID
 * @param {string} productName - 商品名
 */
function confirmDeleteProduct(productId, productName) {
    showConfirmationModal(
        '商品削除の確認',
        `商品「${productName}」を削除しますか？`,
        '削除すると元に戻せません。関連する画像やカテゴリ関連付けも削除されます。',
        '削除',
        () => deleteProduct(productId)
    );
}

/**
 * 商品削除実行
 * @param {number} productId - 商品ID
 */
async function deleteProduct(productId) {
    try {
        showLoadingModal('商品を削除しています...');
        
        const response = await fetch(`/Products/Delete/${productId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });
        
        hideLoadingModal();
        
        if (response.ok) {
            showSuccess('商品が正常に削除されました');
            
            // 商品一覧に戻る
            setTimeout(() => {
                window.location.href = '/Products';
            }, 1500);
        } else {
            const errorText = await response.text();
            showError('商品の削除に失敗しました: ' + errorText);
        }
    } catch (error) {
        hideLoadingModal();
        console.error('商品削除エラー:', error);
        showError('商品削除中にエラーが発生しました');
    }
}

/**
 * 商品複製
 * @param {number|string} productId - 商品ID
 */
async function duplicateProduct(productId) {
    if (!productId) {
        console.error('商品IDが指定されていません');
        return;
    }
    
    if (!confirm('この商品を複製して新しい商品を作成しますか？')) {
        return;
    }
    
    try {
        if (typeof showLoadingModal === 'function') {
            showLoadingModal('商品を複製しています...');
        }
        
        // 複製処理の実装（実際のAPI呼び出しが必要な場合）
        // const response = await fetch(`/Products/Duplicate/${productId}`, {
        //     method: 'POST',
        //     headers: {
        //         'Content-Type': 'application/json',
        //         'RequestVerificationToken': getAntiForgeryToken()
        //     }
        // });
        
        // 暫定的に複製先ページに移動
        window.location.href = `/Products/Create?duplicateFrom=${productId}`;
        
    } catch (error) {
        if (typeof hideLoadingModal === 'function') {
            hideLoadingModal();
        }
        console.error('商品複製エラー:', error);
        if (typeof showError === 'function') {
            showError('商品複製中にエラーが発生しました');
        }
    }
}

/**
 * 商品共有
 * @param {string} productId - 商品ID
 * @param {string} productName - 商品名
 */
function shareProduct(productId, productName) {
    const url = productId ? 
        `${window.location.origin}/Products/Details/${productId}` : 
        window.location.href;
    const name = productName || window.productName || '商品';
    const shareText = `商品「${name}」の詳細をご覧ください`;
    
    if (navigator.share) {
        // ネイティブ共有API使用
        navigator.share({
            title: name,
            text: shareText,
            url: url
        }).catch(error => {
            console.log('共有がキャンセルされました:', error);
        });
    } else {
        // フォールバック: クリップボードにコピー
        copyToClipboard(url);
        if (typeof showSuccess === 'function') {
            showSuccess('商品URLをクリップボードにコピーしました');
        }
    }
}

/**
 * クリップボードにコピー
 * @param {string} text - コピーするテキスト
 */
async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        showSuccess('クリップボードにコピーしました');
    } catch (error) {
        console.error('クリップボードコピーエラー:', error);
        
        // フォールバック
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        
        showSuccess('クリップボードにコピーしました');
    }
}



// ESCキーで画像ビューアーを閉じる
document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        closeImageViewer();
    }
});

/**
 * ボタンイベントハンドラーの設定
 */
function setupButtonEventHandlers() {
    // 商品削除ボタン（IDとdata属性ベース）
    const deleteButton = document.getElementById('deleteProductButton');
    if (deleteButton) {
        deleteButton.addEventListener('click', function() {
            const productId = deleteButton.dataset.productId;
            const productName = deleteButton.dataset.productName;
            confirmDeleteProduct(parseInt(productId), productName);
        });
    }
    
    // 画像モーダル表示ボタン（クラスとdata属性ベース）
    const openImageButtons = document.querySelectorAll('.open-image-modal');
    openImageButtons.forEach(button => {
        button.addEventListener('click', function() {
            const imagePath = button.dataset.imagePath;
            const altText = button.dataset.altText || '';
            openImageModal(imagePath, altText);
        });
    });
    
    // メイン画像変更ボタン（クラスとdata属性ベース）
    const changeMainImageButtons = document.querySelectorAll('.change-main-image');
    changeMainImageButtons.forEach(button => {
        button.addEventListener('click', function() {
            const imagePath = button.dataset.imagePath;
            const altText = button.dataset.altText || '';
            changeMainImage(imagePath, altText, button);
        });
    });
    
    // 商品複製ボタン（IDとdata属性ベース）
    const duplicateButton = document.getElementById('duplicateProductButton');
    if (duplicateButton) {
        duplicateButton.addEventListener('click', function() {
            const productId = duplicateButton.dataset.productId;
            duplicateProduct(parseInt(productId));
        });
    }
    
    // 商品共有ボタン（IDとdata属性ベース）
    const shareButton = document.getElementById('shareProductButton');
    if (shareButton) {
        shareButton.addEventListener('click', function() {
            const productId = shareButton.dataset.productId;
            const productName = shareButton.dataset.productName;
            shareProduct(productId, productName);
        });
    }
    
    // タブ表示切り替えボタン（クラスとdata属性ベース）
    const tabButtons = document.querySelectorAll('.tab-button');
    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            const tabName = button.dataset.tab;
            if (tabName) {
                showTab(tabName);
            }
        });
    });
    
    // クリップボードコピーボタン（クラスとdata属性ベース）
    const copyButtons = document.querySelectorAll('.copy-to-clipboard');
    copyButtons.forEach(button => {
        button.addEventListener('click', function() {
            const copyText = button.dataset.copyText;
            copyToClipboard(copyText);
        });
    });
    
    // 画像モーダル閉じるボタン（クラスベース）
    const closeImageButtons = document.querySelectorAll('.close-image-modal');
    closeImageButtons.forEach(button => {
        button.addEventListener('click', closeImageModal);
    });
}

// 関数名の互換性のためのエイリアス
function openImageModal(imagePath, altText) {
    openImageViewer(imagePath, altText);
}

function changeMainImage(imagePath, altText, element) {
    switchMainImage(imagePath, altText, element);
}

// 互換性のためのグローバル関数公開（段階的に削除予定）
// 新しいID/class/data属性アプローチに移行済み
window.showTab = showTab;
window.showImageModal = showImageModal;
window.closeImageModal = closeImageModal;
window.openImageViewer = openImageViewer;
window.closeImageViewer = closeImageViewer;
window.switchMainImage = switchMainImage;
window.changeMainImage = changeMainImage;
window.openImageModal = openImageModal;
window.confirmDeleteProduct = confirmDeleteProduct;
window.deleteProduct = deleteProduct;
window.duplicateProduct = duplicateProduct;
window.shareProduct = shareProduct;
window.copyToClipboard = copyToClipboard;
