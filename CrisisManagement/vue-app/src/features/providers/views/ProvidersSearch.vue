<script setup>
import { useProviderSearch } from '../composables/useProviderSearch.js';

const {
    criteria, paging, providers, totalRecords, hasSearched, isSearching,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoProvider, canAdd, add
} = useProviderSearch();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Search Provider</h1>

        <SearchPanel title="Search Provider" icon="fa fa-hospital-o" form-name="providerForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="name">Provider</label>
                        <input id="name" v-model="criteria.name" type="text" maxlength="250" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="edisonNumber">Edison Number</label>
                        <input id="edisonNumber" v-model="criteria.edisonNumber" type="text" maxlength="50" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="npi">NPI</label>
                        <input id="npi" v-model="criteria.npi" type="text" maxlength="25" class="form-control form-control-sm" />
                    </div>
                </div>
            </template>
            <template #buttons>
                <AppButton action="search" :disabled="isSearching" />
                <AppButton v-if="canAdd && hasSearched && !totalRecords" action="add" @click="add" />
                <AppButton action="clear" @click="clear" />
            </template>
        </SearchPanel>

        <div class="row" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">Provider Results Grid</h2>
            <div class="col-md-12">
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="name" :sort-icon="sortIcon" @sort="setOrder">Provider Name</SortHeader>
                            <SortHeader col="abbreviation" :sort-icon="sortIcon" @sort="setOrder">Abbreviation</SortHeader>
                            <SortHeader col="edisonNumber" :sort-icon="sortIcon" @sort="setOrder">Edison Number</SortHeader>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!providers.length" class="msg-error">
                            <td colspan="3"><div class="text-center"><strong>No Records Found.</strong></div></td>
                        </tr>
                        <tr v-for="p in providers" :key="p.providerId" style="cursor:pointer" @click="gotoProvider(p)">
                            <td><router-link :to="`/admin/providers/${p.providerId}`" @click.stop>{{ p.name }}</router-link></td>
                            <td>{{ p.abbreviation }}</td>
                            <td>{{ p.edisonNumber }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Provider Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>
    </div>
</template>
