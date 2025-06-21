/**
 * 画像アップロードとプレビュー機能
 * 重複表示問題を解決したバージョン
 */

// 設定
const MAX_FILES = 5;
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/gif'];

// 選択されたファイルを管理
let selectedFiles = [];

/**
 * 初期化
 */
function initializeImageUploadWithPreview() {
    const fileInput = document.getElementById('imageFiles');
    const uploadZone = document.getElementById('imageUploadZone');
    
    if (!fileInput) return;

    // ファイル選択イベント
    fileInput.addEventListener('change', handleFileSelection);
    
    // ドラッグ&ドロップイベント
    if (uploadZone) {
        setupDragAndDrop(uploadZone);
    }
}

/**
 * ドラッグ&ドロップの設定
 */
function setupDragAndDrop(uploadZone) {
    uploadZone.addEventListener('dragover', function(e) {
        e.preventDefault();
        uploadZone.classList.add('drag-over');
    });
    
    uploadZone.addEventListener('dragleave', function(e) {
        e.preventDefault();
        uploadZone.classList.remove('drag-over');
    });
    
    uploadZone.addEventListener('drop', function(e) {
        e.preventDefault();
        uploadZone.classList.remove('drag-over');
        
        const files = e.dataTransfer.files;
        processFiles(Array.from(files));
    });
    
    // クリックでファイル選択
    uploadZone.addEventListener('click', function(e) {
        if (e.target.tagName !== 'BUTTON') {
            document.getElementById('imageFiles').click();
        }
    });
}

/**
 * ファイル選択処理
 */
function handleFileSelection(event) {
    const files = Array.from(event.target.files);
    processFiles(files);
}

/**
 * ファイル処理
 */
function processFiles(files) {
    // 現在の選択をクリア
    selectedFiles = [];
    
    // ファイル数チェック
    if (files.length > MAX_FILES) {
        showError(`画像は最大${MAX_FILES}枚まで選択可能です`);
        clearFileInput();
        return;
    }
    
    // ファイルバリデーションと処理
    const validFiles = [];
    const errors = [];
    
    files.forEach((file) => {
        const validation = validateFile(file);
        if (validation.isValid) {
            validFiles.push(file);
        } else {
            errors.push(`${file.name}: ${validation.error}`);
        }
    });
    
    // エラーがある場合は表示
    if (errors.length > 0) {
        showError('以下のファイルに問題があります：\n' + errors.join('\n'));
        if (validFiles.length === 0) {
            clearFileInput();
            return;
        }
    }
    
    // 有効なファイルを処理
    selectedFiles = validFiles;
    
    if (selectedFiles.length > 0) {
        displayImagePreviews();
        showSuccess(`${selectedFiles.length}枚の画像が選択されました`);
    } else {
        hideImagePreviews();
    }
}

/**
 * ファイルバリデーション
 */
function validateFile(file) {
    // ファイルタイプチェック
    if (!ALLOWED_TYPES.includes(file.type)) {
        return {
            isValid: false,
            error: '対応していないファイル形式です（JPEG、PNG、GIFのみ対応）'
        };
    }
    
    // ファイルサイズチェック
    if (file.size > MAX_FILE_SIZE) {
        return {
            isValid: false,
            error: `ファイルサイズが大きすぎます（最大${formatFileSize(MAX_FILE_SIZE)}）`
        };
    }
    
    return { isValid: true };
}

/**
 * 画像プレビュー表示
 */
function displayImagePreviews() {
    const previewArea = document.getElementById('imagePreviewArea');
    const previewContainer = document.getElementById('imagePreviewContainer');
    
    if (!previewArea || !previewContainer) return;
    
    previewArea.style.display = 'block';
    previewContainer.innerHTML = '';
    
    selectedFiles.forEach((file, index) => {
        createImagePreview(file, index, previewContainer);
    });
}

/**
 * 個別画像プレビュー作成
 */
