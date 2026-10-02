<script setup>
import { useRoleSearch } from '../composables/useRoleSearch.js';

const {
    criteria, paging, roles, totalRecords, isSearching,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoRole, canAdd, add
} = useRoleSearch();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Search Roles</h1>

        <SearchPanel title="Search Roles" icon="fa fa-shield" form-name="roleForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="name">Role name</label>
                        <input id="name" v-model="criteria.name" type="text" maxlength="256" class="form-control form-control-sm" />
                    </div>
                </div>
            </template>
            <template #buttons>
                <AppButton action="search" :disabled="isSearching" />
                <AppButton v-if="canAdd" action="add" @click="add" />
                <AppButton action="clear" @click="clear" />
            </template>
        </SearchPanel>

        <div class="row" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">Role Results Grid</h2>
            <div class="col-md-12">
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="id" :sort-icon="sortIcon" @sort="setOrder">Id</SortHeader>
                            <SortHeader col="name" :sort-icon="sortIcon" @sort="setOrder">Name</SortHeader>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!roles.length" class="msg-error">
                            <td colspan="2"><div class="text-center"><strong>No Roles Found</strong></div></td>
                        </tr>
                        <tr v-for="r in roles" :key="r.id" style="cursor:pointer" @click="gotoRole(r)">
                            <td>{{ r.id }}</td>
                            <td><router-link :to="`/admin/roles/${r.id}`" @click.stop>{{ r.name }}</router-link></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Role Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>
    </div>
</template>
