-- Add OAuth columns to AspNetUsers table
-- Run this script to add Google OAuth support

-- Check if columns already exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'LoginProvider')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [LoginProvider] nvarchar(max) NULL;
    PRINT 'Column LoginProvider added successfully';
END
ELSE
BEGIN
    PRINT 'Column LoginProvider already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'ProviderKey')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [ProviderKey] nvarchar(max) NULL;
    PRINT 'Column ProviderKey added successfully';
END
ELSE
BEGIN
    PRINT 'Column ProviderKey already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'IsOAuthUser')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [IsOAuthUser] bit NOT NULL DEFAULT 0;
    PRINT 'Column IsOAuthUser added successfully';
END
ELSE
BEGIN
    PRINT 'Column IsOAuthUser already exists';
END

PRINT 'OAuth columns setup completed successfully!';

