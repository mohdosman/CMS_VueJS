import fs from 'node:fs';
import path from 'node:path';
import { parse } from 'espree';
import pluginVue from 'eslint-plugin-vue';
import pluginVueA11y from 'eslint-plugin-vuejs-accessibility';
import globals from 'globals';

/**
 * Single lint pass, two independent gates (both run via `npm run lint`,
 * which gates `npm run build`):
 *
 * 1. Accessibility (WCAG 2.1 AA remediation, 2026-07) — flags a11y
 *    regressions on .vue files as new views are added.
 * 2. Correctness — catches basic bugs like using an identifier (e.g. `ref`)
 *    that was never imported, across .js and .vue files, and destructuring a
 *    key a composable never returns.
 */

// --- local rule: valid-composable-destructure --------------------------------
// `const { showError } = useFoo()` compiles and passes no-undef even when
// useFoo() never returns showError — the binding is just undefined, and the
// view dies at render. Resolve the import and compare the two key sets.
// ponytail: deliberately conservative — anything it can't read statically
// (spread, computed key, non-literal return) is skipped, not guessed at.

const returnKeyCache = new Map();

/** Returns inside fn, ignoring returns belonging to functions nested in it. */
function ownReturns(fn) {
    if (fn.body.type !== 'BlockStatement') return [fn.body]; // () => ({ ... })
    const out = [];
    (function walk(node) {
        if (!node || typeof node.type !== 'string') return;
        if (/^(FunctionDeclaration|FunctionExpression|ArrowFunctionExpression)$/.test(node.type)) return;
        if (node.type === 'ReturnStatement') { out.push(node.argument); return; }
        for (const key of Object.keys(node)) {
            const child = node[key];
            if (Array.isArray(child)) child.forEach(walk);
            else if (child && typeof child.type === 'string') walk(child);
        }
    })(fn.body);
    return out;
}

function exportedFn(ast, name) {
    for (const node of ast.body) {
        if (node.type !== 'ExportNamedDeclaration' || !node.declaration) continue;
        const decl = node.declaration;
        if (decl.type === 'FunctionDeclaration' && decl.id?.name === name) return decl;
        if (decl.type === 'VariableDeclaration') {
            const found = decl.declarations.find(d => d.id.name === name
                && /^(ArrowFunctionExpression|FunctionExpression)$/.test(d.init?.type ?? ''));
            if (found) return found.init;
        }
    }
    return null;
}

/** Keys `name()` returns from `file`, or null when they can't be known statically. */
function returnedKeys(file, name) {
    const cacheKey = `${file}#${name}`;
    if (returnKeyCache.has(cacheKey)) return returnKeyCache.get(cacheKey);

    let keys = null;
    try {
        const source = fs.readFileSync(file, 'utf8').replace(/^﻿/, '');
        const fn = exportedFn(parse(source, { ecmaVersion: 'latest', sourceType: 'module' }), name);
        if (fn) {
            const found = new Set();
            const known = ownReturns(fn).every(arg => {
                if (!arg) return true;                      // bare `return;` guard clause
                if (arg.type !== 'ObjectExpression') return false;
                return arg.properties.every(p => {
                    if (p.type !== 'Property' || p.computed) return false; // spread / computed
                    found.add(p.key.name ?? p.key.value);
                    return true;
                });
            });
            if (known && found.size) keys = found;
        }
    } catch { /* unparseable: skip rather than fail the build */ }

    returnKeyCache.set(cacheKey, keys);
    return keys;
}

function resolveImport(fromDir, source) {
    for (const candidate of [source, `${source}.js`, `${source}/index.js`]) {
        const file = path.resolve(fromDir, candidate);
        if (fs.existsSync(file) && fs.statSync(file).isFile()) return file;
    }
    return null;
}

const validComposableDestructure = {
    meta: {
        type: 'problem',
        docs: { description: 'disallow destructuring a key the composable does not return' },
        schema: []
    },
    create(context) {
        const fromDir = path.dirname(context.filename);
        return {
            VariableDeclarator(node) {
                if (node.id.type !== 'ObjectPattern' || node.init?.type !== 'CallExpression') return;
                const callee = node.init.callee;
                if (callee.type !== 'Identifier' || !/^use[A-Z]/.test(callee.name)) return;

                const decl = context.sourceCode.ast.body.find(n => n.type === 'ImportDeclaration'
                    && n.specifiers.some(s => s.local.name === callee.name));
                const source = decl?.source.value;
                if (typeof source !== 'string' || !source.startsWith('.')) return; // local files only

                const file = resolveImport(fromDir, source);
                const keys = file && returnedKeys(file, callee.name);
                if (!keys) return;

                for (const prop of node.id.properties) {
                    if (prop.type !== 'Property' || prop.computed) continue;
                    const name = prop.key.name ?? prop.key.value;
                    if (!keys.has(name)) {
                        context.report({
                            node: prop,
                            message: `'${name}' is not returned by ${callee.name}() — it will be undefined at runtime.`
                        });
                    }
                }
            }
        };
    }
};

export default [
    // Parse .vue SFCs (eslint-plugin-vue supplies vue-eslint-parser)
    ...pluginVue.configs['flat/base'],

    // --- 1. Accessibility gate -------------------------------------------
    ...pluginVueA11y.configs['flat/recommended'],
    {
        files: ['src/**/*.vue'],
        rules: {
            'vuejs-accessibility/label-has-for': ['error', {
                components: ['label'],
                required: { some: ['nesting', 'id'] },
                allowChildren: true
            }],
            'vuejs-accessibility/click-events-have-key-events': 'off',
            'vuejs-accessibility/no-static-element-interactions': 'off',
            'vuejs-accessibility/no-autofocus': 'off'
        }
    },

    // --- 2. Correctness gate ----------------------------------------------
    {
        files: ['src/**/*.{js,vue}'],
        plugins: { local: { rules: { 'valid-composable-destructure': validComposableDestructure } } },
        languageOptions: {
            ecmaVersion: 'latest',
            sourceType: 'module',
            globals: {
                ...globals.browser
            }
        },
        rules: {
            'no-undef': 'error',
            'local/valid-composable-destructure': 'error'
        }
    },

    {
        ignores: ['node_modules/**', '../wwwroot/**']
    }
];
