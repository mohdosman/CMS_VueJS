<script setup>
import { useAssessmentUpload } from '../composables/useAssessmentUpload.js';

const { history, isUploading, pickError, onPick } = useAssessmentUpload();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Assessment File Upload</h1>

        <div class="row">
            <div class="col-sm-12">
                <div class="card">
                    <div class="card-header" role="heading" aria-level="2"><i class="fa fa-upload" aria-hidden="true"></i> Upload Assessment File</div>
                    <div class="card-body">
                        <div class="row justify-content-center">
                            <div class="col-md-6" :class="{ 'has-error': !!(pickError) }">
                                <label class="form-label" for="assessmentFile">Choose XML file</label>
                                <input id="assessmentFile" type="file" accept=".xml,text/xml,application/xml" class="form-control form-control-sm"
                                       :disabled="isUploading" :aria-invalid="!!pickError" aria-describedby="assessmentFile-help assessmentFile-err" @change="onPick" />
                                <div id="assessmentFile-help" class="form-text">
                                    Upload an XML assessment file for processing. Maximum file size: 5 MB. The file uploads as soon as you choose it.
                                </div>
                                <div id="assessmentFile-err" class="form-text has-error" role="alert">{{ pickError }}</div>
                                <p v-if="isUploading" role="status" class="mb-0">Uploading...</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="row mt-2">
            <div class="col-md-12">
                <div class="alert alert-warning" role="note">
                    <h2 class="h6 text-danger fw-bold text-center">ATTENTION</h2>
                    <p>As of 05/03/2018, uploaded files are processed only at night between the hours of 6pm and 6am. So you will not see uploaded file information using 'Display Files' until late evening or the day following upload. PLEASE DO NOT UPLOAD THE SAME FILE MULTIPLE TIMES.</p>
                    <p class="mb-0">If you have issues with file upload, please contact Gina Young at <a href="mailto:gina.young@tn.gov">gina.young@tn.gov</a> 615/532-6675.</p>
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
                            <td>{{ h.message }}</td>
                            <td>{{ h.id }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
