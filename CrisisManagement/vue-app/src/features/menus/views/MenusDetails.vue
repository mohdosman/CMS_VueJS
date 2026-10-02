<script setup>
import { useMenuDetail } from '../composables/useMenuDetail.js';
import MenuPermissions from '../components/MenuPermissions.vue';

const {
    isNew, canEdit, title, form, menuId, parents, tab, urlFields, errors, isLoading, isSaving, msg, err,
    save, cancel, iconOptions
} = useMenuDetail();
</script>

<template>
    <DetailPanel :title="title" icon="fa fa-bars" form-name="menuForm"
                 main-labelledby="main-title" :can-save="canEdit && !isSaving && !isLoading && tab === 'details'"
                 :show-buttons="canEdit && tab === 'details'" @save="save" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ title }}</h1>

            <!-- Permissions belong to a saved menu item, so a new one only has the Details tab until it is saved. -->
            <ul class="nav nav-tabs mb-3" role="tablist">
                <li class="nav-item" role="presentation">
                    <button id="tab-details" type="button" class="nav-link" :class="{ active: tab === 'details' }" role="tab"
                            :aria-selected="tab === 'details'" aria-controls="panel-details" @click="tab = 'details'">Details</button>
                </li>
                <li v-if="!isNew" class="nav-item" role="presentation">
                    <button id="tab-permissions" type="button" class="nav-link" :class="{ active: tab === 'permissions' }" role="tab"
                            :aria-selected="tab === 'permissions'" aria-controls="panel-permissions" @click="tab = 'permissions'">Permissions</button>
                </li>
            </ul>

            <div v-show="tab === 'details'" id="panel-details" role="tabpanel" aria-labelledby="tab-details">
                <!-- A read-only viewer (menus.view only) gets the same form with every control disabled. -->
                <fieldset :disabled="!canEdit || isLoading" class="border-0 p-0 m-0">
                    <div class="row">
                        <div class="col-md-4 mb-3" :class="{ 'has-error': !!(msg('name')) }">
                            <label class="form-label" for="name">Name <span class="f_req" aria-hidden="true">*</span></label>
                            <input id="name" v-model="form.name" type="text" maxlength="50" class="form-control form-control-sm"
                                   :aria-invalid="err('name') > 0" aria-describedby="name-err" />
                            <div id="name-err" class="form-text has-error" role="alert">{{ msg('name') }}</div>
                        </div>
                        <div class="col-md-4 mb-3" :class="{ 'has-error': !!(msg('icon')) }">
                            <label class="form-label" for="icon">Icon</label>
                            <input id="icon" v-model="form.icon" type="text" maxlength="50" list="icon-list" class="form-control form-control-sm"
                                   :aria-invalid="err('icon') > 0" aria-describedby="icon-help icon-err" />
                            <datalist id="icon-list"><option v-for="i in iconOptions" :key="i" :value="i" /></datalist>
                            <div id="icon-help" class="form-text">A MudBlazor icon id. Only the listed ones show an icon in the navbar.</div>
                            <div id="icon-err" class="form-text has-error" role="alert">{{ msg('icon') }}</div>
                        </div>
                        <div class="col-md-4 mb-3" :class="{ 'has-error': !!(msg('parentId')) }">
                            <label class="form-label" for="parentId">Parent</label>
                            <select id="parentId" v-model="form.parentId" class="form-select form-select-sm" :aria-invalid="err('parentId') > 0"
                                    aria-describedby="parentId-err">
                                <option :value="null">- - NONE (top level) - -</option>
                                <option v-for="p in parents" :key="p.id" :value="p.id">{{ p.path }}</option>
                            </select>
                            <div id="parentId-err" class="form-text has-error" role="alert">{{ msg('parentId') }}</div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-2 mb-3" :class="{ 'has-error': !!(msg('displaySequence')) }">
                            <label class="form-label" for="displaySequence">Display order</label>
                            <input id="displaySequence" v-model.number="form.displaySequence" type="number" min="0" max="255"
                                   class="form-control form-control-sm" :aria-invalid="err('displaySequence') > 0" aria-describedby="displaySequence-err" />
                            <div id="displaySequence-err" class="form-text has-error" role="alert">{{ msg('displaySequence') }}</div>
                        </div>
                        <div class="col-md-6 mb-3" :class="{ 'has-error': !!(msg('description')) }">
                            <label class="form-label" for="description">Description</label>
                            <input id="description" v-model="form.description" type="text" maxlength="250" class="form-control form-control-sm"
                                   :aria-invalid="err('description') > 0" aria-describedby="description-err" />
                            <div id="description-err" class="form-text has-error" role="alert">{{ msg('description') }}</div>
                        </div>
                        <div class="col-md-4 mb-3">
                            <div class="form-label">Options</div>
                            <label class="checkbox-inline"><input v-model="form.isAlwaysEnabled" type="checkbox" /> Always enabled</label>
                            <label class="checkbox-inline"><input v-model="form.isEnabled" type="checkbox" /> Enabled</label>
                            <div class="form-text">Always enabled shows the item to every user. A disabled item is hidden from the navbar.</div>
                        </div>
                    </div>

                    <div class="row">
                        <div v-for="u in urlFields" :key="u.f" class="col-md-4 mb-3" :class="{ 'has-error': !!(msg(u.f)) }">
                            <label class="form-label" :for="u.f">{{ u.label }}</label>
                            <input :id="u.f" v-model="form[u.f]" type="text" maxlength="250" class="form-control form-control-sm"
                                   :aria-invalid="err(u.f) > 0" :aria-describedby="`${u.f}-err`" />
                            <div :id="`${u.f}-err`" class="form-text has-error" role="alert">{{ msg(u.f) }}</div>
                        </div>
                        <div class="col-md-4 mb-3" :class="{ 'has-error': !!(msg('comment')) }">
                            <label class="form-label" for="comment">Comment</label>
                            <input id="comment" v-model="form.comment" type="text" maxlength="250" class="form-control form-control-sm"
                                   :aria-invalid="err('comment') > 0" aria-describedby="comment-err" />
                            <div id="comment-err" class="form-text has-error" role="alert">{{ msg('comment') }}</div>
                        </div>
                    </div>
                </fieldset>
            </div>

            <div v-if="!isNew" v-show="tab === 'permissions'" id="panel-permissions" role="tabpanel" aria-labelledby="tab-permissions">
                <MenuPermissions v-if="menuId" :menu-id="menuId" :can-edit="canEdit" />
            </div>
        </template>

        <template v-if="!canEdit" #button-row>
            <AppButton action="close" @click="cancel">Back to menus</AppButton>
        </template>
    </DetailPanel>
</template>
