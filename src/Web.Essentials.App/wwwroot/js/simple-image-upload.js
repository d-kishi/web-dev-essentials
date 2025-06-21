/**
 * シンプルな画像アップロード機能
 * プレビューなし、ファイル選択状況表示のみ
 */

// 設定
const MAX_FILES = 5;
const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/gif'];

/**
 * 初期化
 */
function initializeSimpleImageUpload() {
    const fileInput = document.getElementById('imageFiles');
    if (!fileInput) return;

    fileInput.addEventListener('change', handleFileSelection);
    
    // クリアボタンの設定
    const clearButton = document.getElementById('clearFilesButton');
    if (clearButton) {
        clearButton.addEventListener('click', clearFileSelection);
    }
}

/**
 * ファイル選択処理
 */
function handleFileSelection(event) {
    const files = event.target.files;
    const fileArray = Array.from(files);
    
    // ファイル数チェック
    if (fileArray.length > MAX_FILES) {
        showError(`画像は最大${MAX_FILES}枚まで選択可能です`);
        clearFileInput();
        return;
    }
    
    // ファイルバリデーション
    const validFiles = [];
    const errors = [];
    
    fileArray.forEach((file, index) => {
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
    
    // 選択状況を表示
    displaySelectedFiles(validFiles);
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
 * 選択されたファイルの表示
 */
function displaySelectedFiles(files) {
    const selectedFilesArea = document.getElementById('selectedFilesArea');
    const selectedFilesList = document.getElementById('selectedFilesList');
    
    if (!selectedFilesArea || !selectedFilesList) return;
    
    if (files.length === 0) {
        selectedFilesArea.style.display = 'none';
        return;
    }
    
    selectedFilesArea.style.display = 'block';
    
    const filesHtml = files.map((file, index) => `
        <div class="selected-file-item">
            <div class="file-icon">📷</div>
            <div class="file-info">
                <div class="file-name">${file.name}</div>
                <div class="file-details">
                    <span class="file-size">${formatFileSize(file.size)}</span>
                    <span class="file-type">${getFileTypeLabel(file.type)}</span>
                </div>
            </div>
            <div class="file-status">
                <span class="status-valid">✓</span>
            </div>
        </div>
    `).join('');
    
    selectedFilesList.innerHTML = filesHtml;
    
    // 成功メッセージ
    showSuccess(`${files.length}枚の画像が選択されました`);
}

/**
 * ファイル選択をクリア
 */
function clearFileSelection() {
    clearFileInput();
    const selectedFilesArea = document.getElementById('selectedFilesArea');
    if (selectedFilesArea) {
        selectedFilesArea.style.display = 'none';
    }
    showInfo('ファイル選択をクリアしました');
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
 * ファイルタイプラベル取得
 */
function getFileTypeLabel(mimeType) {
    switch (mimeType) {
        case 'image/jpeg':
            return 'JPEG';
        case 'image/png':
            return 'PNG';
        case 'image/gif':
            return 'GIF';
        default:
            return '画像';
    }
}

/**
 * 成功メッセージ表示
 */
function showSuccess(message) {
    console.log('Success:', message);
    // 実際の実装では、適切なメッセージ表示システムを使用
    if (typeof window.showToast === 'function') {
        window.showToast(message, 'success');
    }
}

/**
 * エラーメッセージ表示
 */
function showError(message) {
    console.error('Error:', message);
    alert(message); // 簡単な実装、実際にはより良いUI表示を使用
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
    initializeSimpleImageUpload();
});

// グローバル関数として公開
window.clearFileSelection = clearFileSelection;