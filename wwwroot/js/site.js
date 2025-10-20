// Site JavaScript
document.addEventListener('DOMContentLoaded', function() {
    console.log('WebCookmate đã sẵn sàng!');
    
    // Smooth scrolling cho navigation links (chỉ cho anchor links)
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            const targetId = this.getAttribute('href');
            console.log('Nav link clicked:', targetId, this.textContent);
            
            // Chỉ prevent default cho anchor links (#)
            if (targetId.startsWith('#')) {
                e.preventDefault();
                const targetElement = document.querySelector(targetId);
                if (targetElement) {
                    targetElement.scrollIntoView({
                        behavior: 'smooth'
                    });
                }
            } else {
                // Cho các link khác (như Home), để browser tự xử lý navigation
                console.log('Allowing navigation to:', targetId);
            }
        });
    });
    
    // Animation cho feature cards
    const featureCards = document.querySelectorAll('.feature-card');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    });
    
    featureCards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(card);
    });

    // Global search functionality
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                const searchTerm = this.value.trim();
                if (searchTerm) {
                    // Redirect to Recipes page with search term
                    window.location.href = `/Home/Recipes?search=${encodeURIComponent(searchTerm)}`;
                }
            }
        });

        // Add search suggestions
        searchInput.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            if (searchTerm.length > 2) {
                showSearchSuggestions(searchTerm);
            } else {
                hideSearchSuggestions();
            }
        });
    }
});

// Search suggestions functionality
function showSearchSuggestions(searchTerm) {
    const suggestions = [
        'Bánh mì', 'Phở bò', 'Cơm tấm', 'Kho quẹt', 'Chè đậu đỏ',
        'Bữa sáng', 'Bữa trưa', 'Bữa tối', 'Đồ ăn vặt', 'Tráng miệng',
        'Dễ', 'Trung bình', 'Khó', 'Dưới 15 phút', 'Dưới 30 phút'
    ];
    
    const filteredSuggestions = suggestions.filter(suggestion => 
        suggestion.toLowerCase().includes(searchTerm)
    );
    
    if (filteredSuggestions.length > 0) {
        createSuggestionsDropdown(filteredSuggestions);
    }
}

function createSuggestionsDropdown(suggestions) {
    // Remove existing dropdown
    hideSearchSuggestions();
    
    const searchContainer = document.querySelector('.search-bar');
    const dropdown = document.createElement('div');
    dropdown.className = 'search-suggestions';
    dropdown.innerHTML = suggestions.map(suggestion => 
        `<div class="suggestion-item" onclick="selectSuggestion('${suggestion}')">${suggestion}</div>`
    ).join('');
    
    searchContainer.appendChild(dropdown);
}

function hideSearchSuggestions() {
    const existingDropdown = document.querySelector('.search-suggestions');
    if (existingDropdown) {
        existingDropdown.remove();
    }
}

function selectSuggestion(suggestion) {
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.value = suggestion;
        hideSearchSuggestions();
        // Redirect to Recipes page with selected suggestion
        window.location.href = `/Home/Recipes?search=${encodeURIComponent(suggestion)}`;
    }
}
