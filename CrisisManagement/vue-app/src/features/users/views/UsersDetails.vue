<script setup>
import { computed } from 'vue';
import { useUserDetail } from '../composables/useUserDetail.js';
import SetPasswordDialog from '../components/SetPasswordDialog.vue';
import UserAgreementsDialog from '../components/UserAgreementsDialog.vue';
import UserAgreementUploadDialog from '../components/UserAgreementUploadDialog.vue';
import MultiSelectDropdown from '../../../common/components/MultiSelectDropdown.vue';
import PasswordChecklist from '../../../common/components/PasswordChecklist.vue';
import RoleChips from '../../../common/components/RoleChips.vue';
import AppDialog from '../../../common/components/AppDialog.vue';

const {
    isNew, canEdit, form, info, roles, providers, policy, errors, formError, dialog,
    isLoading, isSaving, hasAdminRole, documentCount, resetMfa, save, remove, passwordSet, agreementsClosed, uploadClosed, cancel
} = useUserDetail();

const err = (f) => errors.value[f]?.length ?? 0;
// A local account signs in with its email, so its user ID is the email (fixed once created).
const userIdShown = computed(() => (form.isADAccount || !isNew ? form.userName : form.email));
const idCaption = computed(() => (form.isADAccount
    ? 'The user ID cannot be changed after the account is created.'
    : 'The email and user ID cannot be changed after the account is created.'));
</script>

