<script setup>
defineProps({
    title: { type: String, required: true },
    icon: { type: String, default: 'fa fa-briefcase' },
    formName: { type: String, default: '' },
    mainLabelledby: { type: String, default: '' },
    formLabelledby: { type: String, default: '' },
    formClass: { type: String, default: '' },
    buttonColClass: { type: String, default: 'col-md-8 offset-md-3' },
    canSave: { type: Boolean, default: true },
    showButtons: { type: Boolean, default: true }
});

const emit = defineEmits(['save', 'cancel']);
</script>

<template>
    <main class="main_content" :aria-labelledby="mainLabelledby || undefined">
        <div class="row">
            <div class="col-md-12">
                <div class="card">
                    <div class="card-header" role="heading" aria-level="2">
                        <i :class="icon" aria-hidden="true"></i>
                        {{ title }}
                        <span v-if="$slots['heading-extra'] || $slots['heading-docs']" class="float-end">
                            <slot name="heading-extra" />
                            <slot name="heading-docs" />
                        </span>
                    </div>
                    <div class="card-body">
                        <form :name="formName || undefined" :class="formClass"
                              :aria-labelledby="formLabelledby || undefined"
                              novalidate autocomplete="off" @submit.prevent="emit('save')">
                            <slot name="fields" />
                            <slot name="button-row">
                                <div v-if="showButtons" class="row">
                                    <div :class="buttonColClass">
                                        <AppButton action="save" :disabled="!canSave" />
                                        <AppButton action="cancel" @mousedown.prevent @click="emit('cancel')" />
                                        <slot name="actions" />
                                    </div>
                                </div>
                            </slot>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <slot name="below" />
    </main>
</template>