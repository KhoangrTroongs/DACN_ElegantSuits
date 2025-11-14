-- Add Description column to Coupons table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Coupons]') AND name = 'Description')
BEGIN
    ALTER TABLE [dbo].[Coupons]
    ADD [Description] nvarchar(500) NULL;
    PRINT 'Added Description column to Coupons table';
END
ELSE
BEGIN
    PRINT 'Description column already exists in Coupons table';
END
GO

-- Change DiscountPercentage from decimal to int if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Coupons]') AND name = 'DiscountPercentage' AND system_type_id = 106) -- 106 is decimal
BEGIN
    -- First, update any decimal values to integers (round them)
    UPDATE [dbo].[Coupons]
    SET [DiscountPercentage] = ROUND([DiscountPercentage], 0);
    
    -- Then alter the column type
    ALTER TABLE [dbo].[Coupons]
    ALTER COLUMN [DiscountPercentage] int NOT NULL;
    
    PRINT 'Changed DiscountPercentage from decimal to int';
END
ELSE
BEGIN
    PRINT 'DiscountPercentage is already int or does not exist';
END
GO

