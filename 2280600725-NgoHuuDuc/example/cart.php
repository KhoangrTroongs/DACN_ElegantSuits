<?php include 'app/views/shares/header.php'; ?>

<!-- Breadcrumb -->
<nav aria-label="breadcrumb" class="mb-4">
    <ol class="breadcrumb">
        <li class="breadcrumb-item"><a href="/webbanhang/Product/">Trang chủ</a></li>
        <li class="breadcrumb-item active" aria-current="page">Giỏ hàng</li>
    </ol>
</nav>



<div class="row">
    <div class="col-lg-8">
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0"><i class="fas fa-shopping-cart me-2"></i>Giỏ hàng của bạn</h5>
            </div>
            <div class="card-body">
                <?php if (empty($cartItems)): ?>
                    <!-- Giỏ hàng rỗng -->
                    <div class="text-center py-5">
                        <i class="fas fa-shopping-cart fa-3x text-muted mb-3"></i>
                        <h4 class="text-muted">Giỏ hàng của bạn đang trống</h4>
                        <p class="text-muted">Hãy thêm một số sản phẩm vào giỏ hàng để tiếp tục mua sắm.</p>
                        <a href="/webbanhang/Product/list" class="btn btn-primary">
                            <i class="fas fa-arrow-left me-2"></i>Tiếp tục mua sắm
                        </a>
                    </div>
                <?php else: ?>
                    <!-- Bảng sản phẩm -->
                    <form action="/webbanhang/Product/updateCart" method="POST" id="cartForm">
                        <div class="table-responsive">
                            <table class="table table-hover">
                                <thead class="table-light">
                                    <tr>
                                        <th>Hình ảnh</th>
                                        <th>Tên sản phẩm</th>
                                        <th>Giá</th>
                                        <th>Số lượng</th>
                                        <th>Thành tiền</th>
                                        <th>Thao tác</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <?php foreach ($cartItems as $productId => $item): ?>
                                        <tr>
                                            <td>
                                                <?php if ($item['image']): ?>
                                                    <img src="/webbanhang/<?php echo $item['image']; ?>"
                                                         alt="<?php echo htmlspecialchars($item['name'], ENT_QUOTES, 'UTF-8'); ?>"
                                                         class="img-thumbnail" style="width: 60px; height: 60px; object-fit: cover;">
                                                <?php else: ?>
                                                    <img src="https://via.placeholder.com/60x60?text=No+Image"
                                                         alt="No Image" class="img-thumbnail" style="width: 60px; height: 60px;">
                                                <?php endif; ?>
                                            </td>
                                            <td>
                                                <h6 class="mb-0"><?php echo htmlspecialchars($item['name'], ENT_QUOTES, 'UTF-8'); ?></h6>
                                            </td>
                                            <td>
                                                <span class="text-primary fw-bold">
                                                    <?php echo number_format($item['price'], 0, ',', '.'); ?> VND
                                                </span>
                                            </td>
                                            <td>
                                                <div class="input-group" style="width: 120px;">
                                                    <input type="number"
                                                           name="quantities[<?php echo $productId; ?>]"
                                                           value="<?php echo $item['quantity']; ?>"
                                                           min="1"
                                                           max="99"
                                                           class="form-control text-center quantity-input"
                                                           data-price="<?php echo $item['price']; ?>"
                                                           data-product-id="<?php echo $productId; ?>">
                                                </div>
                                            </td>
                                            <td>
                                                <span class="fw-bold subtotal" data-product-id="<?php echo $productId; ?>">
                                                    <?php echo number_format($item['price'] * $item['quantity'], 0, ',', '.'); ?> VND
                                                </span>
                                            </td>
                                            <td>
                                                <a href="/webbanhang/Product/removeFromCart/<?php echo $productId; ?>"
                                                   class="btn btn-sm btn-outline-danger"
                                                   onclick="return confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')">
                                                    <i class="fas fa-trash"></i>
                                                </a>
                                            </td>
                                        </tr>
                                    <?php endforeach; ?>
                                </tbody>
                            </table>
                        </div>

                        <div class="d-flex justify-content-between align-items-center mt-3">
                            <a href="/webbanhang/Product/list" class="btn btn-outline-secondary">
                                <i class="fas fa-arrow-left me-2"></i>Tiếp tục mua sắm
                            </a>
                            <button type="submit" class="btn btn-primary">
                                <i class="fas fa-sync-alt me-2"></i>Cập nhật giỏ hàng
                            </button>
                        </div>
                    </form>
                <?php endif; ?>
            </div>
        </div>
    </div>

    <?php if (!empty($cartItems)): ?>
    <div class="col-lg-4">
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0"><i class="fas fa-calculator me-2"></i>Tóm tắt đơn hàng</h5>
            </div>
            <div class="card-body">
                <div class="d-flex justify-content-between mb-2">
                    <span>Số lượng sản phẩm:</span>
                    <span class="fw-bold" id="totalItems">
                        <?php
                        $totalItems = 0;
                        foreach ($cartItems as $item) {
                            $totalItems += $item['quantity'];
                        }
                        echo $totalItems;
                        ?>
                    </span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                    <span>Tạm tính:</span>
                    <span class="fw-bold" id="subtotalAmount">
                        <?php echo number_format($totalAmount, 0, ',', '.'); ?> VND
                    </span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                    <span>Phí vận chuyển:</span>
                    <span class="text-success fw-bold">Miễn phí</span>
                </div>
                <hr>
                <div class="d-flex justify-content-between mb-3">
                    <span class="h5">Tổng cộng:</span>
                    <span class="h5 text-primary fw-bold" id="totalAmount">
                        <?php echo number_format($totalAmount, 0, ',', '.'); ?> VND
                    </span>
                </div>
                <a href="/webbanhang/Product/checkout" class="btn btn-success w-100 btn-lg">
                    <i class="fas fa-credit-card me-2"></i>Tiến hành thanh toán
                </a>
            </div>
        </div>
    </div>
    <?php endif; ?>
</div>

<!-- JavaScript for real-time calculation -->
<script>
document.addEventListener('DOMContentLoaded', function() {
    const quantityInputs = document.querySelectorAll('.quantity-input');

    quantityInputs.forEach(input => {
        input.addEventListener('input', function() {
            updateSubtotal(this);
            updateTotal();
        });
    });

    function updateSubtotal(input) {
        const price = parseFloat(input.dataset.price);
        const quantity = parseInt(input.value) || 0;
        const productId = input.dataset.productId;
        const subtotal = price * quantity;

        const subtotalElement = document.querySelector(`.subtotal[data-product-id="${productId}"]`);
        if (subtotalElement) {
            subtotalElement.textContent = new Intl.NumberFormat('vi-VN').format(subtotal) + ' VND';
        }
    }

    function updateTotal() {
        let total = 0;
        let totalItems = 0;

        quantityInputs.forEach(input => {
            const price = parseFloat(input.dataset.price);
            const quantity = parseInt(input.value) || 0;
            total += price * quantity;
            totalItems += quantity;
        });

        document.getElementById('totalItems').textContent = totalItems;
        document.getElementById('subtotalAmount').textContent = new Intl.NumberFormat('vi-VN').format(total) + ' VND';
        document.getElementById('totalAmount').textContent = new Intl.NumberFormat('vi-VN').format(total) + ' VND';
    }
});
</script>

<?php include 'app/views/shares/footer.php'; ?>
