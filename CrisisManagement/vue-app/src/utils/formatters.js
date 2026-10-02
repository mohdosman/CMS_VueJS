// Display formats shared by the list and detail screens.

// "1/2/2026", or `empty` when there is no value.
export const formatDate = (v, empty = '-') => (v ? new Date(v).toLocaleDateString('en-US') : empty);

// "1/2/2026, 3:04:05 PM", or `empty` when there is no value.
export const formatDateTime = (v, empty = '') => (v ? new Date(v).toLocaleString('en-US') : empty);

// "01/02/2026 03:04:05 PM", the legacy grids' Date Uploaded format.
export const formatDateTimeFull = (v, empty = '') => {
    if (!v) return empty;
    const d = new Date(v);
    const p = (n) => String(n).padStart(2, '0');
    return `${p(d.getMonth() + 1)}/${p(d.getDate())}/${d.getFullYear()} ${p(d.getHours() % 12 || 12)}:${p(d.getMinutes())}:${p(d.getSeconds())} ${d.getHours() < 12 ? 'AM' : 'PM'}`;
};

// 1536 -> "2 KB", 5242880 -> "5.00 MB".
export const formatFileSize = (b) => {
    if (b == null) return '';
    return b >= 1048576 ? `${(b / 1048576).toFixed(2)} MB` : b >= 1024 ? `${Math.round(b / 1024)} KB` : `${b} Bytes`;
};

// One-line XML -> one element per line, indented two spaces per level (text-only elements stay on one line).
export const prettyXml = (xml) => {
    let depth = 0;
    return (xml || '').trim().replace(/>\s*</g, '>\n<').split('\n').map((line) => {
        if (/^<\//.test(line)) depth = Math.max(depth - 1, 0);
        const out = '  '.repeat(depth) + line;
        if (/^<[^!?/][^>]*[^/]>$/.test(line) && !line.includes('</')) depth++;
        return out;
    }).join('\n');
};

// Long text cut for a grid cell (the full text goes in the title).
export const truncate = (text, max = 50) => (text.length > max ? `${text.slice(0, max)}...` : text);