function createImagePreview(file, index, container) {
    const reader = new FileReader();
    
    reader.onload = function(e) {
        const previewItem = document.createElement('div');
        previewItem.className = 'image-preview-item';
        previewItem.dataset.index = index;
        
        previewItem.innerHTML = `
            <div class="preview-image-container">
                <img src="${e.target.result}" alt="${file.name}" class="preview-image" />
                <div class="preview-overlay">
                    <button type="button" class="overlay-btn delete-btn" onclick="removeImage(${index})" title="削除">
                        🗑️
                    </button>
                    <button type="button" class="overlay-btn view-btn" onclick="viewImage('${e.target.result}', '${file.name}')" title="拡大表示">
                        🔍
                    </button>
                </div>
                ${index === 0 ? '<div class="main-badge">メイン</div>' : ''}
            </div>
            <div class="preview-info">
                <div class="preview-name">${file.name}</div>
                <div class="preview-details">
                    <span class="preview-size">${formatFileSize(file.size)}</span>
                    <span class="preview-order">順序: ${index + 1}</span>
                </div>
            </div>
        `;
        
        container.appendChild(previewItem);
    };
    
    reader.readAsDataURL(file);
}

/**
 * 画像削除
 */
function removeImage(index) {
    if (confirm('この画像を削除しますか？')) {
        selectedFiles.splice(index, 1);
        
        if (selectedFiles.length > 0) {
            displayImagePreviews();
            updateFileInput();
            showInfo('画像を削除しました');
        } else {
            clearAllImages();
        }
    }
}

/**
 * 画像拡大表示
 */
function viewImage(imageSrc, imageName) {
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
            <img src="${imageSrc}" alt="${imageName}" style="max-width: 100%; max-height: 100%; border-radius: 8px;">
            <div style="position: absolute; top: -40px; right: 0; color: white; font-size: 24px; cursor: pointer;">&times;</div>
        </div>
    `;
    
    modal.addEventListener('click', function() {
        document.body.removeChild(modal);
    });
    
    document.body.appendChild(modal);
}

/**
 * すべての画像をクリア
 */
function clearAllImages() {
    selectedFiles = [];
    hideImagePreviews();
    clearFileInput();
    showInfo('すべての画像を削除しました');
}

/**
 * プレビューエリアを非表示
 */
function hideImagePreviews() {
    const previewArea = document.getElementById('imagePreviewArea');
    if (previewArea) {
        previewArea.style.display = 'none';
    }
}

/**
 * ファイル入力を更新（選択されたファイルに同期）
 */
function updateFileInput() {
    const fileInput = document.getElementById('imageFiles');
    if (!fileInput) return;
    
    // FileListは直接編集できないため、新しいDataTransferを使用
    const dt = new DataTransfer();
    selectedFiles.forEach(file => dt.items.add(file));
    fileInput.files = dt.files;
}

/**
 * ファイル入力をクリア
 */
function clearFileInput() {
    const fileInput = document.getElementById('imageFiles');
    if (fileInput) {
        fileInput.value = '';
    }
}

/**
 * ファイルサイズフォーマット
 */
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * 成功メッセージ表示
 */
function showSuccess(message) {
    console.log('Success:', message);
    if (typeof window.showToast === 'function') {
        window.showToast(message, 'success');
    }
}

/**
 * エラーメッセージ表示
 */
function showError(message) {
    console.error('Error:', message);
    alert(message);
}

/**
 * 情報メッセージ表示
 */
function showInfo(message) {
    console.info('Info:', message);
    if (typeof window.showToast === 'function') {
        window.showToast(message, 'info');
    }
}

// 初期化
document.addEventListener('DOMContentLoaded', function() {
    initializeImageUploadWithPreview();
});

// グローバル関数として公開
window.clearAllImages = clearAllImages;
window.removeImage = removeImage;
window.viewImage = viewImage;