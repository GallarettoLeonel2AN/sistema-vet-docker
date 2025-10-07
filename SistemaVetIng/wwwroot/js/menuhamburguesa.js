// Logica Menu Hamburguesa para celulares

function initializeMenu() {
    const hamburger = document.querySelector('.hamburger-menu');
    const mainNav = document.querySelector('.main-nav');

    if (hamburger && mainNav) {
        hamburger.addEventListener('click', () => {
            mainNav.classList.toggle('active');
            hamburger.classList.toggle('is-active'); 
        });
    }
}

document.addEventListener('DOMContentLoaded', initializeMenu);