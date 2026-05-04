function setTheme(theme) {
    localStorage.setItem('theme', theme);
    
    let html = document.getElementsByTagName('html')[0];
    html.setAttribute('data-bs-theme', theme);
}

function getTheme() {
    return localStorage.getItem('theme') || 'light';
}
