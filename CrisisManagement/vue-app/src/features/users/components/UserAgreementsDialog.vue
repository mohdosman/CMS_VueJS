<script setup>
import { ref, onMounted } from 'vue';
import { usersApi } from '../api/usersApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { formatDate, formatFileSize } from '../../../utils/formatters.js';

// Lists a user's agreements with download and (for users.edit) delete. Closes with the count it ended on.
const props = defineProps({
    userKey: { type: String, required: true },
    canEdit: { type: Boolean, default: false }
});
const emit = defineEmits(['close']);
const { logSuccess, logApiError } = useLogger();

const docs = ref([]);
const error = ref('');
const isLoading = ref(true);
const confirming = ref(0);   // id of the document awaiting delete confirmation


async function load() {
    try {
        docs.value = await usersApi.documents(props.userKey);
    } catch (e) {
        error.value = apiErrorMessage(e);
    } finally {
        isLoading.value = false;
    }
}

async function remove(d) {
    try {
        await usersApi.removeDocument(props.userKey, d.documentId);
        logSuccess('User agreement deleted.');
        confirming.value = 0;
        await load();
    } catch (e) {
        confirming.value = 0;
        logApiError(e);
    }
}

onMounted(load);
</script>

<template>
    <AppDialog title="View User Agreement" @close="emit('close', docs.length)">
        <div v-if="error" class="alert alert-danger" role="alert">{{ error }}</div>
        <p v-else-if="isLoading" role="status">Loading...</p>
        <p v-else-if="!docs.length" role="status">No user agreements uploaded for this user.</p>
        <table v-else class="table table-sm table-striped table-bordered">
            <thead>
                <tr>
                    <th scope="col">File Name</th><th scope="col">Date Uploaded</th><th scope="col">File Size</th>
                    <th scope="col">Action</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="d in docs" :key="d.documentId">
                    <td>{{ d.fileName }}</td>
                    <td>{{ formatDate(d.createdOn) }}</td>
                    <td>{{ formatFileSize(d.fileSize) }}</td>
                    <td class="text-nowrap">
                        <a class="btn btn-outline-secondary btn-sm" :href="usersApi.documentUrl(userKey, d.documentId)" download>
                            Download<span class="visually-hidden"> {{ d.fileName }}</span>
                        </a>
                        <template v-if="canEdit">
                            <AppButton v-if="confirming !== d.documentId" action="cancel" @click="confirming = d.documentId">
                                Delete<span class="visually-hidden"> {{ d.fileName }}</span>
                            </AppButton>
                            <template v-else>
                                <AppButton action="delete" @click="remove(d)">Confirm delete</AppButton>
                                <AppButton action="cancel" @click="confirming = 0" />
                            </template>
                        </template>
                    </td>
                </tr>
            </tbody>
        </table>
        <AppButton action="close" @click="emit('close', docs.length)" />
    </AppDialog>
</template>
