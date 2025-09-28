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
});
