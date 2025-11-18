-- =============================================
-- GOOGLE OAUTH - ADD REQUIRED COLUMNS
-- Database: WEBQLSP
-- Table: AspNetUsers
-- =============================================

USE WEBQLSP;
GO

PRINT '========================================';
PRINT 'Adding Google OAuth columns...';
PRINT '========================================';
PRINT '';

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
PRINT '';
GO

-- Verify the columns were added
PRINT 'Verifying columns...';
PRINT '';
SELECT 
    COLUMN_NAME as [Column Name],
    DATA_TYPE as [Data Type],
    IS_NULLABLE as [Nullable],
    COLUMN_DEFAULT as [Default Value]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
AND COLUMN_NAME IN ('LoginProvider', 'ProviderKey', 'IsOAuthUser')
ORDER BY COLUMN_NAME;
GO

PRINT '';
PRINT '========================================';
PRINT '✓ All done! You can now run the app.';
PRINT '========================================';

