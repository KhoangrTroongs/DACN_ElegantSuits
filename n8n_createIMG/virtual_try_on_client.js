/**
 * Virtual Try-On Client Script
 * Thêm vào trang chi tiết sản phẩm của bạn
 */

class VirtualTryOn {
  constructor(config = {}) {
    this.webhookUrl = config.webhookUrl || 'https://your-n8n-domain.com/webhook/virtual-try-on';
    this.productName = config.productName || this.getProductNameFromPage();
    this.userEmail = config.userEmail || this.getUserEmail();
    this.callbackUrl = config.callbackUrl || window.location.href;
    
    this.init();
  }

  init() {
    this.createTryOnButton();
    this.createFileInput();
    this.attachEventListeners();
  }

  createTryOnButton() {
    const btn = document.createElement('button');
    btn.id = 'virtualTryOnBtn';
    btn.className = 'btn btn-primary btn-lg';
    btn.innerHTML = '👕 Thử Đồ';
    btn.style.cssText = `
      margin: 10px 0;
      padding: 12px 24px;
      font-size: 16px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;
    `;
    
    btn.addEventListener('mouseover', () => {
      btn.style.transform = 'translateY(-2px)';
      btn.style.boxShadow = '0 10px 20px rgba(102, 126, 234, 0.4)';
    });
    
    btn.addEventListener('mouseout', () => {
      btn.style.transform = 'translateY(0)';
      btn.style.boxShadow = 'none';
    });

    // Thêm button vào trang (tìm vị trí phù hợp)
    const productSection = document.querySelector('[data-product-actions]') || 
                          document.querySelector('.product-actions') ||
                          document.querySelector('.product-details');
    
    if (productSection) {
      productSection.appendChild(btn);
    }
  }

  createFileInput() {
    const input = document.createElement('input');
    input.id = 'virtualTryOnFileInput';
    input.type = 'file';
    input.accept = 'image/*';
    input.style.display = 'none';
    document.body.appendChild(input);
  }

  attachEventListeners() {
    const btn = document.getElementById('virtualTryOnBtn');
    const input = document.getElementById('virtualTryOnFileInput');

    btn.addEventListener('click', () => {
      input.click();
    });

    input.addEventListener('change', (e) => {
      this.handleImageUpload(e);
    });
  }

  handleImageUpload(event) {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file
    if (!this.validateFile(file)) return;

    // Show loading
    this.showLoading();

    // Convert to base64
    const reader = new FileReader();
    reader.onload = (e) => {
      const base64Image = e.target.result.split(',')[1];
      this.sendToN8N(base64Image);
    };
    reader.readAsDataURL(file);
  }

  validateFile(file) {
    const maxSize = 10 * 1024 * 1024; // 10MB
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];

    if (file.size > maxSize) {
      this.showError('Ảnh quá lớn (tối đa 10MB)');
      return false;
    }

    if (!allowedTypes.includes(file.type)) {
      this.showError('Định dạng ảnh không hỗ trợ (JPEG, PNG, WebP)');
      return false;
    }

    return true;
  }

  async sendToN8N(base64Image) {
    try {
      const payload = {
        customerImage: base64Image,
        productName: this.productName,
        email: this.userEmail,
        callbackUrl: this.callbackUrl
      };

      console.log('Sending to N8N:', {
        ...payload,
        customerImage: '[base64_image_data]'
      });

      const response = await fetch(this.webhookUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      const result = await response.json();

      if (result.status === 'success') {
        this.showSuccess(result.downloadUrl);
      } else {
        this.showError(result.message || 'Lỗi không xác định');
      }
    } catch (error) {
      console.error('Error:', error);
      this.showError('Lỗi kết nối. Vui lòng thử lại sau.');
    } finally {
      this.hideLoading();
    }
  }

  getProductNameFromPage() {
    // Tìm tên sản phẩm từ trang
    const nameElement = document.querySelector('[data-product-name]') ||
                       document.querySelector('h1.product-name') ||
                       document.querySelector('.product-title');
    
    return nameElement ? nameElement.textContent.trim() : 'Unknown Product';
  }

  getUserEmail() {
    // Lấy email từ user đã login
    const emailElement = document.querySelector('[data-user-email]');
    return emailElement ? emailElement.textContent : 'customer@example.com';
  }

  showLoading() {
    const loader = document.createElement('div');
    loader.id = 'virtualTryOnLoader';
    loader.innerHTML = `
      <div style="
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0,0,0,0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 9999;
      ">
        <div style="
          background: white;
          padding: 30px;
          border-radius: 10px;
          text-align: center;
        ">
          <div style="
            border: 4px solid #f3f3f3;
            border-top: 4px solid #667eea;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 0 auto 15px;
          "></div>
          <p style="color: #667eea; font-weight: bold;">Đang xử lý ảnh của bạn...</p>
          <p style="color: #999; font-size: 12px;">Vui lòng chờ (thường mất 10-30 giây)</p>
        </div>
      </div>
      <style>
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      </style>
    `;
    document.body.appendChild(loader);
  }

  hideLoading() {
    const loader = document.getElementById('virtualTryOnLoader');
    if (loader) loader.remove();
  }

  showSuccess(downloadUrl) {
    const message = document.createElement('div');
    message.innerHTML = `
      <div style="
        position: fixed;
        top: 20px;
        right: 20px;
        background: #d4edda;
        color: #155724;
        padding: 20px;
        border-radius: 8px;
        border: 1px solid #c3e6cb;
        z-index: 10000;
        max-width: 400px;
      ">
        <h4 style="margin-top: 0;">✅ Thành công!</h4>
        <p>Ảnh thử đồ của bạn đã được tạo. Kiểm tra email hoặc:</p>
        <a href="${downloadUrl}" target="_blank" style="
          display: inline-block;
          background: #28a745;
          color: white;
          padding: 10px 20px;
          border-radius: 5px;
          text-decoration: none;
          margin-top: 10px;
        ">Tải Ảnh Về</a>
      </div>
    `;
    document.body.appendChild(message);

    setTimeout(() => message.remove(), 10000);
  }

  showError(errorMessage) {
    const message = document.createElement('div');
    message.innerHTML = `
      <div style="
        position: fixed;
        top: 20px;
        right: 20px;
        background: #f8d7da;
        color: #721c24;
        padding: 20px;
        border-radius: 8px;
        border: 1px solid #f5c6cb;
        z-index: 10000;
        max-width: 400px;
      ">
        <h4 style="margin-top: 0;">❌ Lỗi</h4>
        <p>${errorMessage}</p>
      </div>
    `;
    document.body.appendChild(message);

    setTimeout(() => message.remove(), 5000);
  }
}

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', () => {
  new VirtualTryOn({
    webhookUrl: 'https://your-n8n-domain.com/webhook/virtual-try-on',
    // productName: 'Áo Thun Xanh', // Optional - tự động lấy từ trang
    // userEmail: 'customer@example.com' // Optional - tự động lấy từ login
  });
});

// Hoặc khởi tạo thủ công:
// const tryOn = new VirtualTryOn({
//   webhookUrl: 'https://your-n8n-domain.com/webhook/virtual-try-on',
//   productName: 'Áo Thun Xanh',
//   userEmail: 'customer@example.com'
// });

