<?php include 'app/views/shares/header.php'; ?>

<!-- Breadcrumb -->
<nav aria-label="breadcrumb" class="mb-4">
    <ol class="breadcrumb">
        <li class="breadcrumb-item"><a href="/webbanhang/Product/">Trang chủ</a></li>
        <li class="breadcrumb-item"><a href="/webbanhang/Product/cart">Giỏ hàng</a></li>
        <li class="breadcrumb-item"><a href="/webbanhang/Product/checkout">Thanh toán</a></li>
        <li class="breadcrumb-item active" aria-current="page">Đặt hàng thành công</li>
    </ol>
</nav>



<!-- Success Alert -->
<div class="alert alert-success text-center mb-4">
    <div class="mb-3">
        <i class="fas fa-check-circle fa-4x text-success"></i>
    </div>
    <h4 class="alert-heading">Đặt hàng thành công!</h4>
    <p class="mb-0">Cảm ơn bạn đã đặt hàng. Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất để xác nhận đơn hàng.</p>
</div>

<div class="row">
    <div class="col-lg-8">
        <!-- Thông tin đơn hàng -->
        <div class="card mb-4">
            <div class="card-header">
                <h5 class="mb-0"><i class="fas fa-file-invoice me-2"></i>Thông tin đơn hàng</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6">
                        <table class="table table-borderless">
                            <tr>
                                <td class="fw-bold">Mã đơn hàng:</td>
                                <td class="text-primary">#<?php echo str_pad($order->id, 6, '0', STR_PAD_LEFT); ?></td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Ngày đặt:</td>
                                <td><?php echo date('d/m/Y H:i', strtotime($order->created_at)); ?></td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Trạng thái:</td>
                                <td>
                                    <span class="badge bg-warning">
                                        <?php
                                        switch($order->status) {
                                            case 'pending': echo 'Chờ xử lý'; break;
                                            case 'confirmed': echo 'Đã xác nhận'; break;
                                            case 'shipping': echo 'Đang giao hàng'; break;
                                            case 'delivered': echo 'Đã giao hàng'; break;
                                            case 'cancelled': echo 'Đã hủy'; break;
                                            default: echo 'Chờ xử lý';
                                        }
                                        ?>
                                    </span>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="col-md-6">
                        <table class="table table-borderless">
                            <tr>
                                <td class="fw-bold">Tên khách hàng:</td>
                                <td><?php echo htmlspecialchars($order->customer_name, ENT_QUOTES, 'UTF-8'); ?></td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Số điện thoại:</td>
                                <td><?php echo htmlspecialchars($order->customer_phone, ENT_QUOTES, 'UTF-8'); ?></td>
                            </tr>
                            <tr>
                                <td class="fw-bold">Địa chỉ giao hàng:</td>
                                <td><?php echo htmlspecialchars($order->customer_address, ENT_QUOTES, 'UTF-8'); ?></td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <!-- Chi tiết sản phẩm -->
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0"><i class="fas fa-list me-2"></i>Chi tiết sản phẩm đã đặt</h5>
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <table class="table table-hover">
                        <thead class="table-light">
                            <tr>
                                <th>Sản phẩm</th>
                                <th>Giá</th>
                                <th>Số lượng</th>
                                <th>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php foreach ($order->details as $detail): ?>
                                <tr>
                                    <td>
                                        <h6 class="mb-0"><?php echo htmlspecialchars($detail->product_name, ENT_QUOTES, 'UTF-8'); ?></h6>
                                    </td>
                                    <td>
                                        <span class="text-primary">
                                            <?php echo number_format($detail->unit_price, 0, ',', '.'); ?> VND
                                        </span>
                                    </td>
                                    <td>
                                        <span class="badge bg-light text-dark"><?php echo $detail->quantity; ?></span>
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
                                <th class="text-primary">
                                    <?php echo number_format($order->total_amount, 0, ',', '.'); ?> VND
                                </th>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <div class="col-lg-4">
        <!-- Thông tin liên hệ -->
        <div class="card mb-4">
            <div class="card-header">
                <h5 class="mb-0"><i class="fas fa-headset me-2"></i>Hỗ trợ khách hàng</h5>
            </div>
            <div class="card-body">
                <p class="mb-3">Nếu bạn có bất kỳ câu hỏi nào về đơn hàng, vui lòng liên hệ với chúng tôi:</p>
                <div class="mb-2">
                    <i class="fas fa-phone text-primary me-2"></i>
                    <strong>Hotline:</strong> 1900-1234
                </div>
                <div class="mb-2">
                    <i class="fas fa-envelope text-primary me-2"></i>
                    <strong>Email:</strong> support@techshop.com
                </div>
                <div class="mb-2">
                    <i class="fas fa-clock text-primary me-2"></i>
                    <strong>Giờ làm việc:</strong> 8:00 - 22:00 (T2-CN)
                </div>
            </div>
        </div>

        <!-- Thông tin giao hàng -->
        <div class="card mb-4">
            <div class="card-header">
                <h5 class="mb-0"><i class="fas fa-truck me-2"></i>Thông tin giao hàng</h5>
            </div>
            <div class="card-body">
                <div class="timeline">
                    <div class="timeline-item active">
                        <div class="timeline-marker bg-success"></div>
                        <div class="timeline-content">
                            <h6 class="mb-1">Đơn hàng đã được đặt</h6>
                            <small class="text-muted"><?php echo date('d/m/Y H:i', strtotime($order->created_at)); ?></small>
                        </div>
                    </div>
                    <div class="timeline-item">
                        <div class="timeline-marker bg-secondary"></div>
                        <div class="timeline-content">
                            <h6 class="mb-1">Xác nhận đơn hàng</h6>
                            <small class="text-muted">Trong vòng 2-4 giờ</small>
                        </div>
                    </div>
                    <div class="timeline-item">
                        <div class="timeline-marker bg-secondary"></div>
                        <div class="timeline-content">
                            <h6 class="mb-1">Đóng gói và giao hàng</h6>
                            <small class="text-muted">1-3 ngày làm việc</small>
                        </div>
                    </div>
                    <div class="timeline-item">
                        <div class="timeline-marker bg-secondary"></div>
                        <div class="timeline-content">
                            <h6 class="mb-1">Giao hàng thành công</h6>
                            <small class="text-muted">Theo địa chỉ đã cung cấp</small>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Hành động -->
        <div class="d-grid gap-2">
            <a href="/webbanhang/Product/list" class="btn btn-primary">
                <i class="fas fa-shopping-bag me-2"></i>Tiếp tục mua sắm
            </a>
            <a href="/webbanhang/Product/" class="btn btn-outline-secondary">
                <i class="fas fa-home me-2"></i>Về trang chủ
            </a>
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
</style>

<?php include 'app/views/shares/footer.php'; ?>
