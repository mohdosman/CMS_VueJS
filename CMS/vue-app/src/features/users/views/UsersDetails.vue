<script setup>
import { useUserDetail } from '../composables/useUserDetail.js';
import SetPasswordDialog from '../components/SetPasswordDialog.vue';
import AppDialog from '../../../common/components/AppDialog.vue';

const {
    isNew, canEdit, form, info, roles, providers, policy, errors, formError, dialog,
    isLoading, isSaving, hasAdminRole, save, remove, passwordSet, cancel
} = useUserDetail();

const dateTime = (v) => (v ? new Date(v).toLocaleString() : '');
const err = (f) => errors.value[f]?.length ?? 0;
</script>

<template>
    <DetailPanel :title="isNew ? 'Add User' : 'User Details'" icon="fa fa-user" form-name="userForm"
                 main-labelledby="main-title" :can-save="canEdit && !isSaving && !isLoading"
                 :show-buttons="canEdit" @save="save" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ isNew ? 'Add User' : 'User Details' }}</h1>

            <div v-if="formError" class="alert alert-danger" role="alert">{{ formError }}</div>

            <!-- A read-only viewer (users.view only) gets the same form with every control disabled. -->
            <fieldset :disabled="!canEdit || isLoading" class="border-0 p-0 m-0">
                <div v-if="isNew" class="mb-3" role="radiogroup" aria-labelledby="acctTypeLabel">
                    <div id="acctTypeLabel" class="form-label">Account type</div>
                    <label class="radio-inline"><input v-model="form.isADAccount" type="radio" :value="false" name="isAD" /> Local (email and password)</label>
                    <label class="radio-inline"><input v-model="form.isADAccount" type="radio" :value="true" name="isAD" /> Active Directory</label>
                </div>

                <div class="row">
                    <div v-if="form.isADAccount" class="col-md-4 mb-3">
                        <label class="form-label" for="userName">User ID <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="userName" v-model="form.userName" type="text" class="form-control form-control-sm"
                               :disabled="!isNew" :aria-invalid="err('userName') > 0" aria-describedby="userName-help userName-err" />
                        <div id="userName-help" class="form-text">{{ policy.adUserNameRule }}</div>
                        <div id="userName-err" class="form-text has-error" role="alert">{{ errors.userName?.join(' ') }}</div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="firstName">First name <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="firstName" v-model="form.firstName" type="text" class="form-control form-control-sm"
                               :aria-invalid="err('firstName') > 0" aria-describedby="firstName-err" />
                        <div id="firstName-err" class="form-text has-error" role="alert">{{ errors.firstName?.join(' ') }}</div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="lastName">Last name <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="lastName" v-model="form.lastName" type="text" class="form-control form-control-sm"
                               :aria-invalid="err('lastName') > 0" aria-describedby="lastName-err" />
                        <div id="lastName-err" class="form-text has-error" role="alert">{{ errors.lastName?.join(' ') }}</div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="email">Email <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="email" v-model="form.email" type="email" class="form-control form-control-sm"
                               :disabled="!isNew && !form.isADAccount" :aria-invalid="err('email') > 0"
                               aria-describedby="email-help email-err" />
                        <div v-if="!form.isADAccount" id="email-help" class="form-text">A local account signs in with its email address.</div>
                        <div id="email-err" class="form-text has-error" role="alert">{{ errors.email?.join(' ') }}</div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="phoneNumber">Phone</label>
                        <input id="phoneNumber" v-model="form.phoneNumber" type="tel" class="form-control form-control-sm"
                               :aria-invalid="err('phoneNumber') > 0" aria-describedby="phoneNumber-err" />
                        <div id="phoneNumber-err" class="form-text has-error" role="alert">{{ errors.phoneNumber?.join(' ') }}</div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <div class="form-label">Status</div>
                        <label class="checkbox-inline"><input v-model="form.isEnabled" type="checkbox" /> Active</label>
                        <label v-if="!form.isADAccount" class="checkbox-inline"><input v-model="form.twoFactorEnabled" type="checkbox" /> Require two-factor</label>
                    </div>
                </div>

                <!-- Existing users get a new password through the Set password dialog (reuse check + history). -->
                <div v-if="isNew && !form.isADAccount" class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="password">Password <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="password" v-model="form.password" type="password" autocomplete="new-password"
                               class="form-control form-control-sm" :aria-invalid="err('password') > 0"
                               aria-describedby="password-help password-err" />
                        <div id="password-help" class="form-text">
                            The user must change this temporary password.
                            <ul class="mb-0 ps-3"><li v-for="rule in policy.passwordRules" :key="rule">{{ rule }}</li></ul>
                        </div>
                        <div id="password-err" class="form-text has-error" role="alert">{{ errors.password?.join(' ') }}</div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="confirmPassword">Confirm password</label>
                        <input id="confirmPassword" v-model="form.confirmPassword" type="password" autocomplete="new-password"
                               class="form-control form-control-sm" :aria-invalid="err('confirmPassword') > 0"
                               aria-describedby="confirmPassword-err" />
                        <div id="confirmPassword-err" class="form-text has-error" role="alert">{{ errors.confirmPassword?.join(' ') }}</div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6 mb-3">
                        <fieldset aria-describedby="roleIds-err">
                            <legend class="form-label">Roles <span class="f_req" aria-hidden="true">*</span></legend>
                            <div><label v-for="r in roles" :key="r.id" class="checkbox-inline">
                                <input v-model="form.roleIds" type="checkbox" :value="r.id" /> {{ r.label }}
                            </label></div>
                            <div id="roleIds-err" class="form-text has-error" role="alert">{{ errors.roleIds?.join(' ') }}</div>
                        </fieldset>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label" for="providerIds">Providers</label>
                        <select id="providerIds" v-model="form.providerIds" multiple size="6" class="form-select form-select-sm"
                                :disabled="hasAdminRole" aria-describedby="providers-help">
                            <option v-for="p in providers" :key="p.id" :value="p.id">{{ p.label }}</option>
                        </select>
                        <div id="providers-help" class="form-text">
                            {{ hasAdminRole ? 'Administrators have access to every provider.' : 'Hold Ctrl to select more than one.' }}
                        </div>
                    </div>
                </div>

                <div class="mb-3">
                    <label class="form-label" for="notes">Notes</label>
                    <textarea id="notes" v-model="form.notes" rows="2" class="form-control form-control-sm"></textarea>
                </div>
            </fieldset>

            <div v-if="info" class="row text-muted small mb-3">
                <div class="col-md-3">Locked out: {{ info.isLockedOut ? 'Yes' : 'No' }}</div>
                <div class="col-md-3">Last sign-in: {{ dateTime(info.lastLoginAt) }}</div>
                <div class="col-md-3">Created: {{ dateTime(info.createdOn) }}</div>
                <div class="col-md-3">Last updated: {{ dateTime(info.updatedOn) }}</div>
            </div>
        </template>

        <template #actions>
            <template v-if="!isNew && canEdit">
                <AppButton v-if="!form.isADAccount" action="preview" @click="dialog = 'password'">Set password</AppButton>
                <AppButton action="delete" @click="dialog = 'delete'">Delete</AppButton>
            </template>
        </template>

        <template v-if="!canEdit" #button-row>
            <AppButton action="close" @click="cancel">Back to search</AppButton>
        </template>

        <template #below>
            <SetPasswordDialog v-if="dialog === 'password'" :user-key="info.userKey" :rules="policy.passwordRules"
                               @close="dialog = ''" @saved="passwordSet" />
            <AppDialog v-if="dialog === 'delete'" title="Delete user" @close="dialog = ''">
                <p>
                    Delete <strong>{{ info?.userName }}</strong>? This removes the account, its role and provider
                    assignments and its sign-in history. It cannot be undone.
                </p>
                <AppButton action="delete" @click="remove">Delete user</AppButton>
                <AppButton action="cancel" @click="dialog = ''" />
            </AppDialog>
        </template>
    </DetailPanel>
</template>
