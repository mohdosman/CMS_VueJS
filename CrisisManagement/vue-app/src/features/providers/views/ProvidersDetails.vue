<script setup>
import { useProviderDetail } from '../composables/useProviderDetail.js';

const {
    msg, err, isNew, canEdit, title, form, info, states, counties, errors, formError, dialog,
    isLoading, isSaving, save, remove, cancel
} = useProviderDetail();

const sections = [
    { key: 'physicalAddress', title: 'Physical address' },
    { key: 'remitAddress', title: 'Remit address' }
];
const addressFields = [
    { f: 'addressLine1', label: 'Address line 1', col: 'col-12' },
    { f: 'addressLine2', label: 'Address line 2', col: 'col-12' },
    { f: 'city', label: 'City', col: 'col-md-6' }
];
const contactFields = [
    { f: 'title', label: 'Title', col: 'col-md-4' },
    { f: 'firstName', label: 'First name', col: 'col-md-4' },
    { f: 'lastName', label: 'Last name', col: 'col-md-4' },
    { f: 'emailAddress', label: 'Email', col: 'col-md-6', type: 'email' },
    { f: 'phone', label: 'Phone', col: 'col-md-3', type: 'tel' },
    { f: 'wirelessPhone', label: 'Wireless phone', col: 'col-md-3', type: 'tel' }
];
</script>

