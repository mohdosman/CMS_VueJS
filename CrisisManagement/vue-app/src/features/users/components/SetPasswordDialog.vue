<script setup>
import { useSetPassword } from '../composables/useSetPassword.js';
import PasswordChecklist from '../../../common/components/PasswordChecklist.vue';

const props = defineProps({
    userKey: { type: String, required: true },
    policy: { type: Object, default: null }   // the structured password rules from the server
});
const emit = defineEmits(['close', 'saved']);

const { password, confirmPassword, isSaving, errors, save } = useSetPassword(props, emit);
</script>

<template>
    <AppDialog title="Set password" @close="emit('close')">
        <form novalidate autocomplete="off" @submit.prevent="save">

            <div class="mb-3" :class="{ 'has-error': !!(errors.password?.join(' ')) }">
                <label class="form-label" for="newPassword">New password <span class="f_req" aria-hidden="true">*</span></label>
                <input id="newPassword" v-model="password" type="password" autocomplete="new-password" autofocus
                       class="form-control form-control-sm" :aria-invalid="!!errors.password"
                       aria-describedby="password-rules newPassword-err" />
                <div id="newPassword-err" class="form-text has-error" role="alert">{{ errors.password?.join(' ') }}</div>
            </div>
            <div class="mb-3" :class="{ 'has-error': !!(errors.confirmPassword?.join(' ')) }">
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
