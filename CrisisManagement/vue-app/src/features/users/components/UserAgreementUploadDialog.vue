<script setup>
import { useUserAgreementUpload } from '../composables/useUserAgreementUpload.js';

// Closes with true when anything was stored.
const props = defineProps({ userKey: { type: String, required: true } });
const emit = defineEmits(['close']);

const { results, stored, MAX_FILES, isBusy, onPick } = useUserAgreementUpload(props);
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
