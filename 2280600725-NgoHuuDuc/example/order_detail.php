<?php include 'app/views/shares/header.php'; ?>

<!-- Breadcrumb -->
<nav aria-label="breadcrumb" class="mb-4 screen-only">
    <ol class="breadcrumb">
        <li class="breadcrumb-item"><a href="/webbanhang/Product/">Trang chủ</a></li>
        <li class="breadcrumb-item"><a href="/webbanhang/Product/manage">Quản lý</a></li>
        <li class="breadcrumb-item"><a href="/webbanhang/Product/manageOrders">Quản lý đơn hàng</a></li>
        <li class="breadcrumb-item active" aria-current="page">Chi tiết đơn hàng #<?php echo str_pad($order->id, 6, '0', STR_PAD_LEFT); ?></li>
    </ol>
</nav>

<!-- Page Header (Hidden when printing) -->
<div class="d-flex justify-content-between align-items-center mb-4 screen-only">
    <h2><i class="fas fa-file-invoice me-2"></i>Chi tiết đơn hàng #<?php echo str_pad($order->id, 6, '0', STR_PAD_LEFT); ?></h2>
    <a href="/webbanhang/Product/manageOrders" class="btn btn-outline-secondary">
        <i class="fas fa-arrow-left me-2"></i>Quay lại danh sách
    </a>
</div>

