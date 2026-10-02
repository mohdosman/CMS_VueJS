<script setup>
import { useSuicideFileUpload } from '../composables/useSuicideFileUpload.js';

const { history, isUploading, pickError, onPick } = useSuicideFileUpload();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Suicide File Upload</h1>

        <div class="row">
            <div class="col-sm-12">
                <div class="card">
                    <div class="card-header" role="heading" aria-level="2"><i class="fa fa-upload" aria-hidden="true"></i> Upload File</div>
                    <div class="card-body">
                        <div class="row justify-content-center">
                            <div class="col-md-6" :class="{ 'has-error': !!(pickError) }">
                                <label class="form-label" for="suicideFile">Select file</label>
                                <input id="suicideFile" type="file" accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" class="form-control form-control-sm"
                                       :disabled="isUploading" :aria-invalid="!!pickError" aria-describedby="suicideFile-err" @change="onPick" />
                                <div id="suicideFile-err" class="form-text has-error" role="alert">{{ pickError }}</div>
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
