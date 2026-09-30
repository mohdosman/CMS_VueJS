<script setup>
import { ref } from 'vue';
import { usersApi } from '../api/usersApi.js';
import AppDialog from '../../../common/components/AppDialog.vue';
import PasswordChecklist from '../../../common/components/PasswordChecklist.vue';
import { apiErrorMessage } from '../../../utils/apiError.js';

const props = defineProps({
    userKey: { type: String, required: true },
    policy: { type: Object, default: null }   // the structured password rules from the server
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
                       aria-describedby="password-rules newPassword-err" />
                <div id="newPassword-err" class="form-text has-error" role="alert">{{ errors.password?.join(' ') }}</div>
            </div>
            <div class="mb-3">
                <label class="form-label" for="newPasswordConfirm">Confirm password <span class="f_req" aria-hidden="true">*</span></label>
                <input id="newPasswordConfirm" v-model="confirmPassword" type="password" autocomplete="new-password"
                       class="form-control form-control-sm" :aria-invalid="!!errors.confirmPassword"
                       aria-describedby="newPasswordConfirm-err" />
                <div id="newPasswordConfirm-err" class="form-text has-error" role="alert">{{ errors.confirmPassword?.join(' ') }}</div>
            </div>

            <!-- After both fields, as in Blazor; ticks off as the password is typed. -->
            <div id="password-rules" class="mb-3">
                <p class="form-text mb-2">The user must change this temporary password at next sign-in.</p>
                <PasswordChecklist :password="password" :confirm="confirmPassword" :policy="policy" />
            </div>

            <AppButton action="save" :disabled="isSaving">Set password</AppButton>
            <AppButton action="cancel" @click="emit('close')" />
        </form>
    </AppDialog>
</template>
