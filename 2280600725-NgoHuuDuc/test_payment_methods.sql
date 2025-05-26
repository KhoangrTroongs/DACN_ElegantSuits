-- Test script to verify payment methods functionality

-- Check if PaymentMethod column exists in Orders table
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'PaymentMethod';

-- Check current Orders table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Orders' 
ORDER BY ORDINAL_POSITION;

-- Check if there are any existing orders and their payment methods
SELECT 
    Id,
    OrderDate,
    TotalPrice,
    PaymentMethod,
    Status,
    ShippingAddress
FROM Orders
ORDER BY OrderDate DESC;

-- Check migration history for payment method migration
SELECT MigrationId, ProductVersion 
FROM [__EFMigrationsHistory] 
WHERE MigrationId LIKE '%Payment%'
ORDER BY MigrationId;
