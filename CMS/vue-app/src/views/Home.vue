<script setup>
import { useAppStore } from '../stores/useAppStore.js';

const appStore = useAppStore();
</script>

<template>
  <div class="main_content">
    <h1>Welcome, {{ appStore.currentUser?.fullName }}</h1>
    <p v-if="appStore.currentUser?.roles?.length">Roles: {{ appStore.currentUser.roles.join(', ') }}</p>
  </div>
</template>
