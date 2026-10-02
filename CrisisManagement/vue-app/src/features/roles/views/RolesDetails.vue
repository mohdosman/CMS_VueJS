<script setup>
import { useRoleDetail } from '../composables/useRoleDetail.js';
import PermissionPicker from '../components/PermissionPicker.vue';

const {
    isNew, canEdit, title, form, groups, dialog, isLoading, loadFailed, isSaving, touch, msg,
    isAdminRole, hasChanges, onSubmit, remove, cancel
} = useRoleDetail();
</script>

<template>
    <DetailPanel :title="title" icon="fa fa-shield" form-name="roleForm"
                 main-labelledby="main-title" :can-save="canEdit && !isSaving && !isLoading && !isAdminRole && !loadFailed"
                 :show-buttons="canEdit" @save="onSubmit" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ title }}</h1>

            <div v-if="loadFailed" class="alert alert-danger" role="alert">
                This role could not be loaded, so it cannot be edited. Go back to the search and try again.
            </div>

            <div v-if="isAdminRole" class="alert alert-info" role="status">
                The Administrator role is built in. It has every permission and cannot be changed.
            </div>

            <!-- A read-only viewer (roles.view only) gets the same form with every input disabled; the
                 permission picker stays searchable and expandable, only its boxes are disabled. -->
            <fieldset :disabled="!canEdit || isLoading || isAdminRole || loadFailed" class="border-0 p-0 m-0">
                <div class="row">
                    <div class="col-md-5 mb-3" :class="{ 'has-error': !!msg('name') }">
                        <label class="form-label" for="name">Role name <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="name" v-model="form.name" type="text" class="form-control form-control-sm"
                               :aria-invalid="!!msg('name')" aria-describedby="name-err" @blur="touch('name')" />
                        <div id="name-err" class="form-text has-error" role="alert">{{ msg('name') }}</div>
                    </div>
                </div>
            </fieldset>

            <div class="d-flex align-items-center gap-3 mb-2">
                <h2 class="h6 mb-0">Add/Remove Permissions</h2>
                <span v-if="hasChanges && canEdit" class="badge text-bg-warning" role="status">Unsaved changes</span>
            </div>
            <div id="permissions-err" class="form-text has-error mb-2" role="alert">{{ msg('permissions') }}</div>

            <PermissionPicker v-model="form.permissions" :groups="groups" :disabled="!canEdit || isAdminRole || isLoading || loadFailed" />
        </template>

        <template #actions>
            <AppButton v-if="!isNew && canEdit && !isAdminRole && !loadFailed" action="delete" @click="dialog = 'delete'">Delete</AppButton>
        </template>

        <template v-if="!canEdit" #button-row>
            <AppButton action="close" @click="cancel">Back to search</AppButton>
        </template>

        <template #below>
            <AppDialog v-if="dialog === 'delete'" title="Delete role" @close="dialog = ''">
                <p>
                    Delete <strong>{{ form.name }}</strong>? This removes the role and its permissions.
                    A role that users still hold cannot be deleted. It cannot be undone.
                </p>
                <AppButton action="delete" @click="remove">Delete role</AppButton>
                <AppButton action="cancel" @click="dialog = ''" />
            </AppDialog>
        </template>
    </DetailPanel>
</template>
