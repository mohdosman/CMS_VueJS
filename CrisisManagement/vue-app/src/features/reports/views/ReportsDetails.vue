<script setup>
import { useReportDetail } from '../composables/useReportDetail.js';
import AppDialog from '../../../common/components/AppDialog.vue';
import FieldSelect from '../../../common/components/FieldSelect.vue';
import FieldInput from '../../../common/components/FieldInput.vue';

const { isNew, title, form, fileName, available, formError, dialog, isLoading, isSaving, msg, save, remove, cancel } = useReportDetail();

// Fixed by the report server: PDF, CSV, Excel and text.
const exportOptions = [
    { id: 'PDF', label: 'PDF' }, { id: 'CSV', label: 'CSV' }, { id: 'MSExcel', label: 'Excel' }, { id: 'TXT', label: 'TXT' }
];
const nameOptions = () => available.value.map((n) => ({ id: n, label: n }));
</script>

<template>
    <DetailPanel :title="title" icon="fa fa-bar-chart" form-name="reportEditForm" main-labelledby="main-title"
 :can-save="!isSaving && !isLoading" @save="save" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ title }}</h1>

            <div v-if="formError" class="alert alert-danger" role="alert">{{ formError }}</div>

            <fieldset :disabled="isLoading" class="border-0 p-0 m-0">
                <div class="row">
                    <template v-if="isNew">
                        <div v-if="!available.length && !isLoading" class="col-md-6 mb-3">
                            <span class="form-label d-block">Report Name</span>
                            <p class="form-text" role="status">No reports available</p>
                        </div>
                        <!-- The name is chosen from the report files on the server; the file name follows from it. -->
                        <div v-else class="col-md-6 mb-3">
                            <label class="form-label" for="reportName">Report Name <span class="f_req" aria-hidden="true">*</span></label>
                            <select id="reportName" v-model="form.reportName" class="form-select form-select-sm" aria-required="true"
                                    :aria-invalid="!!msg('reportName')" aria-describedby="reportName-err">
                                <option value="">- - SELECT - -</option>
                                <option v-for="n in available" :key="n" :value="n">{{ n }}</option>
                            </select>
                            <div id="reportName-err" class="form-text has-error" role="alert">{{ msg('reportName') }}</div>
                        </div>
                    </template>
                    <FieldInput v-else :model-value="form.reportName" label="Report Name" disabled col="col-md-6" />
                    <FieldInput :model-value="fileName" label="File Name" disabled :error="msg('fileName')" col="col-md-6" />
                    <FieldSelect v-model="form.exportOption" label="Export Option" required :options="exportOptions" placeholder="- - SELECT - -"
                                 :error="msg('exportOption')" col="col-md-6" />
                    <div class="col-md-12 mb-3">
                        <label class="form-label" for="description">Description <span class="f_req" aria-hidden="true">*</span></label>
                        <textarea id="description" v-model="form.description" class="form-control form-control-sm" rows="3" maxlength="255" aria-required="true"
                                  :aria-invalid="!!msg('description')" aria-describedby="description-err"></textarea>
                        <div id="description-err" class="form-text has-error" role="alert">{{ msg('description') }}</div>
                    </div>
                </div>
            </fieldset>
        </template>

        <template #actions>
            <AppButton v-if="!isNew" action="delete" @click="dialog = 'delete'">Delete</AppButton>
        </template>

        <template #below>
            <AppDialog v-if="dialog === 'delete'" title="Delete report" @close="dialog = ''">
                <p>Delete report '{{ form.reportName }}'? This action cannot be undone.</p>
                <AppButton action="delete" @click="remove">Delete report</AppButton>
                <AppButton action="cancel" @click="dialog = ''" />
            </AppDialog>
        </template>
    </DetailPanel>
</template>
