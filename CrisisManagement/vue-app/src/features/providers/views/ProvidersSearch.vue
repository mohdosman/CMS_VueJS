<script setup>
import { useProviderSearch } from '../composables/useProviderSearch.js';

const {
    criteria, paging, providers, totalRecords, isSearching,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoProvider, canAdd, add
} = useProviderSearch();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Search Providers</h1>

        <SearchPanel title="Search Providers" icon="fa fa-hospital-o" form-name="providerForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="name">Name</label>
                        <input id="name" v-model="criteria.name" type="text" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="abbreviation">Abbreviation</label>
                        <input id="abbreviation" v-model="criteria.abbreviation" type="text" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="edisonNumber">Edison #</label>
                        <input id="edisonNumber" v-model="criteria.edisonNumber" type="text" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="npi">NPI</label>
                        <input id="npi" v-model="criteria.npi" type="text" class="form-control form-control-sm" />
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
            <h2 id="results-heading" class="visually-hidden">Provider Results Grid</h2>
            <div class="col-md-12">
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="name" :sort-icon="sortIcon" @sort="setOrder">Name</SortHeader>
                            <SortHeader col="abbreviation" :sort-icon="sortIcon" @sort="setOrder">Abbrev</SortHeader>
                            <SortHeader col="edisonNumber" :sort-icon="sortIcon" @sort="setOrder">Edison #</SortHeader>
                            <SortHeader col="npi" :sort-icon="sortIcon" @sort="setOrder">NPI</SortHeader>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!providers.length" class="msg-error">
                            <td colspan="4"><div class="text-center"><strong>No Providers Found</strong></div></td>
                        </tr>
                        <tr v-for="p in providers" :key="p.providerId" style="cursor:pointer" @click="gotoProvider(p)">
                            <td><router-link :to="`/admin/providers/${p.providerId}`" @click.stop>{{ p.name }}</router-link></td>
                            <td>{{ p.abbreviation }}</td>
                            <td>{{ p.edisonNumber }}</td>
                            <td>{{ p.npi }}</td>
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
