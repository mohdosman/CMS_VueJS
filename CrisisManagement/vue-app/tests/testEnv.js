import fs from 'node:fs';
import path from 'node:path';

const localEnvFile = path.resolve(process.cwd(), '.env.test.local');

// Reads .env.test.local (ignored by git) into process.env; real environment variables win.
export function loadTestEnv() {
    if (!fs.existsSync(localEnvFile)) return;
    for (const line of fs.readFileSync(localEnvFile, 'utf8').split(/\r?\n/)) {
        const trimmed = line.trim();
        if (!trimmed || trimmed.startsWith('#')) continue;
        const at = trimmed.indexOf('=');
        if (at === -1) continue;
        const key = trimmed.slice(0, at).trim();
        const value = trimmed.slice(at + 1).trim().replace(/^['"]|['"]$/g, '');
        if (key && process.env[key] === undefined) process.env[key] = value;
    }
}
