(() => {
    // Theme init
    const pref = localStorage.getItem('theme') || 'auto';
    const root = document.documentElement;
    const setTheme = (t) => {
        root.setAttribute('data-bs-theme', t === 'auto' ? (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light') : t);
        localStorage.setItem('theme', t);
        document.querySelectorAll('[data-theme-value]').forEach(b => b.classList.toggle('active', b.getAttribute('data-theme-value') === t));
    };
    setTheme(pref);
    window.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-theme-value]').forEach(btn => btn.addEventListener('click', () => setTheme(btn.getAttribute('data-theme-value'))));


        // Bootstrap toasts auto show
        document.querySelectorAll('.toast').forEach(t => new bootstrap.Toast(t, { delay: 3000 }).show());


        // Confirm delete modal wiring
        const confirmModal = document.getElementById('confirmDeleteModal');
        if (confirmModal) {
            confirmModal.addEventListener('show.bs.modal', (e) => {
                const button = e.relatedTarget;
                const formId = button?.getAttribute('data-form');
                if (formId) {
                    confirmModal.querySelector('#confirmDeleteYes').onclick = () => document.getElementById(formId).submit();
                }
            });
        }
    });
})();