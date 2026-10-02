<script setup>
import { useUserSearch } from '../composables/useUserSearch.js';
import MultiSelectDropdown from '../../../common/components/MultiSelectDropdown.vue';
import RoleChips from '../../../common/components/RoleChips.vue';

const {
    criteria, paging, users, totalRecords, roles, providers, yesNo, flags, isSearching,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoUser, canAdd, add
} = useUserSearch();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Search Users</h1>

        <SearchPanel title="Search Users" icon="fa fa-users" form-name="userForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="userName">User ID</label>
                        <input id="userName" v-model="criteria.userName" type="text" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="firstName">First name</label>
                        <input id="firstName" v-model="criteria.firstName" type="text" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="lastName">Last name</label>
                        <input id="lastName" v-model="criteria.lastName" type="text" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="email">Email</label>
                        <input id="email" v-model="criteria.email" type="text" class="form-control form-control-sm" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6 mb-3">
                        <MultiSelectDropdown id="providerIds" v-model="criteria.providerIds" label="Provider" :options="providers" />
                    </div>
                    <div v-for="f in flags" :key="f.key" class="col-md-2 mb-3">
                        <fieldset>
                            <legend class="form-label">{{ f.label }}</legend>
                            <label v-for="o in yesNo" :key="o.v" class="radio-inline">
                                <input v-model="criteria[f.key]" type="radio" :name="f.key" :value="o.v" /> {{ o.t }}
                            </label>
                        </fieldset>
                    </div>
                </div>

                <RoleChips v-model="criteria.roleIds" class="mb-3" label="Roles - click to filter"
                           :options="roles" :hint="criteria.roleIds.length ? `${criteria.roleIds.length} role(s) selected` : 'No roles selected - showing all users'" />
            </template>
            <template #buttons>
                <AppButton action="search" :disabled="isSearching" />
                <AppButton v-if="canAdd" action="add" @click="add" />
                <AppButton action="clear" @click="clear" />
            </template>
        </SearchPanel>

        <div class="row" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">User Results Grid</h2>
            <div class="col-md-12">
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="userName" :sort-icon="sortIcon" @sort="setOrder">User ID</SortHeader>
                            <SortHeader col="firstName" :sort-icon="sortIcon" @sort="setOrder">First Name</SortHeader>
                            <SortHeader col="lastName" :sort-icon="sortIcon" @sort="setOrder">Last Name</SortHeader>
                            <SortHeader col="email" :sort-icon="sortIcon" @sort="setOrder">Email</SortHeader>
                            <SortHeader col="isADAccount" :sort-icon="sortIcon" @sort="setOrder">Is AD</SortHeader>
                            <SortHeader col="isLockedOut" :sort-icon="sortIcon" @sort="setOrder">Locked out</SortHeader>
                            <SortHeader col="isEnabled" :sort-icon="sortIcon" @sort="setOrder">Active</SortHeader>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!users.length" class="msg-error">
                            <td colspan="7"><div class="text-center"><strong>No Users Found</strong></div></td>
                        </tr>
                        <tr v-for="u in users" :key="u.userKey" style="cursor:pointer" @click="gotoUser(u)">
                            <td><router-link :to="`/admin/users/${u.userKey}`" @click.stop>{{ u.userName }}</router-link></td>
                            <td>{{ u.firstName }}</td>
                            <td>{{ u.lastName }}</td>
                            <td>{{ u.email }}</td>
                            <td>{{ u.isADAccount ? 'Yes' : 'No' }}</td>
                            <td>{{ u.isLockedOut ? 'Yes' : 'No' }}</td>
                            <td>{{ u.isEnabled ? 'Yes' : 'No' }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">User Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>
    </div>
</template>
