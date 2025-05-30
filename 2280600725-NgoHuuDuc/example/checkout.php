<?php include 'app/views/shares/header.php'; ?>

<!-- Breadcrumb -->
<nav aria-label="breadcrumb" class="mb-4">
    <ol class="breadcrumb">
        <li class="breadcrumb-item"><a href="/webbanhang/Product/">Trang chủ</a></li>
        <li class="breadcrumb-item"><a href="/webbanhang/Product/cart">Giỏ hàng</a></li>
        <li class="breadcrumb-item active" aria-current="page">Thanh toán</li>
    </ol>
</nav>



<div class="row">
    <div class="col-lg-8">
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0"><i class="fas fa-user me-2"></i>Thông tin khách hàng</h5>
            </div>
            <div class="card-body">
                <form action="/webbanhang/Product/processCheckout" method="POST" id="checkoutForm">
                    <div class="row">
                        <div class="col-md-12 mb-3">
                            <label for="customer_name" class="form-label">Họ và tên <span class="text-danger">*</span></label>
                            <input type="text"
                                   class="form-control"
                                   id="customer_name"
                                   name="customer_name"
                                   required
                                   minlength="2"
                                   placeholder="Nhập họ và tên của bạn"
                                   value="<?php echo isset($_POST['customer_name']) ? htmlspecialchars($_POST['customer_name'], ENT_QUOTES, 'UTF-8') : ''; ?>">
                            <div class="invalid-feedback">
                                Vui lòng nhập họ và tên (ít nhất 2 ký tự).
                            </div>
                        </div>

                        <div class="col-md-6 mb-3">
                            <label for="customer_phone" class="form-label">Số điện thoại <span class="text-danger">*</span></label>
                            <input type="tel"
                                   class="form-control"
                                   id="customer_phone"
                                   name="customer_phone"
                                   required
                                   pattern="[0-9]{10,11}"
                                   placeholder="Nhập số điện thoại"
                                   value="<?php echo isset($_POST['customer_phone']) ? htmlspecialchars($_POST['customer_phone'], ENT_QUOTES, 'UTF-8') : ''; ?>">
                            <div class="invalid-feedback">
                                Vui lòng nhập số điện thoại hợp lệ (10-11 chữ số).
                            </div>
                        </div>

                        <div class="col-md-6 mb-3">
                            <label for="customer_email" class="form-label">Email (tùy chọn)</label>
                            <input type="email"
                                   class="form-control"
                                   id="customer_email"
                                   name="customer_email"
                                   placeholder="Nhập email của bạn"
                                   value="<?php echo isset($_POST['customer_email']) ? htmlspecialchars($_POST['customer_email'], ENT_QUOTES, 'UTF-8') : ''; ?>">
                        </div>

                        <div class="col-md-12 mb-3">
                            <label for="customer_address" class="form-label">Địa chỉ giao hàng <span class="text-danger">*</span></label>
                            <textarea class="form-control"
                                      id="customer_address"
                                      name="customer_address"
                                      rows="3"
                                      required
                                      minlength="10"
                                      placeholder="Nhập địa chỉ chi tiết để giao hàng"><?php echo isset($_POST['customer_address']) ? htmlspecialchars($_POST['customer_address'], ENT_QUOTES, 'UTF-8') : ''; ?></textarea>
                            <div class="invalid-feedback">
                                Vui lòng nhập địa chỉ giao hàng (ít nhất 10 ký tự).
                            </div>
                        </div>

                        <div class="col-md-12 mb-3">
                            <label for="order_notes" class="form-label">Ghi chú đơn hàng (tùy chọn)</label>
                            <textarea class="form-control"
                                      id="order_notes"
                                      name="order_notes"
                                      rows="2"
                                      placeholder="Ghi chú thêm về đơn hàng (thời gian giao hàng, yêu cầu đặc biệt...)"><?php echo isset($_POST['order_notes']) ? htmlspecialchars($_POST['order_notes'], ENT_QUOTES, 'UTF-8') : ''; ?></textarea>
                        </div>
                    </div>

                    <div class="d-flex justify-content-between">
                        <a href="/webbanhang/Product/cart" class="btn btn-outline-secondary">
                            <i class="fas fa-arrow-left me-2"></i>Quay lại giỏ hàng
                        </a>
                        <button type="submit" class="btn btn-success btn-lg">
                            <i class="fas fa-check me-2"></i>Xác nhận đặt hàng
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>

    <div class="col-lg-4">
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0"><i class="fas fa-receipt me-2"></i>Tóm tắt đơn hàng</h5>
            </div>
            <div class="card-body">
                <!-- Danh sách sản phẩm -->
                <div class="mb-3">
                    <?php foreach ($cartItems as $productId => $item): ?>
                        <div class="d-flex justify-content-between align-items-center mb-2 pb-2 border-bottom">
                            <div class="d-flex align-items-center">
                                <?php if ($item['image']): ?>
                                    <img src="/webbanhang/<?php echo $item['image']; ?>"
                                         alt="<?php echo htmlspecialchars($item['name'], ENT_QUOTES, 'UTF-8'); ?>"
                                         class="img-thumbnail me-2" style="width: 40px; height: 40px; object-fit: cover;">
                                <?php else: ?>
                                    <img src="https://via.placeholder.com/40x40?text=No+Image"
                                         alt="No Image" class="img-thumbnail me-2" style="width: 40px; height: 40px;">
                                <?php endif; ?>
                                <div>
                                    <h6 class="mb-0 small"><?php echo htmlspecialchars($item['name'], ENT_QUOTES, 'UTF-8'); ?></h6>
                                    <small class="text-muted">SL: <?php echo $item['quantity']; ?></small>
                                </div>
                            </div>
                            <span class="fw-bold small">
                                <?php echo number_format($item['price'] * $item['quantity'], 0, ',', '.'); ?> VND
                            </span>
                        </div>
                    <?php endforeach; ?>
                </div>

                <!-- Tính toán -->
                <div class="d-flex justify-content-between mb-2">
                    <span>Tạm tính:</span>
                    <span class="fw-bold">
                        <?php echo number_format($totalAmount, 0, ',', '.'); ?> VND
                    </span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                    <span>Phí vận chuyển:</span>
                    <span class="text-success fw-bold">Miễn phí</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                    <span>Thuế VAT:</span>
                    <span class="text-muted">Đã bao gồm</span>
                </div>
                <hr>
                <div class="d-flex justify-content-between mb-3">
                    <span class="h5">Tổng cộng:</span>
                    <span class="h5 text-primary fw-bold">
                        <?php echo number_format($totalAmount, 0, ',', '.'); ?> VND
                    </span>
                </div>

                <!-- Phương thức thanh toán -->
                <div class="alert alert-info">
                    <h6 class="alert-heading"><i class="fas fa-credit-card me-2"></i>Phương thức thanh toán</h6>
                    <p class="mb-0 small">Thanh toán khi nhận hàng (COD)</p>
                </div>

                <!-- Chính sách -->
                <div class="small text-muted">
                    <p class="mb-1"><i class="fas fa-shield-alt me-1"></i> Bảo hành chính hãng</p>
                    <p class="mb-1"><i class="fas fa-truck me-1"></i> Giao hàng toàn quốc</p>
                    <p class="mb-0"><i class="fas fa-undo me-1"></i> Đổi trả trong 7 ngày</p>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- JavaScript for form validation -->
<script>
document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('checkoutForm');

    form.addEventListener('submit', function(event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }

        form.classList.add('was-validated');
    });

    // Phone number formatting
    const phoneInput = document.getElementById('customer_phone');
    phoneInput.addEventListener('input', function() {
        // Remove non-numeric characters
        this.value = this.value.replace(/[^0-9]/g, '');
    });
});
</script>

<?php include 'app/views/shares/footer.php'; ?>
