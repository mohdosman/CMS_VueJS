<script setup>
import { useProfile } from '../composables/useProfile.js';

const { profile, setupUrl } = useProfile();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <div class="row">
            <div class="col-md-8">
                <div class="card mb-3">
                    <div class="card-header" role="heading" aria-level="1" id="main-title"><i class="fa fa-id-card-o" aria-hidden="true"></i> My Profile</div>
                    <div class="card-body">
                        <p v-if="!profile" role="status">Loading...</p>
                        <dl v-else class="row mb-0">
                            <dt class="col-sm-3">User ID</dt><dd class="col-sm-9">{{ profile.userName }}</dd>
                            <dt class="col-sm-3">Email</dt><dd class="col-sm-9">{{ profile.email }}</dd>
                            <dt class="col-sm-3">First Name</dt><dd class="col-sm-9">{{ profile.firstName }}</dd>
                            <dt class="col-sm-3">Last Name</dt><dd class="col-sm-9">{{ profile.lastName }}</dd>
                        </dl>
                    </div>
                </div>

                <div v-if="profile" class="card">
                    <div class="card-header" role="heading" aria-level="2"><i class="fa fa-lock" aria-hidden="true"></i> Security</div>
                    <div class="card-body">
                        <div v-if="profile.isAdAccount" class="alert alert-info mb-0" role="note">
                            Your account is managed by Azure AD. Password changes and two-factor authentication are handled through your organization's portal.
                        </div>
                        <div v-else-if="profile.isLockedOut" class="alert alert-warning mb-0" role="alert">
                            Your account is locked out. Password changes are not available.
                        </div>
                        <template v-else>
                            <h2 class="h6">Two-factor authentication</h2>
                            <p>
                                {{ profile.mfaEnrolled
                                    ? 'On. You are asked for an authenticator code when you sign in.'
                                    : profile.mfaRequired
                                        ? 'Required for your account, but no authenticator is set up yet.'
                                        : 'Off. Add a code from Microsoft Authenticator to your sign in.' }}
                            </p>
                            <a class="btn btn-outline-secondary btn-sm" :href="setupUrl">{{ profile.mfaEnrolled ? 'Re-enroll device' : 'Set up' }}</a>
                        </template>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
