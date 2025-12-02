# Hướng dẫn Setup Virtual Try-On với n8n và Gemini AI

## 📋 Yêu cầu

- Node.js 18+ hoặc Docker
- Tài khoản Google Cloud với Gemini API enabled
- n8n (sẽ cài đặt bên dưới)

---

## 🚀 Bước 1: Cài đặt n8n

### Cách 1: Sử dụng npm (Khuyến nghị cho development)

```bash
# Cài đặt n8n globally
npm install n8n -g

# Khởi động n8n
n8n start
```

### Cách 2: Sử dụng Docker

```bash
docker run -it --rm --name n8n -p 5678:5678 -v ~/.n8n:/home/node/.n8n n8nio/n8n
```

Sau khi chạy, truy cập: **http://localhost:5678**

---

## 🔑 Bước 2: Lấy Gemini API Key

1. Truy cập: https://makersuite.google.com/app/apikey
2. Click **"Create API Key"**
3. Chọn project hoặc tạo project mới
4. Copy API key

---

## ⚙️ Bước 3: Import Workflow vào n8n

1. Mở n8n tại `http://localhost:5678`
2. Đăng ký/Đăng nhập tài khoản
3. Click **"Add workflow"** → **"Import from File"**
4. Chọn file: `n8n-virtual-tryon-workflow.json`
5. Workflow sẽ được import

---

## 🔐 Bước 4: Cấu hình Gemini Credentials

1. Trong n8n, click vào node **"Gemini AI - Generate Try-On"**
2. Click **"Credentials"** → **"Create New"**
3. Chọn **"Google Gemini API"**
4. Nhập:
   - **Name**: `Gemini Virtual Try-On`
   - **API Key**: Paste API key từ bước 2
5. Click **"Save"**

---

## 🧪 Bước 5: Test Workflow

### 5.1. Activate Webhook

1. Click vào node **"Webhook"** đầu tiên
2. Click **"Listen for Test Event"**
3. Copy **Production URL** (ví dụ: `http://localhost:5678/webhook/virtual-try-on`)

### 5.2. Test với Postman hoặc cURL

```bash
curl -X POST http://localhost:5678/webhook/virtual-try-on \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "Áo blazer xanh navy",
    "productImageUrl": "https://example.com/product.jpg",
    "customerImage": "data:image/jpeg;base64,/9j/4AAQSkZJRg..."
  }'
```

### 5.3. Kiểm tra Response

Response mong đợi:
```json
{
  "success": true,
  "resultImage": "data:image/png;base64,iVBORw0KGgo...",
  "message": "Virtual try-on generated successfully"
}
```

---

## 🔧 Bước 6: Cập nhật Frontend

Trong file `_VirtualTryOnModal.cshtml`, dòng 224, thay:

```javascript
const n8nWebhookUrl = 'http://localhost:5678/webhook/virtual-try-on';
```

Bằng Production URL từ n8n (nếu deploy lên server).

---

## 🌐 Bước 7: Deploy n8n (Production)

### Cách 1: Deploy lên Railway.app (Free tier)

1. Truy cập: https://railway.app
2. Click **"Start a New Project"** → **"Deploy n8n"**
3. Sau khi deploy, copy URL (ví dụ: `https://your-app.railway.app`)
4. Cập nhật webhook URL trong frontend

### Cách 2: Deploy lên Render.com

1. Truy cập: https://render.com
2. Click **"New"** → **"Web Service"**
3. Connect GitHub repo hoặc deploy từ Docker image: `n8nio/n8n`
4. Set environment variables:
   - `N8N_BASIC_AUTH_ACTIVE=true`
   - `N8N_BASIC_AUTH_USER=admin`
   - `N8N_BASIC_AUTH_PASSWORD=your-password`
5. Deploy và copy URL

---

## 🎨 Bước 8: Tối ưu hóa Gemini Prompt (Tùy chọn)

Để cải thiện chất lượng ảnh, bạn có thể chỉnh sửa prompt trong node **"Gemini AI - Generate Try-On"**:

```
You are an expert AI fashion stylist and image compositor.

Task: Create a photorealistic image of the customer wearing the product.

Input:
- Customer photo: Full body or upper body shot
- Product: {{ $('Process Input').item.json.productName }}

Requirements:
1. Seamlessly overlay the product onto the customer
2. Match lighting, shadows, and color temperature
3. Maintain natural body proportions and posture
4. Preserve the customer's pose and background
5. Ensure the product fits realistically (wrinkles, folds, draping)
6. High resolution output (minimum 1024x1024)

Style: Photorealistic, professional fashion photography
```

---

## 🐛 Troubleshooting

### Lỗi: "Webhook not found"
- Đảm bảo workflow đã được **Activate** (toggle ở góc trên phải)
- Kiểm tra webhook path: `/webhook/virtual-try-on`

### Lỗi: "Gemini API quota exceeded"
- Gemini free tier có giới hạn: 60 requests/minute
- Nâng cấp lên paid plan hoặc thêm rate limiting

### Lỗi: "CORS blocked"
- Đã thêm `Access-Control-Allow-Origin: *` trong response headers
- Nếu vẫn lỗi, cần setup CORS proxy hoặc deploy n8n cùng domain với ASP.NET app

### Ảnh kết quả không đẹp
- Thử các model khác: `gemini-1.5-pro`, `gemini-1.5-flash-8b`
- Điều chỉnh temperature (0.4-0.9)
- Cải thiện prompt với ví dụ cụ thể

---

## 📊 Monitoring và Logging

1. Trong n8n, click **"Executions"** để xem lịch sử
2. Click vào execution để xem chi tiết từng node
3. Kiểm tra logs nếu có lỗi

---

## 🔒 Bảo mật (Production)

1. **Enable Basic Auth** cho n8n:
   ```bash
   export N8N_BASIC_AUTH_ACTIVE=true
   export N8N_BASIC_AUTH_USER=admin
   export N8N_BASIC_AUTH_PASSWORD=strong-password
   ```

2. **Sử dụng HTTPS** cho webhook URL

3. **Rate limiting** trong ASP.NET:
   ```csharp
   // Thêm vào Program.cs
   builder.Services.AddRateLimiter(options => {
       options.AddFixedWindowLimiter("virtual-tryon", opt => {
           opt.Window = TimeSpan.FromMinutes(1);
           opt.PermitLimit = 10;
       });
   });
   ```

---

## 📈 Nâng cao

### Lưu ảnh vào Database

Thay vì trả base64, bạn có thể:
1. Lưu ảnh vào Azure Blob Storage / AWS S3
2. Trả về URL của ảnh
3. Lưu metadata vào SQL Server

### Thêm History cho User

1. Tạo bảng `VirtualTryOnHistory`
2. Lưu: UserId, ProductId, ResultImageUrl, CreatedDate
3. Hiển thị lịch sử trong trang Profile

---

## 🎯 Kết luận

Sau khi hoàn thành các bước trên:
- ✅ n8n workflow đã sẵn sàng nhận request
- ✅ Gemini AI tích hợp để generate ảnh
- ✅ Frontend có thể gọi API và hiển thị kết quả

**Next Steps:**
1. Test workflow với ảnh thật
2. Điều chỉnh prompt để cải thiện chất lượng
3. Deploy n8n lên production
4. Thêm caching và optimization

---

## 📞 Support

Nếu gặp vấn đề:
- n8n Docs: https://docs.n8n.io
- Gemini API Docs: https://ai.google.dev/docs
- Community: https://community.n8n.io
