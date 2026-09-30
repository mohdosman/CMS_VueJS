<script setup>
import { useMenuTree } from '../composables/useMenuTree.js';
import MenuTreeNode from '../components/MenuTreeNode.vue';
import AppDialog from '../../../common/components/AppDialog.vue';

const { tree, filter, isLoading, canEdit, confirming, search, clear, edit, addRoot, addChild, remove } = useMenuTree();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Menus</h1>

        <SearchPanel title="Search Menus" icon="fa fa-bars" form-name="menuForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-5 mb-3">
                        <label class="form-label" for="filter">Search menus</label>
                        <input id="filter" v-model="filter" type="text" class="form-control form-control-sm" aria-describedby="filter-help"
                               @input="search" />
                        <div id="filter-help" class="form-text">By name or url. Matching items keep their parents.</div>
                    </div>
                </div>
            </template>
            <template #buttons>
                <AppButton action="search" :disabled="isLoading" />
                <AppButton v-if="canEdit" action="add" @click="addRoot">New root</AppButton>
                <AppButton action="clear" @click="clear" />
            </template>
        </SearchPanel>

        <div class="row" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">Menu Tree</h2>
            <div class="col-md-12">
                <p v-if="isLoading" role="status">Loading...</p>
                <p v-else-if="!tree.length" class="msg-error text-center"><strong>No Menu Items Found</strong></p>
                <ul v-else class="list-unstyled mb-3">
                    <MenuTreeNode v-for="n in tree" :key="n.id" :node="n" :can-edit="canEdit" :force-open="!!filter.trim()"
                                  @edit="edit" @add-child="addChild" @remove="confirming = $event" />
                </ul>
            </div>
        </div>

        <AppDialog v-if="confirming" title="Delete menu item" @close="confirming = null">
            <p>
                Delete <strong>{{ confirming.name }}</strong>? Its permissions and the role grants of those permissions are removed too.
                An item that still has sub-menus cannot be deleted. This cannot be undone.
            </p>
            <AppButton action="delete" @click="remove">Delete menu item</AppButton>
            <AppButton action="cancel" @click="confirming = null" />
        </AppDialog>
    </div>
</template>
