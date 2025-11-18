-- =============================================
-- CHẠY SCRIPT NÀY ĐỂ SỬA LỖI GOOGLE LOGIN
-- =============================================

USE WEBQLSP;
GO

-- Thêm cột LoginProvider
ALTER TABLE [dbo].[AspNetUsers] 
ADD [LoginProvider] nvarchar(max) NULL;

-- Thêm cột ProviderKey
ALTER TABLE [dbo].[AspNetUsers] 
ADD [ProviderKey] nvarchar(max) NULL;

-- Thêm cột IsOAuthUser
ALTER TABLE [dbo].[AspNetUsers] 
ADD [IsOAuthUser] bit NOT NULL DEFAULT 0;

GO

PRINT '✓✓✓ HOÀN TẤT! Bạn có thể chạy lại ứng dụng.';

