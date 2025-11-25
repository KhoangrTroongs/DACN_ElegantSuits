-- Script to add MoMo payment fields to Orders table
USE WEBQLSP;
GO

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'PaymentMethod')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [PaymentMethod] NVARCHAR(50) NOT NULL DEFAULT 'COD';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'PaymentStatus')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [PaymentStatus] INT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'TransactionId')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [TransactionId] NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'TotalAmount')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [TotalAmount] DECIMAL(18, 2) NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'OrderStatus')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD [OrderStatus] INT NOT NULL DEFAULT 0;
END
GO

-- Update TotalAmount for existing orders
UPDATE [dbo].[Orders]
SET [TotalAmount] = [TotalPrice]
WHERE [TotalAmount] = 0;
GO

PRINT 'MoMo payment fields added successfully!';
GO

