-- =============================================
-- SCRIPT SỬA LỖI DATABASE (V2 - ĐÃ SỬA TÊN BẢNG)
-- =============================================

USE WEBQLSP;
GO

-- Thêm cột LoginProvider
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'LoginProvider')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [LoginProvider] nvarchar(max) NULL;
END

-- Thêm cột ProviderKey
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ProviderKey')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [ProviderKey] nvarchar(max) NULL;
END

-- Thêm cột IsOAuthUser
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'IsOAuthUser')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [IsOAuthUser] bit NOT NULL DEFAULT 0;
END

GO

PRINT '✓✓✓ HOÀN TẤT! Database đã được cập nhật. Bạn có thể chạy lại ứng dụng.';

