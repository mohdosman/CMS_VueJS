<script setup>
import { ref } from 'vue';
import { suicidesApi } from '../api/suicidesApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Upload one death record spreadsheet (.xlsx). The server reads the first worksheet (row 1 = column names) and stores the
// file with its records; this page keeps a history of what was uploaded in this visit.
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
    if (!file.name.toLowerCase().endsWith('.xlsx')) { pickError.value = 'Only .xlsx files are accepted.'; return; }
    if (file.size > MAX_BYTES) { pickError.value = `The file exceeds the ${MAX_BYTES / (1024 * 1024)} MB limit.`; return; }

    isUploading.value = true;
    try {
        const r = await suicidesApi.uploadFile(file);
        history.value.unshift({ name: file.name, status: 'Uploaded', messages: [`${r.recordCount} records stored.`], id: r.id });
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
        <h1 id="main-title" class="visually-hidden">Suicide File Upload</h1>

        <div class="row">
            <div class="col-sm-12">
                <div class="card">
                    <div class="card-header" role="heading" aria-level="2"><i class="fa fa-upload" aria-hidden="true"></i> Upload Suicide File</div>
                    <div class="card-body">
                        <div class="row justify-content-center">
                            <div class="col-md-6">
                                <label class="form-label" for="suicideFile">Choose Excel file</label>
                                <input id="suicideFile" type="file" accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" class="form-control form-control-sm"
                                       :disabled="isUploading" :aria-invalid="!!pickError" aria-describedby="suicideFile-help suicideFile-err" @change="onPick" />
                                <div id="suicideFile-help" class="form-text">
                                    Upload an .xlsx file with the column names in the first row of the first worksheet. Maximum file size: 5 MB.
                                    The file uploads as soon as you choose it.
                                </div>
                                <div id="suicideFile-err" class="form-text has-error" role="alert">{{ pickError }}</div>
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
                            <td><span class="badge" :class="h.status === 'Rejected' ? 'text-bg-danger' : 'text-bg-success'">{{ h.status }}</span></td>
                            <td><ul class="mb-0 ps-3"><li v-for="(m, k) in h.messages" :key="k">{{ m }}</li></ul></td>
                            <td>{{ h.id }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