<template>
    <DetailPanel :title="title" icon="fa fa-hospital-o" form-name="providerForm"
                 main-labelledby="main-title" :can-save="canEdit && !isSaving && !isLoading"
                 :show-buttons="canEdit" @save="save" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ title }}</h1>

            <div v-if="formError" class="alert alert-danger" role="alert">{{ formError }}</div>

            <!-- A read-only viewer (providers.view only) gets the same form with every control disabled. -->
            <fieldset :disabled="!canEdit || isLoading" class="border-0 p-0 m-0">
                <h2 class="h6">Provider demographics</h2>
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label class="form-label" for="name">Provider name <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="name" v-model="form.name" type="text" maxlength="150" class="form-control form-control-sm"
                               :aria-invalid="err('name') > 0" aria-describedby="name-err" />
                        <div id="name-err" class="form-text has-error" role="alert">{{ msg('name') }}</div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label" for="abbreviation">Abbreviation <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="abbreviation" v-model="form.abbreviation" type="text" maxlength="50" class="form-control form-control-sm"
                               :aria-invalid="err('abbreviation') > 0" aria-describedby="abbreviation-err" />
                        <div id="abbreviation-err" class="form-text has-error" role="alert">{{ msg('abbreviation') }}</div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label" for="edisonNumber">Edison number <span class="f_req" aria-hidden="true">*</span></label>
                        <input id="edisonNumber" v-model="form.edisonNumber" type="text" maxlength="10" inputmode="numeric"
                               class="form-control form-control-sm" :aria-invalid="err('edisonNumber') > 0"
                               aria-describedby="edisonNumber-help edisonNumber-err" />
                        <div id="edisonNumber-help" class="form-text">Exactly 10 digits.</div>
                        <div id="edisonNumber-err" class="form-text has-error" role="alert">{{ msg('edisonNumber') }}</div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label" for="npi">NPI</label>
                        <input id="npi" v-model="form.npi" type="text" maxlength="10" inputmode="numeric" class="form-control form-control-sm"
                               :aria-invalid="err('npi') > 0" aria-describedby="npi-help npi-err" />
                        <div id="npi-help" class="form-text">Exactly 10 digits when entered.</div>
                        <div id="npi-err" class="form-text has-error" role="alert">{{ msg('npi') }}</div>
                    </div>
                </div>

                <section v-for="s in sections" :key="s.key" class="mb-2" :aria-labelledby="`${s.key}-heading`">
                    <h2 :id="`${s.key}-heading`" class="h6 mt-2">{{ s.title }}</h2>
                    <div class="row">
                        <div class="col-lg-6">
                            <div class="row">
                                <div v-for="a in addressFields" :key="a.f" class="mb-3" :class="a.col">
                                    <label class="form-label" :for="`${s.key}-${a.f}`">{{ a.label }}</label>
                                    <input :id="`${s.key}-${a.f}`" v-model="form[s.key][a.f]" type="text" class="form-control form-control-sm"
                                           :aria-invalid="err(`${s.key}.${a.f}`) > 0" :aria-describedby="`${s.key}-${a.f}-err`" />
                                    <div :id="`${s.key}-${a.f}-err`" class="form-text has-error" role="alert">{{ msg(`${s.key}.${a.f}`) }}</div>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="form-label" :for="`${s.key}-countyId`">County</label>
                                    <select :id="`${s.key}-countyId`" v-model="form[s.key].countyId" class="form-select form-select-sm">
                                        <option :value="null">- - SELECT - -</option>
                                        <option v-for="c in counties" :key="c.id" :value="c.id">{{ c.label }}</option>
                                    </select>
                                </div>
                                <div class="col-md-6 mb-3">
                                    <label class="form-label" :for="`${s.key}-stateId`">State</label>
                                    <select :id="`${s.key}-stateId`" v-model="form[s.key].stateId" class="form-select form-select-sm">
                                        <option :value="null">- - SELECT - -</option>
                                        <option v-for="st in states" :key="st.id" :value="st.id">{{ st.label }}</option>
                                    </select>
                                </div>
                                <div class="col-8 col-md-6 mb-3">
                                    <label class="form-label" :for="`${s.key}-zipcode`">ZIP code</label>
                                    <input :id="`${s.key}-zipcode`" v-model="form[s.key].zipcode" type="text" maxlength="5" class="form-control form-control-sm"
                                           :aria-invalid="err(`${s.key}.zipcode`) > 0" :aria-describedby="`${s.key}-zipcode-err`" />
                                    <div :id="`${s.key}-zipcode-err`" class="form-text has-error" role="alert">{{ msg(`${s.key}.zipcode`) }}</div>
                                </div>
                                <div class="col-4 col-md-6 mb-3">
                                    <label class="form-label" :for="`${s.key}-zipExtension`">Ext.</label>
                                    <input :id="`${s.key}-zipExtension`" v-model="form[s.key].zipExtension" type="text" maxlength="4" class="form-control form-control-sm"
                                           :aria-invalid="err(`${s.key}.zipExtension`) > 0" :aria-describedby="`${s.key}-zipExtension-err`" />
                                    <div :id="`${s.key}-zipExtension-err`" class="form-text has-error" role="alert">{{ msg(`${s.key}.zipExtension`) }}</div>
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="row">
                                <div v-for="c in contactFields" :key="c.f" class="mb-3" :class="c.col">
                                    <label class="form-label" :for="`${s.key}-contact-${c.f}`">{{ c.label }}</label>
                                    <input :id="`${s.key}-contact-${c.f}`" v-model="form[s.key].contact[c.f]" :type="c.type || 'text'" class="form-control form-control-sm"
                                           :aria-invalid="err(`${s.key}.contact.${c.f}`) > 0" :aria-describedby="`${s.key}-contact-${c.f}-err`" />
                                    <div :id="`${s.key}-contact-${c.f}-err`" class="form-text has-error" role="alert">{{ msg(`${s.key}.contact.${c.f}`) }}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>
            </fieldset>

            <div v-if="info" class="text-muted small mb-3">
                Created: {{ new Date(info.createdOn).toLocaleString() }} &middot; Last updated: {{ new Date(info.updatedOn).toLocaleString() }}
            </div>
        </template>

        <template #actions>
            <AppButton v-if="!isNew && canEdit" action="delete" @click="dialog = 'delete'">Delete</AppButton>
        </template>

        <template v-if="!canEdit" #button-row>
            <AppButton action="close" @click="cancel">Back to search</AppButton>
        </template>

        <template #below>
            <AppDialog v-if="dialog === 'delete'" title="Delete provider" @close="dialog = ''">
                <p>Delete <strong>{{ form.name }}</strong>? This cannot be undone. A provider still used by users, contracts, assessments or services cannot be deleted.</p>
                <AppButton action="delete" @click="remove">Delete provider</AppButton>
                <AppButton action="cancel" @click="dialog = ''" />
            </AppDialog>
        </template>
    </DetailPanel>
</template>
