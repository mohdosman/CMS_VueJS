<script setup>
import { ref } from 'vue';
import { servicesApi } from '../api/servicesApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Upload one service file (.txt). The server checks it (header, records, footer count, the provider NPI in it) and stores it
// for the nightly import; this page keeps a history of what was uploaded in this visit.
const MAX_BYTES = 5 * 1024 * 1024;
const { logSuccess } = useLogger();

const history = ref([]);          // [{ name, status, messages, id }]
const isUploading = ref(false);
const pickError = ref('');

async function onPick(e) {
    const file = e.target.files[0];
    e.target.value = '';
    pickError.value = '';
    if (!file) return;
    if (!file.name.toLowerCase().endsWith('.txt')) { pickError.value = 'Only .txt files are accepted.'; return; }
    if (file.size > MAX_BYTES) { pickError.value = `The file exceeds the ${MAX_BYTES / (1024 * 1024)} MB limit.`; return; }

    isUploading.value = true;
    try {
        const r = await servicesApi.uploadFile(file);
        history.value.unshift({ name: file.name, status: 'Pending', messages: [`Uploaded for ${r.providerName}. It is imported overnight.`], id: r.id });
        logSuccess(`${file.name} uploaded.`);
    } catch (err) {
        const fieldErrors = err.response?.status === 400 ? err.response.data?.errors?.file : null;
        history.value.unshift({ name: file.name, status: 'Rejected', messages: fieldErrors ?? [apiErrorMessage(err)], id: '' });
        announce(`${file.name} was rejected`);
    } finally {
        isUploading.value = false;
    }
}
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Service File Upload</h1>

        <div class="row">
            <div class="col-sm-12">
                <div class="card">
                    <div class="card-header" role="heading" aria-level="2"><i class="fa fa-upload" aria-hidden="true"></i> Upload Service File</div>
                    <div class="card-body">
                        <div class="alert alert-warning" role="note">
                            <strong>ATTENTION:</strong> Service files are processed only at night, between 6 PM and 6 AM. Do not upload the same file
                            more than once. For help, contact Gina Young at
                            <a href="mailto:gina.young@tn.gov" class="text-decoration-underline">gina.young@tn.gov</a> or 615-532-6675.
                        </div>
                        <div class="row justify-content-center">
                            <div class="col-md-6">
                                <label class="form-label" for="serviceFile">Choose text file</label>
                                <input id="serviceFile" type="file" accept=".txt,text/plain" class="form-control form-control-sm"
                                       :disabled="isUploading" :aria-invalid="!!pickError" aria-describedby="serviceFile-help serviceFile-err" @change="onPick" />
                                <div id="serviceFile-help" class="form-text">
                                    Upload a comma-separated .txt service file for processing. Maximum file size: 5 MB. The file uploads as soon as you choose it.
                                </div>
                                <div id="serviceFile-err" class="form-text has-error" role="alert">{{ pickError }}</div>
                                <p v-if="isUploading" role="status" class="mb-0">Uploading...</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div v-if="history.length" class="row mt-2" role="region" aria-labelledby="history-heading">
            <h2 id="history-heading" class="visually-hidden">Files uploaded in this visit</h2>
            <div class="col-md-12" aria-live="polite">
                <table class="table table-sm table-striped table-bordered">
                    <thead><tr><th scope="col">File Name</th><th scope="col">Status</th><th scope="col">Message</th><th scope="col">ID</th></tr></thead>
                    <tbody>
                        <tr v-for="(h, i) in history" :key="i">
                            <td>{{ h.name }}</td>
                            <td><span class="badge" :class="h.status === 'Rejected' ? 'text-bg-danger' : 'text-bg-warning'">{{ h.status }}</span></td>
                            <td><ul class="mb-0 ps-3"><li v-for="(m, k) in h.messages" :key="k">{{ m }}</li></ul></td>
                            <td>{{ h.id }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
