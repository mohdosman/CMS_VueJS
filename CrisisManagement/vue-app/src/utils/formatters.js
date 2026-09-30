// Display formats shared by the list and detail screens.

// "1/2/2026", or `empty` when there is no value.
export const formatDate = (v, empty = '-') => (v ? new Date(v).toLocaleDateString('en-US') : empty);

// "1/2/2026, 3:04:05 PM", or `empty` when there is no value.
export const formatDateTime = (v, empty = '') => (v ? new Date(v).toLocaleString('en-US') : empty);

// 1536 -> "2 KB", 5242880 -> "5.00 MB".
export const formatFileSize = (b) => {
    if (b == null) return '';
    return b >= 1048576 ? `${(b / 1048576).toFixed(2)} MB` : b >= 1024 ? `${Math.round(b / 1024)} KB` : `${b} Bytes`;
};

// Long text cut for a grid cell (the full text goes in the title).
export const truncate = (text, max = 50) => (text.length > max ? `${text.slice(0, max)}...` : text);
