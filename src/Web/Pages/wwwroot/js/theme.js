const choice = localStorage.getItem('theme');
if (choice != null) {
    document.body.setAttribute('data-theme', choice)
    document.body.setAttribute("data-bs-theme", choice.toLowerCase());
}
else {
    const theme = localStorage.getItem('theme')
        ?? matchMedia('(prefers-color-scheme: dark)').matches
        ? 'Dark' : 'Light';
    if (theme === 'Dark') {
        document.body.setAttribute('data-theme', theme);
        document.body.setAttribute("data-bs-theme", "dark");
    }
}   


