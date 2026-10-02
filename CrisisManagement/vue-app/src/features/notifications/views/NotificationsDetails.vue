<script setup>
import { useNotificationDetail } from '../composables/useNotificationDetail.js';

const { canEdit, title, maxLength, form, errors, isLoading, isSaving, save, cancel } = useNotificationDetail();
</script>

<template>
    <DetailPanel :title="title" icon="fa fa-bell" form-name="notificationForm"
                 main-labelledby="main-title" :can-save="canEdit && !isSaving && !isLoading"
                 :show-buttons="canEdit" @save="save" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ title }}</h1>

            <!-- A read-only viewer (notifications.view only) gets the same form with the control disabled. -->
            <fieldset :disabled="!canEdit || isLoading" class="border-0 p-0 m-0">
                <div class="row">
                    <div class="col-md-8 mb-3" :class="{ 'has-error': !!(errors.notification?.join(' ')) }">
                        <label class="form-label" for="notification">Notification <span class="f_req" aria-hidden="true">*</span></label>
                        <textarea id="notification" v-model="form.notification" rows="4" :maxlength="maxLength" class="form-control form-control-sm"
                                  :aria-invalid="!!errors.notification?.length" aria-describedby="notification-err"></textarea>
                        <div id="notification-err" class="form-text has-error" role="alert">{{ errors.notification?.join(' ') }}</div>
                    </div>
                </div>
            </fieldset>

        </template>

        <template v-if="!canEdit" #button-row>
            <AppButton action="close" @click="cancel">Back to search</AppButton>
        </template>
    </DetailPanel>
</template>
