import optimusUiTailwindcss from '@openng/optimus-ui-tailwindcss';

export default {
    content: [
        './Components/**/*.razor',
        './wwwroot/index.html'
    ],
    darkMode: ['selector', '[class="app-dark"]'],
    corePlugins: {
        preflight: false
    },
    theme: {
        screens: {
            sm: '576px',
            md: '768px',
            lg: '992px',
            xl: '1200px',
            '2xl': '1920px'
        }
    },
    plugins: [
        optimusUiTailwindcss
    ]
};