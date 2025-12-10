-- Script cập nhật mã Linear cho sản phẩm
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio

PRINT 'Bắt đầu cập nhật mã Linear...';
PRINT '================================================';

-- Cập nhật từng sản phẩm
UPDATE Products SET LinearCode = '200000000001' WHERE Id = 1;
UPDATE Products SET LinearCode = '200000000002' WHERE Id = 2;
UPDATE Products SET LinearCode = '200000000003' WHERE Id = 3;
UPDATE Products SET LinearCode = '200000000004' WHERE Id = 4;
UPDATE Products SET LinearCode = '200000000005' WHERE Id = 5;
UPDATE Products SET LinearCode = '200000000006' WHERE Id = 6;
UPDATE Products SET LinearCode = '200000000007' WHERE Id = 7;
UPDATE Products SET LinearCode = '200000000008' WHERE Id = 8;
UPDATE Products SET LinearCode = '200000000009' WHERE Id = 9;
UPDATE Products SET LinearCode = '200000000010' WHERE Id = 10;
UPDATE Products SET LinearCode = '200000000011' WHERE Id = 11;
UPDATE Products SET LinearCode = '200000000012' WHERE Id = 12;
UPDATE Products SET LinearCode = '200000000013' WHERE Id = 13;
UPDATE Products SET LinearCode = '200000000014' WHERE Id = 14;
UPDATE Products SET LinearCode = '200000000015' WHERE Id = 15;
UPDATE Products SET LinearCode = '200000000016' WHERE Id = 16;
UPDATE Products SET LinearCode = '200000000017' WHERE Id = 17;
UPDATE Products SET LinearCode = '200000000018' WHERE Id = 18;
UPDATE Products SET LinearCode = '200000000019' WHERE Id = 19;
UPDATE Products SET LinearCode = '200000000020' WHERE Id = 20;
UPDATE Products SET LinearCode = '200000000021' WHERE Id = 21;
UPDATE Products SET LinearCode = '200000000022' WHERE Id = 22;
UPDATE Products SET LinearCode = '200000000023' WHERE Id = 23;
UPDATE Products SET LinearCode = '200000000024' WHERE Id = 24;
UPDATE Products SET LinearCode = '200000000025' WHERE Id = 25;
UPDATE Products SET LinearCode = '200000000026' WHERE Id = 26;
UPDATE Products SET LinearCode = '200000000027' WHERE Id = 27;
UPDATE Products SET LinearCode = '200000000028' WHERE Id = 28;
UPDATE Products SET LinearCode = '200000000029' WHERE Id = 29;
UPDATE Products SET LinearCode = '200000000030' WHERE Id = 30;

PRINT '================================================';
PRINT 'Hoàn thành! Đã cập nhật mã Linear cho 30 sản phẩm';

-- Kiểm tra kết quả
SELECT Id, Name, LinearCode 
FROM Products 
WHERE LinearCode IS NOT NULL
ORDER BY Id;
