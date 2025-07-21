document.addEventListener('DOMContentLoaded', function () {
    let cartItems = [];
    let totalAmount = 0;
    const cartCount = document.querySelector('.cart-icon span');
    const cartList = document.querySelector('.cart-items');
    const cartTotal = document.querySelector('.cart-total');

    // Sidebar open/close
    document.querySelector('.cart-icon').addEventListener('click', function () {
        document.getElementById('cartbar').classList.toggle('open');
    });

    document.querySelector('.cartbar-close').addEventListener('click', function () {
        document.getElementById('cartbar').classList.remove('open');
        cleanUpBackdrop();
    });

    // Handle Add to Cart
    const addToCartButtons = document.querySelectorAll('.add-to-cart');
    addToCartButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            const cardBody = button.closest('.card-body');
            const name = cardBody.querySelector('.card-title').textContent.trim();
            const priceText = cardBody.querySelector('.price').textContent.replace('P', '').trim();
            const price = parseFloat(priceText);
            const id = parseInt(button.dataset.id);
            let stock = parseInt(button.dataset.stock);

            if (stock <= 0) return;

            // Check if already in cart
            const existingItem = cartItems.find(item => item.id === id);
            if (existingItem) {
                if (existingItem.quantity >= stock) return;
                existingItem.quantity++;
            } else {
                cartItems.push({ id: id, name: name, price: price, quantity: 1 });
            }

            stock--;
            button.dataset.stock = stock;

            if (stock === 0) {
                button.disabled = true;
                button.classList.remove('btn-success');
                button.classList.add('btn-secondary');
                button.textContent = 'Out of Stock';
            }

            totalAmount += price;
            updateCart();
        });
    });

    function updateCart() {
        cartCount.textContent = cartItems.reduce((sum, item) => sum + item.quantity, 0);
        cartList.innerHTML = '';

        cartItems.forEach(function (item, index) {
            const itemDiv = document.createElement('div');
            itemDiv.classList.add('cart-item');
            itemDiv.innerHTML = `
                <span>(${item.quantity}x) ${item.name}</span>
                <span class="cart-item-price">
                    P${(item.quantity * item.price).toFixed(2)}
                    <button class="remove-btn" data-index="${index}"><i class="fa-solid fa-times"></i></button>
                </span>
            `;
            cartList.appendChild(itemDiv);
        });

        // Remove item
        const removeButtons = document.querySelectorAll('.remove-btn');
        removeButtons.forEach(function (btn) {
            btn.addEventListener('click', function () {
                const i = parseInt(btn.getAttribute('data-index'));
                totalAmount -= cartItems[i].price * cartItems[i].quantity;
                cartItems.splice(i, 1);
                updateCart();
            });
        });

        cartTotal.textContent = `P${totalAmount.toFixed(2)}`;
    }

    // --- FILTERING SECTION ---
    const btnFilter = document.getElementById('btnFilter');
    const priceInput = document.getElementById('price-range');
    const searchInput = document.querySelector('.page1--searchInput');

    priceInput.addEventListener('input', function () {
        document.getElementById('price-output').textContent = '₱' + priceInput.value;
    });

    btnFilter.addEventListener('click', function () {
        const maxPrice = parseFloat(priceInput.value);
        const searchText = searchInput.value.toLowerCase();

        const selectedRadio = document.querySelector('input[name="flexRadioDefault"]:checked');
        const selectedCategory = selectedRadio?.nextElementSibling?.textContent.trim().toLowerCase() || 'all food';

        const allItems = document.querySelectorAll('.food--item');

        console.log('Filter criteria:', { searchText, maxPrice, selectedCategory });

        allItems.forEach(function (item) {
            const name = item.querySelector('.card-title').textContent.toLowerCase();
            const desc = item.querySelector('.card-text').textContent.toLowerCase();
            const price = parseFloat(item.querySelector('.price').textContent.replace('P', '').trim());
            const category = item.querySelector('.card-title').getAttribute('data-category')?.toLowerCase() || '';

            const matchSearch = name.includes(searchText) || desc.includes(searchText);
            const matchPrice = price <= maxPrice;
            const matchCategory = selectedCategory === 'all food' || category === selectedCategory;

            console.log(`Food: ${name} | Price: ${price} | Category: ${category}`);
            console.log(`Match? search:${matchSearch}, price:${matchPrice}, category:${matchCategory}`);

            if (matchSearch && matchPrice && matchCategory) {
                item.style.display = '';
            } else {
                item.style.display = 'none';
            }
        });
    });

    // --- CHECKOUT SECTION ---
    document.querySelector('.checkout-btn').addEventListener('click', function () {
        const address = document.getElementById("sessionUserAddress").value;

        if (!address || address.trim() === "") {
            // Show the missing address modal
            const addressWarningModal = new bootstrap.Modal(document.getElementById('addressWarningModal'));
            addressWarningModal.show();
            return; // prevent checkout modal
        }

        if (cartItems.length === 0) {
            alert("Your cart is empty.");
            return;
        }

        // Populate modal items list
        const list = document.getElementById('checkoutItemsList');
        list.innerHTML = '';
        let discount = 0;
        cartItems.forEach(item => {
            const li = document.createElement('li');
            li.classList.add('list-group-item', 'd-flex', 'justify-content-between');
            li.innerHTML = `
            <span>${item.quantity}x ${item.name}</span>
            <span>₱${(item.price * item.quantity).toFixed(2)}</span>
        `;
            list.appendChild(li);
        });

        // Calculate and show totals
        document.getElementById('checkoutTotal').textContent = `₱${totalAmount.toFixed(2)}`;

        if (totalAmount >= 500) discount = totalAmount * 0.1;
        const final = totalAmount - discount;

        document.getElementById('checkoutDiscount').textContent = `₱${discount.toFixed(2)}`;
        document.getElementById('checkoutFinalTotal').textContent = `₱${final.toFixed(2)}`;

        // Show modal
        new bootstrap.Modal(document.getElementById('checkoutModal')).show();
    });

    // Confirm checkout
    document.getElementById('confirmCheckout').addEventListener('click', function () {
        const modeOfPayment = document.getElementById('modeOfPayment').value;
        const discount = parseFloat(document.getElementById('checkoutDiscount').textContent.replace('₱', ''));
        const finalAmount = totalAmount - discount;

        fetch('/Transaction/Checkout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                items: cartItems,
                totalAmount: totalAmount,
                discount: discount,
                finalAmount: finalAmount,
                modeOfPayment: modeOfPayment
            })
        })
            .then(async response => {
                const contentType = response.headers.get("content-type");
                if (contentType && contentType.includes("application/json")) {
                    return response.json();
                } else {
                    const text = await response.text();
                    throw new Error("Non-JSON response: " + text);
                }
            })
            .then(data => {
                if (data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                } else if (data?.message) {
                    alert(data.message);
                    location.reload();
                }
            })
            .catch(error => {
                console.error("Checkout failed", error);
                alert("An error occurred while processing the order.");
            });
    });

    // Manually clean up modal backdrop and body scroll
    function cleanUpBackdrop() {
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.remove();
        document.body.classList.remove('modal-open');
        document.body.style.overflow = 'auto';
    }
    document.getElementById('checkoutModal').addEventListener('hidden.bs.modal', cleanUpBackdrop);


});