<!-- Professional Invoice Layout (Only visible when printing) -->
<div class="invoice-container print-only">
    <!-- Invoice Header -->
    <div class="invoice-header">
        <div class="company-info">
            <div class="company-name">NHD TECHSHOP</div>
            <div class="company-details">
                Địa chỉ: 123 Đường Công Nghệ, Quận 1, TP.HCM<br>
                Điện thoại: (028) 1234-5678 | Email: info@ndhtechshop.vn<br>
                Website: www.ndhtechshop.vn | MST: 0123456789
            </div>
        </div>
        <h3 style="margin: 15px 0 5px 0; font-size: 20px;">HÓA ĐƠN BÁN HÀNG</h3>
        <div style="font-size: 12px;">Invoice No: #<?php echo str_pad($order->id, 6, '0', STR_PAD_LEFT); ?></div>
    </div>

    <!-- Invoice Information -->
    <div class="invoice-info">
        <div class="invoice-details">
            <h6>THÔNG TIN HÓA ĐƠN</h6>
            <div><strong>Số hóa đơn:</strong> #<?php echo str_pad($order->id, 6, '0', STR_PAD_LEFT); ?></div>
            <div><strong>Ngày xuất:</strong> <?php echo date('d/m/Y H:i:s'); ?></div>
            <div><strong>Ngày đặt hàng:</strong> <?php echo date('d/m/Y H:i:s', strtotime($order->created_at)); ?></div>
            <div><strong>Phương thức thanh toán:</strong> Thanh toán khi nhận hàng (COD)</div>
        </div>
        <div class="customer-details">
            <h6>THÔNG TIN KHÁCH HÀNG</h6>
            <div><strong>Họ và tên:</strong> <?php echo htmlspecialchars($order->customer_name, ENT_QUOTES, 'UTF-8'); ?></div>
            <div><strong>Số điện thoại:</strong> <?php echo htmlspecialchars($order->customer_phone, ENT_QUOTES, 'UTF-8'); ?></div>
            <?php if (!empty($order->email)): ?>
            <div><strong>Email:</strong> <?php echo htmlspecialchars($order->email, ENT_QUOTES, 'UTF-8'); ?></div>
            <?php endif; ?>
            <div><strong>Địa chỉ giao hàng:</strong><br><?php echo htmlspecialchars($order->customer_address, ENT_QUOTES, 'UTF-8'); ?></div>
        </div>
    </div>

    <!-- Product Details Table -->
    <table class="invoice-table">
        <thead>
            <tr>
                <th style="width: 5%;">STT</th>
                <th style="width: 45%;">Tên sản phẩm</th>
                <th style="width: 10%;">Số lượng</th>
                <th style="width: 20%;">Đơn giá</th>
                <th style="width: 20%;">Thành tiền</th>
            </tr>
        </thead>
        <tbody>
            <?php $stt = 1; foreach ($order->details as $detail): ?>
                <tr>
                    <td class="text-center"><?php echo $stt++; ?></td>
                    <td><?php echo htmlspecialchars($detail->product_name ?? 'Sản phẩm #' . $detail->product_id, ENT_QUOTES, 'UTF-8'); ?></td>
                    <td class="text-center"><?php echo $detail->quantity; ?></td>
                    <td class="text-right"><?php echo number_format($detail->unit_price, 0, ',', '.'); ?> VND</td>
                    <td class="text-right"><?php echo number_format($detail->subtotal, 0, ',', '.'); ?> VND</td>
                </tr>
            <?php endforeach; ?>
        </tbody>
    </table>

    <!-- Order Notes Section -->
    <?php if (!empty($order->notes)): ?>
    <div class="order-notes-section">
        <h6 style="font-size: 14px; font-weight: bold; margin-bottom: 8px; border-bottom: 1px solid #ddd; padding-bottom: 3px;">
            GHI CHÚ ĐƠN HÀNG
        </h6>
        <div style="padding: 10px; border: 1px solid #ccc; background-color: #f9f9f9; margin-bottom: 15px; font-size: 11px; line-height: 1.4;">
            <?php echo nl2br(htmlspecialchars($order->notes, ENT_QUOTES, 'UTF-8')); ?>
        </div>
    </div>
    <?php endif; ?>

    <!-- Invoice Summary -->
    <div class="invoice-summary">
        <table class="summary-table">
            <tr>
                <td>Tạm tính:</td>
                <td class="text-right"><?php echo number_format($order->total_amount, 0, ',', '.'); ?> VND</td>
            </tr>
            <tr>
                <td>Phí vận chuyển:</td>
                <td class="text-right">0 VND</td>
            </tr>
            <tr>
                <td>Thuế VAT (0%):</td>
                <td class="text-right">0 VND</td>
            </tr>
            <tr class="total-row">
                <td><strong>TỔNG CỘNG:</strong></td>
                <td class="text-right"><strong><?php echo number_format($order->total_amount, 0, ',', '.'); ?> VND</strong></td>
            </tr>
        </table>
    </div>

    <!-- Invoice Footer -->
    <div class="invoice-footer">
        <div style="margin-bottom: 10px;">
            <strong>Cảm ơn quý khách đã mua hàng tại NHD TechShop!</strong>
        </div>
        <div>
            Hotline hỗ trợ: 1900-1234 | Email: support@ndhtechshop.vn<br>
            Chính sách đổi trả trong vòng 7 ngày | Bảo hành chính hãng
        </div>
        <div style="margin-top: 15px; font-style: italic;">
            Hóa đơn được in tự động từ hệ thống - Ngày in: <?php echo date('d/m/Y H:i:s'); ?>
        </div>
    </div>
</div>

