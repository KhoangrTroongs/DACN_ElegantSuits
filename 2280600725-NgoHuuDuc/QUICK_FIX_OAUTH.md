# QUICK FIX - Google OAuth Database Error

## ❌ Lỗi hiện tại:
```
Invalid column name 'IsOAuthUser'.
Invalid column name 'LoginProvider'.
Invalid column name 'ProviderKey'.
```

## ✅ Giải pháp:

### Bước 1: Mở SQL Server Management Studio (SSMS)

1. Kết nối đến server: **VERON**
2. Chọn database: **WEBQLSP**

### Bước 2: Chạy script SQL sau

Mở New Query và paste đoạn code sau, sau đó nhấn **Execute** (F5):

```sql
-- Add OAuth columns to AspNetUsers table
USE WEBQLSP;
GO

-- Add LoginProvider column
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
               AND name = 'LoginProvider')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [LoginProvider] nvarchar(max) NULL;
    PRINT '✓ Column LoginProvider added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column LoginProvider already exists';
END

-- Add ProviderKey column
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
               AND name = 'ProviderKey')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [ProviderKey] nvarchar(max) NULL;
    PRINT '✓ Column ProviderKey added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column ProviderKey already exists';
END

-- Add IsOAuthUser column
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
               AND name = 'IsOAuthUser')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [IsOAuthUser] bit NOT NULL DEFAULT 0;
    PRINT '✓ Column IsOAuthUser added successfully';
END
ELSE
BEGIN
    PRINT '✓ Column IsOAuthUser already exists';
END

PRINT '';
PRINT '========================================';
PRINT '✓ OAuth columns setup completed!';
PRINT '========================================';
GO

-- Verify the columns were added
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
AND COLUMN_NAME IN ('LoginProvider', 'ProviderKey', 'IsOAuthUser')
ORDER BY COLUMN_NAME;
```

### Bước 3: Kiểm tra kết quả

Sau khi chạy script, bạn sẽ thấy thông báo:
```
✓ Column LoginProvider added successfully
✓ Column ProviderKey added successfully
✓ Column IsOAuthUser added successfully

========================================
✓ OAuth columns setup completed!
========================================
```

Và một bảng hiển thị 3 cột mới:
```
COLUMN_NAME      DATA_TYPE    IS_NULLABLE    COLUMN_DEFAULT
IsOAuthUser      bit          NO             ((0))
LoginProvider    nvarchar     YES            NULL
ProviderKey      nvarchar     YES            NULL
```

### Bước 4: Khởi động lại ứng dụng

Sau khi chạy script thành công, quay lại terminal và chạy lại ứng dụng:

```bash
dotnet watch run
```

## 🎉 Hoàn tất!

Bây giờ bạn có thể:
1. Truy cập trang đăng nhập: `https://localhost:5001/Account/Login`
2. Nhấn nút "Đăng nhập với Google"
3. Đăng nhập bằng tài khoản Google của bạn
4. Hệ thống sẽ tự động:
   - Tạo tài khoản mới (nếu chưa có)
   - Lưu thông tin vào database
   - Gửi email thông báo (nếu đã cấu hình SMTP)
   - Đăng nhập thành công

## 📝 Lưu ý:

- Script này an toàn và có kiểm tra trước khi thêm cột
- Nếu cột đã tồn tại, script sẽ không làm gì cả
- Không ảnh hưởng đến dữ liệu hiện có trong database

## 🔧 Nếu vẫn gặp lỗi:

1. Kiểm tra xem bạn đã kết nối đúng database chưa
2. Đảm bảo bạn có quyền ALTER TABLE
3. Refresh lại database trong SSMS (F5)
4. Kiểm tra lại tên bảng là `AspNetUsers` (không phải `Users`)

---

**Cần hỗ trợ thêm?** Hãy kiểm tra file `GOOGLE_OAUTH_IMPLEMENTATION.md` để biết thêm chi tiết!
