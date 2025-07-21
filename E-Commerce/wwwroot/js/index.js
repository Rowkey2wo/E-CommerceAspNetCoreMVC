document.addEventListener('DOMContentLoaded', () => {
    let allFoodData = [];
    let cartItems = [];
    let totalAmount = 0;

    const cartItemCount = document.querySelector('.cart-icon span');
    const cartItemList = document.querySelector('.cart-items');
    const cartTotal = document.querySelector('.cart-total');
    const cartIcon = document.querySelector('.cart-icon');
    const sidebar = document.getElementById('sidebar');
    const closeButton = document.querySelector('.sidebar-close');
    const rangeInput = document.getElementById('price-range');
    const priceOutput = document.getElementById('price-output');
    const btnFilter = document.getElementById('btnFilter');

    fetch('/js/FoodData.json')
        .then(response => response.json())
        .then(data => {
            allFoodData = data;
            renderFoodItems(allFoodData);
            initializeCartButtons();
        })
        .catch(error => {
            console.log(error);
        });

    function renderFoodItems(foodArray) {
        const foodContainer = document.getElementById('food--container');
        const mainText = document.getElementById('MainText');

        foodContainer.innerHTML = '';
        if (foodArray.length === 0) {
            const noItemsMessage = document.createElement('h1');
            noItemsMessage.classList.add('text-center', 'text-muted', 'my-5', 'fs-5');
            noItemsMessage.textContent = 'There are no available products on our menu that match your filter.';
            foodContainer.appendChild(noItemsMessage);
            return;
        }

        const gridWrapper = document.createElement('div');
        gridWrapper.classList.add('food--menu', 'mb-5', 'row', 'row-cols-1', 'row-cols-md-3', 'row-cols-lg-4', 'gap-5', 'd-flex', 'justify-content-center');
        foodContainer.appendChild(gridWrapper);

        foodArray.forEach((food, index) => {
            const item = document.createElement('div');
            item.classList.add('food--item', 'col');

            const card = document.createElement('div');
            card.classList.add('card', 'h-100');
            card.innerHTML = `
                <div class="h-100 d-flex align-items-center">
                    <img src="${food.image}" class="card-img-top" alt="${food.name}">
                </div>
                <div class="card-body d-flex flex-column">
                    <h5 class="card-title">${food.name}</h5>
                    <p class="card-text text-secondary">${food.description}</p>
                    <div class="card--price d-flex flex-column align-items-start">
                        <p class="price card-text fw-bold">P${food.price}</p>
                        <button data-index="${index}" class="add-to-cart btn btn-success">Add To Cart</button>
                    </div>
                </div>
            `;
            item.appendChild(card);
            gridWrapper.appendChild(item);
        });

        initializeCartButtons();
    }


    function initializeCartButtons() {
        const btns = document.querySelectorAll('.add-to-cart');
        btns.forEach(button => {
            button.addEventListener('click', () => {
                const index = parseInt(button.dataset.index, 10);
                const food = allFoodData[index];
                if (!food) return;

                const existingFood = cartItems.find(item => item.name === food.name);
                if (existingFood) {
                    existingFood.quantity++;
                } else {
                    cartItems.push({ name: food.name, price: food.price, quantity: 1 });
                }

                totalAmount += food.price;
                updateCartUI();
            });
        });
    }

    function updateCartUI() {
        updateCartItemCount(cartItems.length);
        updateCartItemList();
        updateCartTotal();
    }

    function updateCartItemCount(count) {
        cartItemCount.textContent = count;
    }

    function updateCartItemList() {
        cartItemList.innerHTML = '';
        cartItems.forEach((item, index) => {
            const cartItem = document.createElement('div');
            cartItem.classList.add('cart-item', 'individual-cart-item');
            cartItem.innerHTML = `
                <span>(${item.quantity}x) ${item.name}</span>
                <span class="cart-item-price">P${(item.price * item.quantity).toFixed(2)}
                    <button class="remove-btn" data-index="${index}"><i class="fa-solid fa-times"></i></button>
                </span>
            `;
            cartItemList.appendChild(cartItem);
        });

        document.querySelectorAll('.remove-btn').forEach(button => {
            button.addEventListener('click', (e) => {
                const index = e.target.closest('button').dataset.index;
                removeItemFromCart(index);
            });
        });
    }

    function updateCartTotal() {
        cartTotal.textContent = `P${totalAmount.toFixed(2)}`;
    }

    function removeItemFromCart(index) {
        const removedItem = cartItems.splice(index, 1)[0];
        totalAmount -= removedItem.price * removedItem.quantity;
        updateCartUI();
    }

    if (cartIcon) {
        cartIcon.addEventListener('click', () => {
            sidebar.classList.toggle('open');
        });
    }

    if (closeButton) {
        closeButton.addEventListener('click', () => {
            sidebar.classList.remove('open');
        });
    }

    priceOutput.textContent = rangeInput.value;
    rangeInput.addEventListener('input', () => {
        priceOutput.textContent = `₱${rangeInput.value}`;
    });

    if (btnFilter !== null) {
        btnFilter.addEventListener('click', function () {
            // Get the search input
            const searchInput = document.querySelector('.page1--searchInput');
            let searchText = '';
            if (searchInput !== null) {
                searchText = searchInput.value.toLowerCase();
            }

            // Get the selected maximum price
            const maxPrice = parseFloat(rangeInput.value);

            // Get the selected category
            const selectedRadio = document.querySelector('input[name="flexRadioDefault"]:checked');
            let selectedCategory = 'all food';
            if (selectedRadio !== null) {
                const label = selectedRadio.nextElementSibling;
                if (label !== null) {
                    selectedCategory = label.textContent.trim().toLowerCase();
                }
            }

            // Start filtering the food data
            const filteredFoods = [];

            for (let i = 0; i < allFoodData.length; i++) {
                const food = allFoodData[i];
                const nameMatch = food.name.toLowerCase().includes(searchText);
                const descMatch = food.description.toLowerCase().includes(searchText);
                const priceMatch = food.price <= maxPrice;

                let categoryMatch = true;
                if (selectedCategory !== 'all food') {
                    if (food.category) {
                        categoryMatch = food.category.toLowerCase() === selectedCategory;
                    } else {
                        categoryMatch = false;
                    }
                }

                if ((nameMatch || descMatch) && priceMatch && categoryMatch) {
                    filteredFoods.push(food);
                }
            }

            renderFoodItems(filteredFoods);
        });
    }

});
