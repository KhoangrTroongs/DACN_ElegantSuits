-- Check the complete structure of the Users table
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' 
ORDER BY ORDINAL_POSITION;

-- Check if external authentication fields exist
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'GoogleId') 
        THEN 'GoogleId column exists' 
        ELSE 'GoogleId column missing' 
    END AS GoogleId_Status,
    CASE 
        WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'FacebookId') 
        THEN 'FacebookId column exists' 
        ELSE 'FacebookId column missing' 
    END AS FacebookId_Status;

-- Check migration history
SELECT MigrationId, ProductVersion 
FROM [__EFMigrationsHistory] 
WHERE MigrationId LIKE '%External%' OR MigrationId LIKE '%Auth%'
ORDER BY MigrationId;