<template>
    <DetailPanel :title="isNew ? 'Add User' : 'User Details'" icon="fa fa-user" form-name="userForm"
                 main-labelledby="main-title" :can-save="canEdit && !isSaving && !isLoading"
                 :show-buttons="canEdit" @save="save" @cancel="cancel">
        <!-- Agreements sit with the title, not the save bar: they are not part of the edit. -->
        <template v-if="!isNew && info" #heading-docs>
            <button type="button" class="link-btn" @click="dialog = 'agreements'">
                <i class="fa fa-file-o" aria-hidden="true"></i>
                View User Agreement ({{ documentCount }})
            </button>
            <button v-if="canEdit" type="button" class="link-btn" @click="dialog = 'upload'">
                <i class="fa fa-upload" aria-hidden="true"></i>
                Upload User Agreement
            </button>
        </template>

        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ isNew ? 'Add User' : 'User Details' }}</h1>

            <div v-if="formError" class="alert alert-danger" role="alert">{{ formError }}</div>

            <!-- A read-only viewer (users.view only) gets the same form with every control disabled. -->
            <fieldset :disabled="!canEdit || isLoading" class="border-0 p-0 m-0">
                <div class="row align-items-start">
                    <div class="col-md-2 mb-3">
                        <div class="form-label">Account type</div>
                        <label class="checkbox-inline"><input v-model="form.isADAccount" type="checkbox" :disabled="!isNew" /> Is AD Account</label>
                        <div v-if="err('isADAccount')" class="form-text has-error" role="alert">{{ errors.isADAccount?.join(' ') }}</div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="email">Email <span v-if="isNew || form.isADAccount" class="f_req" aria-hidden="true">*</span></label>
                        <input id="email" v-model="form.email" type="email" class="form-control form-control-sm"
                               :disabled="!isNew && !form.isADAccount" :aria-invalid="err('email') > 0" aria-describedby="email-err" />
                        <div id="email-err" class="form-text has-error" role="alert">{{ errors.email?.join(' ') }}</div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="userName">User ID <span v-if="isNew && form.isADAccount" class="f_req" aria-hidden="true">*</span></label>
                        <input id="userName" :value="userIdShown" type="text" class="form-control form-control-sm"
                               :disabled="!(isNew && form.isADAccount)" :aria-invalid="err('userName') > 0"
                               aria-describedby="userName-help userName-err" @input="form.userName = $event.target.value" />
                        <div id="userName-help" class="form-text">
                            <template v-if="isNew">{{ form.isADAccount ? policy.adUserNameRule : 'Same as the email address' }}</template>
                        </div>
                        <div id="userName-err" class="form-text has-error" role="alert">{{ errors.userName?.join(' ') }}</div>
                    </div>
                    <div v-if="isNew" class="col-md-4 mb-3 pt-md-4 text-muted small">{{ idCaption }}</div>
                </div>

                <div class="row">
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="firstName">First name <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="firstName" v-model="form.firstName" type="text" class="form-control form-control-sm"
                               :aria-invalid="err('firstName') > 0" aria-describedby="firstName-err" />
                        <div id="firstName-err" class="form-text has-error" role="alert">{{ errors.firstName?.join(' ') }}</div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="lastName">Last name <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="lastName" v-model="form.lastName" type="text" class="form-control form-control-sm"
                               :aria-invalid="err('lastName') > 0" aria-describedby="lastName-err" />
                        <div id="lastName-err" class="form-text has-error" role="alert">{{ errors.lastName?.join(' ') }}</div>
                    </div>
                    <div class="col-md-2 mb-3">
                        <label class="form-label" for="phoneNumber">Phone number</label>
                        <input id="phoneNumber" v-model="form.phoneNumber" type="tel" class="form-control form-control-sm"
                               :aria-invalid="err('phoneNumber') > 0" aria-describedby="phoneNumber-err" />
                        <div id="phoneNumber-err" class="form-text has-error" role="alert">{{ errors.phoneNumber?.join(' ') }}</div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <MultiSelectDropdown id="providerIds" v-model="form.providerIds" label="Providers" :options="providers"
                                             :disabled="hasAdminRole" />
                        <div v-if="hasAdminRole" class="form-text">Administrators have unrestricted access; provider assignment is not applicable.</div>
                    </div>
                </div>

                <div class="mb-3">
                    <label class="checkbox-inline"><input v-model="form.isEnabled" type="checkbox" /> Active</label>
                    <!-- AD accounts get MFA from Entra, so the switch would be a lie for them. -->
                    <label v-if="!form.isADAccount" class="checkbox-inline"
                           title="When on, the user must set up Microsoft Authenticator at their next sign-in before they can use the application, and cannot turn it off themselves.">
                        <input v-model="form.twoFactorEnabled" type="checkbox" /> Require two-factor
                    </label>
                </div>

                <!-- Existing users get a new password through the Set password dialog (reuse check + history). -->
                <div v-if="isNew && !form.isADAccount" class="row">
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="password">Password <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="password" v-model="form.password" type="password" autocomplete="new-password"
                               class="form-control form-control-sm" :aria-invalid="err('password') > 0"
                               aria-describedby="password-help password-err" />
                        <div id="password-err" class="form-text has-error" role="alert">{{ errors.password?.join(' ') }}</div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="confirmPassword">Confirm password <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="confirmPassword" v-model="form.confirmPassword" type="password" autocomplete="new-password"
                               class="form-control form-control-sm" :aria-invalid="err('confirmPassword') > 0"
                               aria-describedby="confirmPassword-err" />
                        <div id="confirmPassword-err" class="form-text has-error" role="alert">{{ errors.confirmPassword?.join(' ') }}</div>
                    </div>
                    <div id="password-help" class="col-md-6 mb-3">
                        <p class="form-text mb-2">The user must change this temporary password.</p>
                        <PasswordChecklist :password="form.password" :confirm="form.confirmPassword" :policy="policy.password" />
                    </div>
                </div>

                <div class="mb-3">
                    <RoleChips v-model="form.roleIds" label="Roles *" :options="roles" :disabled="!canEdit" />
                    <div id="roleIds-err" class="form-text has-error" role="alert">{{ errors.roleIds?.join(' ') }}</div>
                </div>
            </fieldset>
        </template>

        <template #actions>
            <template v-if="!isNew && canEdit">
                <AppButton v-if="!form.isADAccount" action="preview" @click="dialog = 'password'">Set password</AppButton>
                <AppButton v-if="!form.isADAccount && info?.twoFactorEnabled" action="preview" @click="dialog = 'mfa'">Reset MFA</AppButton>
                <AppButton action="delete" @click="dialog = 'delete'">Delete</AppButton>
            </template>
        </template>

        <template v-if="!canEdit" #button-row>
            <AppButton action="close" @click="cancel">Back to search</AppButton>
        </template>

        <template #below>
            <SetPasswordDialog v-if="dialog === 'password'" :user-key="info.userKey" :policy="policy.password"
                               @close="dialog = ''" @saved="passwordSet" />
            <AppDialog v-if="dialog === 'mfa'" title="Reset MFA" @close="dialog = ''">
                <p>
                    Reset two-factor authentication for <strong>{{ info?.userName }}</strong>? This turns the requirement
                    off and removes their authenticator, so their current device stops working. Turn "Require two-factor"
                    back on if they should enroll a new device at next sign-in.
                </p>
                <AppButton action="run" @click="resetMfa">Reset MFA</AppButton>
                <AppButton action="cancel" @click="dialog = ''" />
            </AppDialog>
            <UserAgreementsDialog v-if="dialog === 'agreements'" :user-key="info.userKey" :can-edit="canEdit"
                                  @close="agreementsClosed" />
            <UserAgreementUploadDialog v-if="dialog === 'upload'" :user-key="info.userKey" @close="uploadClosed" />
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
