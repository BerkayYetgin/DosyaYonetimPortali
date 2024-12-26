document.addEventListener('DOMContentLoaded', function() {
    // Scroll olayını dinle
    window.addEventListener('scroll', function() {
        const navbar = document.querySelector('.modern-navbar');
        if (window.scrollY > 10) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });

    // Mobil menü toggle
    const navToggle = document.querySelector('.nav-toggle');
    const navMenu = document.querySelector('.nav-menu');

    if (navToggle) {
        navToggle.addEventListener('click', function() {
            navMenu.classList.toggle('active');
            document.body.classList.toggle('menu-open');
        });
    }
}); 