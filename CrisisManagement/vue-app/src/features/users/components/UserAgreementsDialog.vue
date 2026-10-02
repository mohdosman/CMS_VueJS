<script setup>
import { useUserAgreements } from '../composables/useUserAgreements.js';

// Closes with the count it ended on.
const props = defineProps({
    userKey: { type: String, required: true },
    canEdit: { type: Boolean, default: false }
});
const emit = defineEmits(['close']);

const { docs, isLoading, confirming, remove, formatDate, formatFileSize, documentUrl } = useUserAgreements(props);
</script>

<template>
    <AppDialog title="View User Agreement" @close="emit('close', docs.length)">
        <p v-if="isLoading" role="status">Loading...</p>
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
                        <a class="btn btn-outline-secondary btn-sm" :href="documentUrl(userKey, d.documentId)" download>
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