<div class="row screen-only" style="display:flex;">
    <!-- Thông tin đơn hàng -->
    <div class="col-lg-8">
        <div class="card shadow-sm mb-4">
            <div class="card-header bg-light">
                <h5 class="mb-0"><i class="fas fa-info-circle me-2 text-primary"></i>Thông tin đơn hàng</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6">
                        <table class="table table-borderless">
                            <tr>
                                <td class="fw-bold" style="width: 40%;">Mã đơn hàng:</td>
                                <td class="text-primary fw-bold">#<?php echo str_pad($order->id, 6, '0', STR_PAD_LEFT); ?></td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Ngày đặt hàng:</td>
                                <td><?php echo date('d/m/Y H:i:s', strtotime($order->created_at)); ?></td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Trạng thái:</td>
                                <td>
                                    <?php
                                    $orderModel = new OrderModel($db ?? null);
                                    $currentStatus = $order->status ?? 'pending';
                                    $statusInfo = $orderModel->getStatusInfo($currentStatus);
                                    ?>
                                    <?php if ($statusInfo): ?>
                                        <span class="badge bg-<?php echo $statusInfo['class']; ?>">
                                            <i class="<?php echo $statusInfo['icon']; ?> me-1"></i>
                                            <?php echo $statusInfo['name']; ?>
                                        </span>
                                    <?php else: ?>
                                        <span class="badge bg-secondary">Không xác định</span>
                                    <?php endif; ?>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="col-md-6">
                        <table class="table table-borderless">
                            <tr>
                                <td class="fw-bold" style="width: 40%;">Tổng tiền:</td>
                                <td class="text-success fw-bold fs-5">
                                    <?php echo number_format($order->total_amount, 0, ',', '.'); ?> VND
                                </td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Số sản phẩm:</td>
                                <td>
                                    <?php
                                    $totalItems = 0;
                                    foreach ($order->details as $detail) {
                                        $totalItems += $detail->quantity;
                                    }
                                    echo $totalItems;
                                    ?> sản phẩm
                                </td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Phương thức thanh toán:</td>
                                <td>Thanh toán khi nhận hàng (COD)</td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <!-- Chi tiết sản phẩm -->
        <div class="card shadow-sm mb-4">
            <div class="card-header bg-light">
                <h5 class="mb-0"><i class="fas fa-list me-2 text-primary"></i>Chi tiết sản phẩm</h5>
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <table class="table table-hover">
                        <thead class="table-light">
                            <tr>
                                <th>Tên sản phẩm</th>
                                <th>Giá</th>
                                <th>Số lượng</th>
                                <th>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php foreach ($order->details as $detail): ?>
                                <tr>
                                    <td>
                                        <div class="d-flex align-items-center">
                                            <div>
                                                <h6 class="mb-0">
                                                    <?php echo htmlspecialchars($detail->product_name ?? 'Sản phẩm #' . $detail->product_id, ENT_QUOTES, 'UTF-8'); ?>
                                                </h6>
                                                <small class="text-muted">ID: <?php echo $detail->product_id; ?></small>
                                            </div>
                                        </div>
                                    </td>
                                    <td>
                                        <span class="text-primary">
                                            <?php echo number_format($detail->unit_price, 0, ',', '.'); ?> VND
                                        </span>
                                    </td>
                                    <td>
                                        <span class="badge bg-light text-dark fs-6"><?php echo $detail->quantity; ?></span>
                                    </td>
                                    <td>
                                        <span class="fw-bold">
                                            <?php echo number_format($detail->subtotal, 0, ',', '.'); ?> VND
                                        </span>
                                    </td>
                                </tr>
                            <?php endforeach; ?>
                        </tbody>
                        <tfoot class="table-light">
                            <tr>
                                <th colspan="3" class="text-end">Tổng cộng:</th>
                                <th class="text-success fs-5">
                                    <?php echo number_format($order->total_amount, 0, ',', '.'); ?> VND
                                </th>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <!-- Thông tin khách hàng -->
    <div class="col-lg-4">
        <div class="card shadow-sm mb-4">
            <div class="card-header bg-light">
                <h5 class="mb-0"><i class="fas fa-user me-2 text-primary"></i>Thông tin khách hàng</h5>
            </div>
            <div class="card-body p-3">
                <div class="mb-3">
                    <label class="form-label fw-bold text-muted small">
                        <i class="fas fa-user me-1"></i>Họ và tên:
                    </label>
                    <p class="mb-0 fs-6"><?php echo htmlspecialchars($order->customer_name, ENT_QUOTES, 'UTF-8'); ?></p>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold text-muted small">
                        <i class="fas fa-phone me-1"></i>Số điện thoại:
                    </label>
                    <p class="mb-0 fs-6">
                        <a href="tel:<?php echo $order->customer_phone; ?>" class="text-decoration-none text-primary">
                            <?php echo htmlspecialchars($order->customer_phone, ENT_QUOTES, 'UTF-8'); ?>
                        </a>
                    </p>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold text-muted small">
                        <i class="fas fa-envelope me-1"></i>Email:
                    </label>
                    <p class="mb-0 fs-6">
                        <?php if (!empty($order->email)): ?>
                            <a href="mailto:<?php echo $order->email; ?>" class="text-decoration-none text-primary">
                                <?php echo htmlspecialchars($order->email, ENT_QUOTES, 'UTF-8'); ?>
                            </a>
                        <?php else: ?>
                            <span class="text-muted">Không có thông tin</span>
                        <?php endif; ?>
                    </p>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold text-muted small">
                        <i class="fas fa-map-marker-alt me-1"></i>Địa chỉ giao hàng:
                    </label>
                    <p class="mb-0 fs-6"><?php echo htmlspecialchars($order->customer_address, ENT_QUOTES, 'UTF-8'); ?></p>
                </div>
            </div>
        </div>

        <!-- Ghi chú đơn hàng -->
        <?php if (!empty($order->notes)): ?>
        <div class="card shadow-sm mb-4">
            <div class="card-header bg-light">
                <h5 class="mb-0"><i class="fas fa-sticky-note me-2 text-primary"></i>Ghi chú đơn hàng</h5>
            </div>
            <div class="card-body p-3">
                <p class="mb-0 fs-6"><?php echo nl2br(htmlspecialchars($order->notes, ENT_QUOTES, 'UTF-8')); ?></p>
            </div>
        </div>
        <?php endif; ?>

        <!-- Timeline trạng thái đơn hàng -->
        <div class="card shadow-sm mb-4">
            <div class="card-header bg-light">
                <h5 class="mb-0"><i class="fas fa-clock me-2 text-primary"></i>Trạng thái đơn hàng</h5>
            </div>
            <div class="card-body">
                <div class="timeline">
                    <?php
                    $timelineSteps = [
                        'pending' => ['title' => 'Đơn hàng đã được đặt', 'time' => date('d/m/Y H:i', strtotime($order->created_at))],
                        'confirmed' => ['title' => 'Xác nhận đơn hàng', 'time' => 'Chờ xử lý'],
                        'shipping' => ['title' => 'Đóng gói và giao hàng', 'time' => 'Chờ xử lý'],
                        'delivered' => ['title' => 'Giao hàng thành công', 'time' => 'Chờ xử lý']
                    ];

                    $statusOrder = ['pending', 'confirmed', 'shipping', 'delivered'];
                    $currentIndex = array_search($currentStatus, $statusOrder);
                    if ($currentStatus === 'cancelled') {
                        $currentIndex = 0; // Chỉ hiển thị bước đầu tiên nếu đã hủy
                    }

                    foreach ($statusOrder as $index => $step):
                        $isActive = ($index <= $currentIndex && $currentStatus !== 'cancelled');
                        $isCancelled = ($currentStatus === 'cancelled' && $index > 0);
                    ?>
                        <div class="timeline-item <?php echo $isActive ? 'active' : ''; ?>">
                            <div class="timeline-marker <?php echo $isActive ? 'bg-success' : ($isCancelled ? 'bg-danger' : 'bg-secondary'); ?>"></div>
                            <div class="timeline-content">
                                <h6 class="mb-1"><?php echo $timelineSteps[$step]['title']; ?></h6>
                                <small class="text-muted">
                                    <?php
                                    if ($isCancelled) {
                                        echo 'Đã hủy';
                                    } else {
                                        echo $timelineSteps[$step]['time'];
                                    }
                                    ?>
                                </small>
                            </div>
                        </div>
                    <?php endforeach; ?>

                    <?php if ($currentStatus === 'cancelled'): ?>
                        <div class="timeline-item active">
                            <div class="timeline-marker bg-danger"></div>
                            <div class="timeline-content">
                                <h6 class="mb-1">Đơn hàng đã bị hủy</h6>
                                <small class="text-muted">Đơn hàng không thể tiếp tục xử lý</small>
                            </div>
                        </div>
                    <?php endif; ?>
                </div>
            </div>
        </div>

        <!-- Cập nhật trạng thái -->
        <div class="card shadow-sm mb-4">
            <div class="card-header bg-light">
                <h5 class="mb-0"><i class="fas fa-edit me-2 text-primary"></i>Cập nhật trạng thái</h5>
            </div>
            <div class="card-body">
                <?php
                $validStatuses = $orderModel->getValidStatuses();
                $hasAvailableStatus = false;
                foreach ($validStatuses as $statusKey => $statusData) {
                    if ($statusKey !== $currentStatus && $orderModel->canChangeStatus($currentStatus, $statusKey)) {
                        $hasAvailableStatus = true;
                        break;
                    }
                }
                ?>

                <?php if ($hasAvailableStatus): ?>
                    <form id="statusUpdateForm" action="/webbanhang/Product/updateOrderStatus" method="POST">
                        <input type="hidden" name="order_id" value="<?php echo $order->id; ?>">
                        <input type="hidden" name="redirect_to" value="viewOrder">

                        <div class="mb-3">
                            <label for="status" class="form-label">Chọn trạng thái mới:</label>
                            <select class="form-select" id="status" name="status" required>
                                <option value="">-- Chọn trạng thái --</option>
                                <?php foreach ($validStatuses as $statusKey => $statusData): ?>
                                    <?php if ($statusKey !== $currentStatus && $orderModel->canChangeStatus($currentStatus, $statusKey)): ?>
                                        <option value="<?php echo $statusKey; ?>">
                                            <?php echo $statusData['name']; ?>
                                        </option>
                                    <?php endif; ?>
                                <?php endforeach; ?>
                            </select>
                        </div>

                        <button type="submit" class="btn btn-success w-100">
                            <i class="fas fa-save me-2"></i>Cập nhật trạng thái
                        </button>
                    </form>
                <?php else: ?>
                    <div class="alert alert-info mb-0">
                        <i class="fas fa-info-circle me-2"></i>
                        Không thể cập nhật trạng thái từ "<?php echo $statusInfo['name']; ?>" sang trạng thái khác.
                    </div>
                <?php endif; ?>
            </div>
        </div>

        <!-- Thao tác -->
        <div class="card shadow-sm">
            <div class="card-header bg-light">
                <h5 class="mb-0"><i class="fas fa-cogs me-2 text-primary"></i>Thao tác</h5>
            </div>
            <div class="card-body">
                <div class="d-grid gap-2">
                    <button type="button" class="btn btn-primary" onclick="printInvoice()">
                        <i class="fas fa-print me-2"></i>In hóa đơn
                    </button>
                    <a href="/webbanhang/Product/manageOrders" class="btn btn-outline-secondary">
                        <i class="fas fa-arrow-left me-2"></i>Quay lại danh sách
                    </a>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Custom CSS for timeline -->
