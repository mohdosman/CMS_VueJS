<script setup>
import { useHelpDialog } from '../composables/useHelpDialog.js';

const emit = defineEmits(['close']);

const { files, isLoading, size, date, downloadUrl } = useHelpDialog();
</script>

<template>
    <AppDialog title="Help - Public Files" @close="emit('close')">
        <p v-if="isLoading" role="status">Loading...</p>
        <p v-else-if="!files.length" role="status">No files available.</p>
        <table v-else class="table table-sm table-striped table-bordered">
            <thead>
                <tr><th scope="col">File Name</th><th scope="col">Date Uploaded</th><th scope="col">File Size</th></tr>
            </thead>
            <tbody>
                <tr v-for="f in files" :key="f.id">
                    <td><a :href="downloadUrl(f.id)" download>{{ f.fileName }}</a></td>
                    <td>{{ date(f.createdOn) }}</td>
                    <td>{{ size(f.fileSize) }}</td>
                </tr>
            </tbody>
        </table>
        <AppButton action="close" @click="emit('close')" />
    </AppDialog>
</template>
