<script setup>
import { ref } from 'vue';
import { usersApi } from '../api/usersApi.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Uploads PDFs on selection, one at a time. The server checks type and size again and is the one that counts.
// Closes with true when anything was stored.
const props = defineProps({ userKey: { type: String, required: true } });
const emit = defineEmits(['close']);

const MAX_BYTES = 10 * 1024 * 1024;
const MAX_FILES = 10;

const results = ref([]);   // [{ name, error }]
const isBusy = ref(false);
const stored = ref(0);

async function uploadOne(file) {
    if (file.size > MAX_BYTES) return { name: file.name, error: `Exceeds the ${MAX_BYTES / (1024 * 1024)} MB limit.` };
    try {
        await usersApi.uploadDocument(props.userKey, file);
        stored.value++;
        return { name: file.name, error: null };
    } catch (e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors?.file : null;
        return { name: file.name, error: fieldErrors?.join(' ') ?? apiErrorMessage(e) };
    }
}

async function onPick(e) {
    const files = [...e.target.files].slice(0, MAX_FILES);
    e.target.value = '';
    isBusy.value = true;
    try {
        for (const f of files) results.value.push(await uploadOne(f));
    } finally {
        isBusy.value = false;
    }
    const ok = results.value.filter((r) => !r.error).length;
    announce(`${ok} of ${results.value.length} files uploaded`);
}
</script>

<template>
    <AppDialog title="Upload User Agreement" @close="emit('close', stored > 0)">
        <div class="mb-3">
            <label class="form-label" for="agreementFile">PDF files</label>
            <input id="agreementFile" type="file" accept=".pdf,application/pdf" multiple class="form-control form-control-sm"
                   :disabled="isBusy" aria-describedby="agreementFile-help" @change="onPick" />
            <div id="agreementFile-help" class="form-text">PDF only, up to 10 MB each and {{ MAX_FILES }} files at a time. Files upload as soon as you choose them.</div>
        </div>

        <ul v-if="results.length" class="list-unstyled" aria-live="polite">
            <li v-for="(r, i) in results" :key="i" :class="r.error ? 'text-danger' : 'text-success'">
                {{ r.name }}: {{ r.error ?? 'uploaded' }}
            </li>
        </ul>
        <p v-if="isBusy" role="status">Uploading...</p>

        <AppButton action="close" @click="emit('close', stored > 0)" />
    </AppDialog>
</template>
