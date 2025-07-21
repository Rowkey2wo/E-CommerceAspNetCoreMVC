document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("userSearch");
    const roleRadios = document.querySelectorAll("input[name='flexRadioDefault']");
    const userCards = document.querySelectorAll("[data-email]");

    function filterUsers() {
        const searchQuery = searchInput.value.toLowerCase();
        const selectedRole = [...roleRadios].find(r => r.checked).nextElementSibling.textContent.trim().toLowerCase();

        userCards.forEach(card => {
            const email = card.dataset.email;
            const role = card.dataset.role;

            const matchesEmail = email.includes(searchQuery);
            const matchesRole = selectedRole === "all" || role === selectedRole;

            card.style.display = (matchesEmail && matchesRole) ? "block" : "none";
        });
    }

    if (searchInput) {
        searchInput.addEventListener("input", filterUsers);
    }

    roleRadios.forEach(radio => radio.addEventListener("change", filterUsers));
});
