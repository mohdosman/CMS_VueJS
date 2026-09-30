<script setup>
import { usePublicFiles } from '../composables/usePublicFiles.js';
import { publicFilesApi } from '../../../common/api/publicFilesApi.js';
import AppDialog from '../../../common/components/AppDialog.vue';

const {
    paging, files, totalRecords, canEdit, uploadError, isUploading, confirming,
    setOrder, sortIcon, onPageChanged, onPageSizeChanged, onPick, remove
} = usePublicFiles();

const size = (b) => (b >= 1048576 ? `${(b / 1048576).toFixed(2)} MB` : b >= 1024 ? `${Math.round(b / 1024)} KB` : `${b} Bytes`);
const dateTime = (v) => new Date(v).toLocaleString('en-US');
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Public Files</h1>

        <!-- Upload is for publicfiles.edit; a viewer only sees the list. -->
        <div v-if="canEdit" class="row">
            <div class="col-sm-12">
                <div class="card">
                    <div class="card-header" role="heading" aria-level="2"><i class="fa fa-upload" aria-hidden="true"></i> Upload Public File</div>
                    <div class="card-body">
                        <div class="row justify-content-center">
                            <div class="col-md-6">
                                <label class="form-label" for="publicFile">Choose PDF file</label>
                                <input id="publicFile" type="file" accept=".pdf,application/pdf" class="form-control form-control-sm"
                                       :disabled="isUploading" :aria-invalid="!!uploadError" aria-describedby="publicFile-help publicFile-err" @change="onPick" />
                                <div id="publicFile-help" class="form-text">PDF only, up to 5 MB. The file uploads as soon as you choose it and appears in the Help dialog.</div>
                                <div id="publicFile-err" class="form-text has-error" role="alert">{{ uploadError }}</div>
                                <p v-if="isUploading" role="status" class="mb-0">Uploading...</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="row mt-2" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">Public Files Results Grid</h2>
            <div class="col-md-12">
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="id" :sort-icon="sortIcon" @sort="setOrder">Id</SortHeader>
                            <SortHeader col="fileName" :sort-icon="sortIcon" @sort="setOrder">File Name</SortHeader>
                            <SortHeader col="createdOn" :sort-icon="sortIcon" @sort="setOrder">Date Uploaded</SortHeader>
                            <SortHeader col="fileSize" :sort-icon="sortIcon" @sort="setOrder">File Size</SortHeader>
                            <th scope="col">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!files.length" class="msg-error">
                            <td colspan="5"><div class="text-center"><strong>No Public Files Found</strong></div></td>
                        </tr>
                        <tr v-for="f in files" :key="f.id">
                            <td>{{ f.id }}</td>
                            <td>{{ f.fileName }}</td>
                            <td>{{ dateTime(f.createdOn) }}</td>
                            <td>{{ size(f.fileSize) }}</td>
                            <td class="text-nowrap">
                                <a class="btn btn-outline-secondary btn-sm" :href="publicFilesApi.downloadUrl(f.id)" download>
                                    Download<span class="visually-hidden"> {{ f.fileName }}</span>
                                </a>
                                <AppButton v-if="canEdit" action="cancel" @click="confirming = f">Delete<span class="visually-hidden"> {{ f.fileName }}</span></AppButton>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Public Files Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>

        <AppDialog v-if="confirming" title="Delete public file" @close="confirming = null">
            <p>Delete <strong>{{ confirming.fileName }}</strong>? It disappears from the Help dialog. This cannot be undone.</p>
            <AppButton action="delete" @click="remove">Delete file</AppButton>
            <AppButton action="cancel" @click="confirming = null" />
        </AppDialog>
    </div>
</template>
