<script setup>
import { ref } from 'vue';
import { usersApi } from '../api/usersApi.js';
import AppDialog from '../../../common/components/AppDialog.vue';
import { apiErrorMessage } from '../../../utils/apiError.js';

const props = defineProps({
    userKey: { type: String, required: true },
    rules: { type: Array, default: () => [] }
});
const emit = defineEmits(['close', 'saved']);

const password = ref('');
const confirmPassword = ref('');
const errors = ref({});
const formError = ref('');
const isSaving = ref(false);

async function save() {
    errors.value = {};
    formError.value = '';
    isSaving.value = true;
    try {
        await usersApi.setPassword(props.userKey, { password: password.value, confirmPassword: confirmPassword.value });
        emit('saved');
    } catch (e) {
        // Field problems (400) sit next to their inputs; anything else is shown in the dialog.
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        errors.value = fieldErrors ?? {};
        formError.value = fieldErrors ? '' : apiErrorMessage(e);
    } finally {
        isSaving.value = false;
    }
}
</script>

<template>
    <AppDialog title="Set password" @close="emit('close')">
        <form novalidate autocomplete="off" @submit.prevent="save">
            <div v-if="formError" class="alert alert-danger" role="alert">{{ formError }}</div>

            <div class="mb-3">
                <label class="form-label" for="newPassword">New password <span class="f_req" aria-hidden="true">*</span></label>
                <input id="newPassword" v-model="password" type="password" autocomplete="new-password" autofocus
                       class="form-control form-control-sm" :aria-invalid="!!errors.password"
                       aria-describedby="newPassword-help newPassword-err" />
                <div id="newPassword-help" class="form-text">
                    The user must change this temporary password at next sign-in.
                    <ul class="mb-0 ps-3"><li v-for="rule in rules" :key="rule">{{ rule }}</li></ul>
                </div>
                <div id="newPassword-err" class="form-text has-error" role="alert">{{ errors.password?.join(' ') }}</div>
            </div>
            <div class="mb-3">
                <label class="form-label" for="newPasswordConfirm">Confirm password <span class="f_req" aria-hidden="true">*</span></label>
                <input id="newPasswordConfirm" v-model="confirmPassword" type="password" autocomplete="new-password"
                       class="form-control form-control-sm" :aria-invalid="!!errors.confirmPassword"
                       aria-describedby="newPasswordConfirm-err" />
                <div id="newPasswordConfirm-err" class="form-text has-error" role="alert">{{ errors.confirmPassword?.join(' ') }}</div>
            </div>

            <AppButton action="save" :disabled="isSaving">Set password</AppButton>
            <AppButton action="cancel" @click="emit('close')" />
        </form>
    </AppDialog>
</template>
