/**
 * Pure JavaScript Form Validation
 * ASP.NET Core data annotation attributes対応
 */

class FormValidator {
    constructor() {
        this.init();
    }

    init() {
        // DOMが読み込まれた後に初期化
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.setupValidation());
        } else {
            this.setupValidation();
        }
    }

    setupValidation() {
        // すべてのフォームにバリデーションを設定
        const forms = document.querySelectorAll('form');
        forms.forEach(form => this.setupFormValidation(form));
    }

    setupFormValidation(form) {
        // リアルタイムバリデーション
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => this.clearFieldError(input));
        });

        // フォーム送信時バリデーション
        form.addEventListener('submit', (e) => {
            if (!this.validateForm(form)) {
                e.preventDefault();
                e.stopPropagation();
            }
        });
    }

    validateForm(form) {
        let isValid = true;
        const inputs = form.querySelectorAll('input, textarea, select');
        
        inputs.forEach(input => {
            if (!this.validateField(input)) {
                isValid = false;
            }
        });

        return isValid;
    }

    validateField(field) {
        this.clearFieldError(field);
        
        const validators = this.getValidators(field);
        let isValid = true;

        for (const validator of validators) {
            const result = validator.validate(field.value, field);
            if (!result.isValid) {
                this.showFieldError(field, result.message);
                isValid = false;
                break;
            }
        }

        return isValid;
    }

    getValidators(field) {
        const validators = [];

        // Required validation
        if (field.hasAttribute('data-val-required') || field.required) {
            validators.push({
                validate: (value) => ({
                    isValid: value.trim() !== '',
                    message: field.getAttribute('data-val-required') || 'この項目は必須です。'
                })
            });
        }

        // StringLength validation
        if (field.hasAttribute('data-val-length')) {
            const min = parseInt(field.getAttribute('data-val-length-min')) || 0;
            const max = parseInt(field.getAttribute('data-val-length-max')) || Number.MAX_VALUE;
            validators.push({
                validate: (value) => ({
                    isValid: value.length >= min && value.length <= max,
                    message: field.getAttribute('data-val-length') || `文字数は${min}文字以上${max}文字以下で入力してください。`
                })
            });
        }

        // Range validation
        if (field.hasAttribute('data-val-range')) {
            const min = parseFloat(field.getAttribute('data-val-range-min'));
            const max = parseFloat(field.getAttribute('data-val-range-max'));
            validators.push({
                validate: (value) => {
                    const numValue = parseFloat(value);
                    return {
                        isValid: !isNaN(numValue) && numValue >= min && numValue <= max,
                        message: field.getAttribute('data-val-range') || `値は${min}以上${max}以下で入力してください。`
                    };
                }
            });
        }

        // RegularExpression validation
        if (field.hasAttribute('data-val-regex')) {
            const pattern = field.getAttribute('data-val-regex-pattern');
            validators.push({
                validate: (value) => {
                    const regex = new RegExp(pattern);
                    return {
                        isValid: regex.test(value),
                        message: field.getAttribute('data-val-regex') || '入力形式が正しくありません。'
                    };
                }
            });
        }

        // Email validation
        if (field.type === 'email' || field.hasAttribute('data-val-email')) {
            validators.push({
                validate: (value) => {
                    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                    return {
                        isValid: value === '' || emailRegex.test(value),
                        message: field.getAttribute('data-val-email') || '有効なメールアドレスを入力してください。'
                    };
                }
            });
        }

        // Number validation
        if (field.type === 'number' || field.hasAttribute('data-val-number')) {
            validators.push({
                validate: (value) => {
                    return {
                        isValid: value === '' || !isNaN(parseFloat(value)),
                        message: field.getAttribute('data-val-number') || '数値を入力してください。'
                    };
                }
            });
        }

        return validators;
    }

    showFieldError(field, message) {
        // エラーメッセージ要素を探す
        let errorElement = document.querySelector(`[data-valmsg-for="${field.name}"]`);
        
        if (!errorElement) {
            // エラーメッセージ要素がない場合は作成
            errorElement = document.createElement('span');
            errorElement.className = 'field-validation-error';
            errorElement.setAttribute('data-valmsg-for', field.name);
            errorElement.setAttribute('data-valmsg-replace', 'true');
            
            // フィールドの後に挿入
            field.parentNode.insertBefore(errorElement, field.nextSibling);
        }

        errorElement.textContent = message;
        errorElement.className = 'field-validation-error';
        field.classList.add('input-validation-error');
    }

    clearFieldError(field) {
        const errorElement = document.querySelector(`[data-valmsg-for="${field.name}"]`);
        if (errorElement) {
            errorElement.textContent = '';
            errorElement.className = 'field-validation-valid';
        }
        field.classList.remove('input-validation-error');
    }
}

// グローバルインスタンスを作成
const formValidator = new FormValidator();

// パブリックAPI（必要に応じて使用）
window.FormValidator = {
    validateForm: (form) => formValidator.validateForm(form),
    validateField: (field) => formValidator.validateField(field)
};