<style>
.timeline {
    position: relative;
    padding-left: 30px;
}

.timeline::before {
    content: '';
    position: absolute;
    left: 15px;
    top: 0;
    bottom: 0;
    width: 2px;
    background: #dee2e6;
}

.timeline-item {
    position: relative;
    margin-bottom: 20px;
}

.timeline-marker {
    position: absolute;
    left: -23px;
    top: 5px;
    width: 16px;
    height: 16px;
    border-radius: 50%;
    border: 2px solid #fff;
}

.timeline-item.active .timeline-marker {
    background: #28a745 !important;
}

.timeline-content h6 {
    margin-bottom: 5px;
    font-size: 14px;
}

.timeline-content small {
    font-size: 12px;
}

/* Hide invoice container on screen, only show when printing */
.print-only {
    display: none !important;
}

/* Show screen content normally, hide when printing */
.screen-only {
    display: block;
}

/* Print Styles for Professional Invoice */
@media print {
    /* Show invoice container when printing */
    .print-only {
        display: block !important;
    }

    /* Hide screen-only content when printing */
    .screen-only {
        display: none !important;
    }

    /* Hide non-essential elements */
    .btn, .breadcrumb, .navbar, .card-header,
    .timeline, #statusUpdateForm, .alert,
    .dropdown, .badge, .text-muted small,
    nav, header, footer, .no-print {
        display: none !important;
    }

    /* Reset page margins and layout */
    @page {
        margin: 0.5in;
        size: A4;
    }

    body {
        font-family: 'Arial', sans-serif;
        font-size: 12px;
        line-height: 1.4;
        color: #000;
        background: white;
    }

    .container {
        max-width: 100%;
        margin: 0;
        padding: 0;
    }

    .card {
        border: none !important;
        box-shadow: none !important;
        margin-bottom: 0 !important;
        page-break-inside: avoid;
    }

    .card-body {
        padding: 0 !important;
    }

    /* Invoice Header */
    .invoice-header {
        text-align: center;
        border-bottom: 2px solid #000;
        padding-bottom: 15px;
        margin-bottom: 20px;
    }

    .company-info {
        margin-bottom: 10px;
    }

    .company-name {
        font-size: 24px;
        font-weight: bold;
        margin-bottom: 5px;
    }

    .company-details {
        font-size: 11px;
        line-height: 1.3;
    }

    /* Invoice Info */
    .invoice-info {
        display: flex;
        justify-content: space-between;
        margin-bottom: 20px;
        border-bottom: 1px solid #ccc;
        padding-bottom: 15px;
    }

    .invoice-details, .customer-details {
        width: 48%;
    }

    .invoice-details h6, .customer-details h6 {
        font-size: 14px;
        font-weight: bold;
        margin-bottom: 8px;
        border-bottom: 1px solid #ddd;
        padding-bottom: 3px;
    }

    /* Product Table */
    .invoice-table {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 20px;
    }

    .invoice-table th,
    .invoice-table td {
        border: 1px solid #000;
        padding: 8px;
        text-align: left;
        font-size: 11px;
    }

    .invoice-table th {
        background-color: #f0f0f0;
        font-weight: bold;
        text-align: center;
    }

    .invoice-table .text-center {
        text-align: center;
    }

    .invoice-table .text-right {
        text-align: right;
    }

    /* Order Notes */
    .order-notes-section {
        margin-bottom: 15px;
        page-break-inside: avoid;
    }

    .order-notes-section h6 {
        font-size: 14px;
        font-weight: bold;
        margin-bottom: 8px;
        border-bottom: 1px solid #ddd;
        padding-bottom: 3px;
    }

    /* Summary */
    .invoice-summary {
        float: right;
        width: 300px;
        margin-top: 10px;
    }

    .summary-table {
        width: 100%;
        border-collapse: collapse;
    }

    .summary-table td {
        padding: 5px 10px;
        border: 1px solid #000;
        font-size: 12px;
    }

    .summary-table .total-row {
        font-weight: bold;
        background-color: #f0f0f0;
    }

    /* Footer */
    .invoice-footer {
        clear: both;
        margin-top: 30px;
        padding-top: 15px;
        border-top: 1px solid #ccc;
        text-align: center;
        font-size: 10px;
    }

    /* Utilities */
    .text-bold {
        font-weight: bold;
    }

    .mb-print {
        margin-bottom: 15px;
    }

    /* Force page break */
    .page-break {
        page-break-before: always;
    }
}
</style>

