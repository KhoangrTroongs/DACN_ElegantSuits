import openpyxl
import pyodbc

# Kết nối database
conn_str = (
    'DRIVER={ODBC Driver 18 for SQL Server};'
    'SERVER=localhost;'
    'DATABASE=WEBQLSP;'
    'UID=sa;'
    'PWD=reallyStrongPwd123;'
    'TrustServerCertificate=yes;'
)

try:
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    
    # Đọc file Excel
    wb = openpyxl.load_workbook('/Users/huuducngo/Documents/DACN/DACN_ElegantSuits/DanhSachMaLinear.xlsx')
    ws = wb.active
    
    updated_count = 0
    not_found_count = 0
    
    print("Bắt đầu cập nhật mã Linear...")
    print("-" * 50)
    
    # Bỏ qua header row
    for row in ws.iter_rows(min_row=2, values_only=True):
        product_id = row[0]
        product_name = row[1]
        linear_code = row[2]
        
        if product_id and linear_code:
            # Kiểm tra sản phẩm tồn tại
            cursor.execute("SELECT Id, Name FROM Products WHERE Id = ?", product_id)
            product = cursor.fetchone()
            
            if product:
                # Cập nhật LinearCode
                cursor.execute(
                    "UPDATE Products SET LinearCode = ? WHERE Id = ?",
                    str(linear_code), product_id
                )
                updated_count += 1
                print(f"✓ Updated: ID {product_id} - {product[1][:40]} -> {linear_code}")
            else:
                not_found_count += 1
                print(f"✗ Not found: ID {product_id} - {product_name}")
    
    # Commit changes
    conn.commit()
    
    print("-" * 50)
    print(f"Hoàn thành!")
    print(f"Đã cập nhật: {updated_count} sản phẩm")
    print(f"Không tìm thấy: {not_found_count} sản phẩm")
    
    cursor.close()
    conn.close()
    
except Exception as e:
    print(f"Lỗi: {e}")
    if 'conn' in locals():
        conn.rollback()
