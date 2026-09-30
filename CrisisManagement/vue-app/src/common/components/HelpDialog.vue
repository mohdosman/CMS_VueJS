<script setup>
import { ref, onMounted } from 'vue';
import { publicFilesApi } from '../api/publicFilesApi.js';
import { apiErrorMessage } from '../../utils/apiError.js';

const emit = defineEmits(['close']);

const files = ref([]);
const error = ref('');
const isLoading = ref(true);

const size = (b) => (b >= 1048576 ? `${(b / 1048576).toFixed(2)} MB` : b >= 1024 ? `${Math.round(b / 1024)} KB` : `${b} Bytes`);
const date = (v) => new Date(v).toLocaleDateString();

onMounted(async () => {
    try {
        files.value = await publicFilesApi.helpFiles();
    } catch (e) {
        error.value = apiErrorMessage(e);
    } finally {
        isLoading.value = false;
    }
});
</script>

<template>
    <AppDialog title="Help - Public Files" @close="emit('close')">
        <div v-if="error" class="alert alert-danger" role="alert">{{ error }}</div>
        <p v-else-if="isLoading" role="status">Loading...</p>
        <p v-else-if="!files.length" role="status">No files available.</p>
        <table v-else class="table table-sm table-striped table-bordered">
            <thead>
                <tr><th scope="col">File Name</th><th scope="col">Date Uploaded</th><th scope="col">File Size</th></tr>
            </thead>
            <tbody>
                <tr v-for="f in files" :key="f.id">
                    <td><a :href="publicFilesApi.downloadUrl(f.id)" download>{{ f.fileName }}</a></td>
                    <td>{{ date(f.createdOn) }}</td>
                    <td>{{ size(f.fileSize) }}</td>
                </tr>
            </tbody>
        </table>
        <AppButton action="close" @click="emit('close')" />
    </AppDialog>
</template>