<!-- JavaScript for status update confirmation and print functionality -->
<script>
document.addEventListener('DOMContentLoaded', function() {
    // Status update confirmation
    const statusForm = document.getElementById('statusUpdateForm');
    if (statusForm) {
        statusForm.addEventListener('submit', function(e) {
            const statusSelect = document.getElementById('status');
            const selectedOption = statusSelect.options[statusSelect.selectedIndex];
            const statusName = selectedOption.text;
            const orderId = '<?php echo str_pad($order->id, 6, '0', STR_PAD_LEFT); ?>';

            if (!confirm(`Bạn có chắc chắn muốn cập nhật trạng thái đơn hàng #${orderId} thành "${statusName}"?`)) {
                e.preventDefault();
            }
        });
    }

    // Enhanced print functionality
    window.printInvoice = function() {
        // Set document title for print
        const originalTitle = document.title;
        document.title = 'Hóa đơn #<?php echo str_pad($order->id, 6, '0', STR_PAD_LEFT); ?> - NHD TechShop';

        // Print the page
        window.print();

        // Restore original title
        document.title = originalTitle;
    };

    // Add keyboard shortcut for printing (Ctrl+P)
    document.addEventListener('keydown', function(e) {
        if (e.ctrlKey && e.key === 'p') {
            e.preventDefault();
            printInvoice();
        }
    });
});
</script>

<?php include 'app/views/shares/footer.php'; ?>
