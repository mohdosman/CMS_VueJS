<script setup>
import { useNotificationSearch } from '../composables/useNotificationSearch.js';
import { formatDateTime } from '../../../utils/formatters.js';

const {
    paging, notifications, totalRecords, isSearching, confirming,
    search, remove, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoNotification, canEdit, add
} = useNotificationSearch();

</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Search Notifications</h1>

        <SearchPanel title="Search Notifications" icon="fa fa-bell" form-name="notificationForm" @submit="search">
            <template #fields>
                <p class="text-muted">The newest notification is shown as "Attention" on the sign-in page.</p>
            </template>
            <template #buttons>
                <AppButton action="search" :disabled="isSearching">Refresh</AppButton>
                <AppButton v-if="canEdit" action="add" @click="add" />
            </template>
        </SearchPanel>

        <div class="row" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">Notification Results Grid</h2>
            <div class="col-md-12">
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="id" :sort-icon="sortIcon" @sort="setOrder">Id</SortHeader>
                            <SortHeader col="notification" :sort-icon="sortIcon" @sort="setOrder">Notification</SortHeader>
                            <SortHeader col="createdOn" :sort-icon="sortIcon" @sort="setOrder">Created On</SortHeader>
                            <th v-if="canEdit" scope="col">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!notifications.length" class="msg-error">
                            <td :colspan="canEdit ? 4 : 3"><div class="text-center"><strong>No Notifications Found</strong></div></td>
                        </tr>
                        <tr v-for="n in notifications" :key="n.id" style="cursor:pointer" @click="gotoNotification(n)">
                            <td><router-link :to="`/notifications/${n.id}`" @click.stop>{{ n.id }}</router-link></td>
                            <td>{{ n.notification }}</td>
                            <td>{{ formatDateTime(n.createdOn) }}</td>
                            <td v-if="canEdit" @click.stop>
                                <AppButton action="cancel" @click="confirming = n">Delete<span class="visually-hidden"> notification {{ n.id }}</span></AppButton>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Notification Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>

        <AppDialog v-if="confirming" title="Delete notification" @close="confirming = null">
            <p>Delete notification {{ confirming.id }}? It stops showing on the sign-in page. This cannot be undone.</p>
            <AppButton action="delete" @click="remove">Delete notification</AppButton>
            <AppButton action="cancel" @click="confirming = null" />
        </AppDialog>
    </div>
</template>
