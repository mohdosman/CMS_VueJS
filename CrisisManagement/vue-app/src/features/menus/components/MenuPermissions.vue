<script setup>
import { useMenuPermissions } from '../composables/useMenuPermissions.js';

const props = defineProps({
    menuId: { type: Number, required: true },
    canEdit: { type: Boolean, default: false }
});

const {
    groups, filter, shown, dialog, editing, perm, group, renaming, confirming, isLoading, isSaving, errors, err,
    openPermission, savePermission, removePermission, openNewGroup, saveNewGroup, startRename, saveRename, close,
    announce
} = useMenuPermissions(props);
</script>

<template>
    <div>
        <p v-if="isLoading" role="status">Loading...</p>
        <template v-else>
            <div class="d-flex flex-wrap align-items-end gap-3 mb-3">
                <div style="max-width: 24rem" class="flex-grow-1">
                    <label class="form-label" for="permFilter">Search permissions</label>
                    <input id="permFilter" v-model="filter" type="text" class="form-control form-control-sm" aria-describedby="permFilter-help"
                           @input="announce('Filtered')" />
                    <div id="permFilter-help" class="form-text">By name or value.</div>
                </div>
                <div>
                    <AppButton action="cancel" @click="dialog = 'groups'">Permission groups</AppButton>
                    <AppButton v-if="canEdit" action="cancel" @click="openNewGroup">New group</AppButton>
                    <AppButton v-if="canEdit" action="add" @click="openPermission()">New permission</AppButton>
                </div>
            </div>

            <p v-if="!shown.length" class="text-muted">No permissions yet for this menu item.</p>
            <table v-else class="table table-sm table-bordered">
                <thead>
                    <tr><th scope="col">Name</th><th scope="col">Value</th><th scope="col">Description</th><th v-if="canEdit" scope="col">Action</th></tr>
                </thead>
                <template v-for="g in shown" :key="g.name">
                    <tbody>
                        <tr class="table-active"><th :colspan="canEdit ? 4 : 3" scope="colgroup">{{ g.name }}</th></tr>
                        <tr v-for="r in g.items" :key="r.id">
                            <td>{{ r.name }}</td>
                            <td><code>{{ r.value }}</code></td>
                            <td>{{ r.description }}</td>
                            <td v-if="canEdit" class="text-nowrap">
                                <AppButton action="cancel" size="xs" @click="openPermission(r)">Edit<span class="visually-hidden"> {{ r.value }}</span></AppButton>
                                <AppButton action="cancel" size="xs" @click="confirming = r; dialog = 'delete'">Delete<span class="visually-hidden"> {{ r.value }}</span></AppButton>
                            </td>
                        </tr>
                    </tbody>
                </template>
            </table>
        </template>

        <!-- Add / edit a permission -->
        <AppDialog v-if="dialog === 'permission'" :title="editing ? 'Edit permission' : 'New permission'" @close="close">
            <div class="mb-3" :class="{ 'has-error': !!(err('groupId')) }">
                <label class="form-label" for="p-group">Group <span class="f_req" aria-hidden="true">*</span></label>
                <select id="p-group" v-model="perm.groupId" class="form-select form-select-sm" :aria-invalid="!!err('groupId')" aria-describedby="p-group-err">
                    <option v-for="g in groups" :key="g.id" :value="g.id">{{ g.name }}</option>
                </select>
                <div id="p-group-err" class="form-text has-error" role="alert">{{ err('groupId') }}</div>
            </div>
            <div class="mb-3" :class="{ 'has-error': !!(err('name')) }">
                <label class="form-label" for="p-name">Name <span class="f_req" aria-hidden="true">*</span></label>
                <input id="p-name" v-model="perm.name" type="text" maxlength="250" class="form-control form-control-sm" :aria-invalid="!!err('name')"
                       aria-describedby="p-name-err" @keydown.enter.prevent="savePermission" />
                <div id="p-name-err" class="form-text has-error" role="alert">{{ err('name') }}</div>
            </div>
            <div class="mb-3" :class="{ 'has-error': !!(err('value')) }">
                <label class="form-label" for="p-value">Value <span class="f_req" aria-hidden="true">*</span></label>
                <input id="p-value" v-model="perm.value" type="text" maxlength="250" class="form-control form-control-sm" :aria-invalid="!!err('value')"
                       aria-describedby="p-value-help p-value-err" @keydown.enter.prevent="savePermission" />
                <div id="p-value-help" class="form-text">Like <code>users.view</code>. Unique across the application, no spaces. Renaming it updates the roles that hold it.</div>
                <div id="p-value-err" class="form-text has-error" role="alert">{{ err('value') }}</div>
            </div>
            <div class="mb-3" :class="{ 'has-error': !!(err('description')) }">
                <label class="form-label" for="p-desc">Description <span class="f_req" aria-hidden="true">*</span></label>
                <textarea id="p-desc" v-model="perm.description" rows="2" maxlength="250" class="form-control form-control-sm" :aria-invalid="!!err('description')"
                          aria-describedby="p-desc-err"></textarea>
                <div id="p-desc-err" class="form-text has-error" role="alert">{{ err('description') }}</div>
            </div>
            <div v-if="err('form')" class="alert alert-danger" role="alert">{{ err('form') }}</div>
            <AppButton action="run" :disabled="isSaving" @click="savePermission">Save</AppButton>
            <AppButton action="cancel" @click="close" />
        </AppDialog>

        <!-- Delete a permission -->
        <AppDialog v-if="dialog === 'delete'" title="Delete permission" @close="close">
            <p>Delete <code>{{ confirming.value }}</code>? Roles that hold it lose it. This cannot be undone.</p>
            <AppButton action="delete" :disabled="isSaving" @click="removePermission">Delete permission</AppButton>
            <AppButton action="cancel" @click="close" />
        </AppDialog>

        <!-- New permission group -->
        <AppDialog v-if="dialog === 'group'" title="New permission group" @close="close">
            <div class="mb-3" :class="{ 'has-error': !!(err('name')) }">
                <label class="form-label" for="g-name">Group name <span class="f_req" aria-hidden="true">*</span></label>
                <input id="g-name" v-model="group.name" type="text" maxlength="100" class="form-control form-control-sm" :aria-invalid="!!err('name')"
                       aria-describedby="g-name-err" @keydown.enter.prevent="saveNewGroup" />
                <div id="g-name-err" class="form-text has-error" role="alert">{{ err('name') }}</div>
            </div>
            <div class="mb-3" :class="{ 'has-error': !!(err('description')) }">
                <label class="form-label" for="g-desc">Description</label>
                <input id="g-desc" v-model="group.description" type="text" maxlength="250" class="form-control form-control-sm"
                       :aria-invalid="!!err('description')" aria-describedby="g-desc-err" @keydown.enter.prevent="saveNewGroup" />
                <div id="g-desc-err" class="form-text has-error" role="alert">{{ err('description') }}</div>
            </div>
            <AppButton action="run" :disabled="isSaving" @click="saveNewGroup">Save</AppButton>
            <AppButton action="cancel" @click="close" />
        </AppDialog>

        <!-- Permission groups: list and rename -->
        <AppDialog v-if="dialog === 'groups'" title="Permission groups" @close="close">
            <table class="table table-sm table-bordered">
                <thead><tr><th scope="col">Group</th><th scope="col">Description</th><th scope="col">Permissions</th><th v-if="canEdit" scope="col">Action</th></tr></thead>
                <tbody>
                    <tr v-for="g in groups" :key="g.id">
                        <template v-if="renaming === g.id">
                            <td>
                                <label class="visually-hidden" :for="`gn-${g.id}`">Group name</label>
                                <input :id="`gn-${g.id}`" v-model="group.name" type="text" maxlength="100" class="form-control form-control-sm" :aria-invalid="!!err('name')"
                                       @keydown.enter.prevent="saveRename" />
                                <div class="form-text has-error" role="alert">{{ err('name') }}</div>
                            </td>
                            <td>
                                <label class="visually-hidden" :for="`gd-${g.id}`">Description</label>
                                <input :id="`gd-${g.id}`" v-model="group.description" type="text" maxlength="250" class="form-control form-control-sm"
                                       @keydown.enter.prevent="saveRename" />
                                <div class="form-text has-error" role="alert">{{ err('description') }}</div>
                            </td>
                            <td>{{ g.permissionCount }}</td>
                            <td class="text-nowrap">
                                <AppButton action="run" size="xs" :disabled="isSaving" @click="saveRename">Save</AppButton>
                                <AppButton action="cancel" size="xs" @click="renaming = 0; errors = {}" />
                            </td>
                        </template>
                        <template v-else>
                            <td>{{ g.name }}</td>
                            <td>{{ g.description }}</td>
                            <td>{{ g.permissionCount }}</td>
                            <td v-if="canEdit"><AppButton action="cancel" size="xs" @click="startRename(g)">Edit<span class="visually-hidden"> {{ g.name }}</span></AppButton></td>
                        </template>
                    </tr>
                </tbody>
            </table>
            <AppButton action="close" @click="close" />
        </AppDialog>
    </div>
</template>
