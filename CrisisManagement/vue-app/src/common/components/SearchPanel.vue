<script>
let uid = 0;
</script>

<script setup>
import { ref } from 'vue';

const props = defineProps({
    title: { type: String, required: true },
    icon: { type: String, default: 'fa fa-search' },
    formName: { type: String, default: 'searchForm' },
    formLabelledby: { type: String, default: '' },
    formClass: { type: String, default: '' },
    buttonColClass: { type: String, default: 'col-md-6 offset-md-3' },
    collapsed: { type: Boolean, default: false }
});

const emit = defineEmits(['submit', 'reset']);

const isCollapsed = ref(props.collapsed);
const bodyId = `search-panel-body-${++uid}`;
</script>

<template>
    <div class="row">
        <div class="col-sm-12">
            <div class="card">
                <div class="card-header search-panel__heading" role="heading" aria-level="2">
                    <button type="button" class="search-panel__toggle"
                            :aria-expanded="!isCollapsed"
                            :aria-controls="bodyId"
                            :title="isCollapsed ? 'Expand' : 'Collapse'"
                            @click="isCollapsed = !isCollapsed">
                        <i :class="icon" aria-hidden="true"></i>
                        {{ title }}
                        <i class="fa search-panel__chevron"
                           :class="isCollapsed ? 'fa-chevron-down' : 'fa-chevron-up'" aria-hidden="true"></i>
                    </button>
                </div>

                <transition name="search-panel-slide">
                    <div v-show="!isCollapsed" :id="bodyId" class="card-body">
                        <form :name="formName" :class="formClass"
                              :aria-labelledby="formLabelledby || undefined"
                              :aria-label="!formLabelledby ? title : undefined"
                              @submit.prevent="emit('submit')" @keydown.esc="emit('reset')">
                            <slot name="fields" />
                            <slot name="button-row">
                                <div class="row">
                                    <div :class="buttonColClass">
                                        <slot name="buttons" />
                                    </div>
                                </div>
                            </slot>
                        </form>
                    </div>
                </transition>

                <slot name="resources" />
            </div>
        </div>
    </div>
</template>

<style scoped>
.search-panel__heading {
    user-select: none;
}

/* Full-width unstyled button so the whole heading stays the click target
   while being reachable and operable from the keyboard. */
.search-panel__toggle {
    display: flex;
    align-items: center;
    gap: 6px;
    width: 100%;
    background: none;
    border: 0;
    padding: 0;
    margin: 0;
    color: inherit;
    font: inherit;
    text-align: left;
    cursor: pointer;
}

.search-panel__chevron {
    margin-left: auto;
    font-size: 12px;
    transition: transform .2s;
}

.search-panel-slide-enter-active,
.search-panel-slide-leave-active {
    transition: opacity .2s ease, max-height .25s ease;
    overflow: hidden;
    max-height: 600px;
}

.search-panel-slide-enter-from,
.search-panel-slide-leave-to {
    opacity: 0;
    max-height: 0;
}
</style>
