import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import path from 'path';

// Same build contract as SafetyNet: one bundle emitted to wwwroot/scripts/vue/{app.js,app.css}.
export default defineConfig({
    plugins: [vue({ template: { compilerOptions: { whitespace: 'preserve' } } })],
    base: './',
    resolve: { alias: { '@': path.resolve(__dirname, 'src') } },
    build: {
        outDir: path.resolve(__dirname, '../wwwroot/scripts/vue'),
        emptyOutDir: true,
        cssCodeSplit: false,
        rollupOptions: {
            input: path.resolve(__dirname, 'src/main.js'),
            output: {
                inlineDynamicImports: true,
                entryFileNames: 'app.js',
                assetFileNames: (a) => (a.name.endsWith('.css') ? 'app.css' : '[name][extname]')
            }
        }
    }
});
