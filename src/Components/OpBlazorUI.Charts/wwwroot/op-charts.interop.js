// Lê cores/fonte do tema (tokens CSS) para alimentar o OpChart. Sem tokens disponíveis,
// devolve uma paleta de fallback.
export function getChartTheme() {
    const styles = getComputedStyle(document.documentElement);
    const get = (name, fallback) => {
        const value = (styles.getPropertyValue(name) || '').trim();
        return value.length > 0 ? value : fallback;
    };

    return {
        colors: [
            get('--p-primary-color', '#6366f1'),
            get('--p-primary-400', '#818cf8'),
            get('--p-primary-200', '#c7d2fe'),
            get('--p-surface-400', '#9ca3af'),
            get('--p-surface-600', '#4b5563')
        ],
        foreColor: get('--p-text-color', '#495057'),
        fontFamily: get('--p-font-family', 'inherit')
    };
}
