<script setup>
import { useHome } from '../composables/useHome.js';

const { currentUser } = useHome();
</script>

<template>
  <div class="main_content">
    <h1>Welcome, {{ currentUser?.fullName }}</h1>
    <p v-if="currentUser?.roles?.length">Roles: {{ currentUser.roles.join(', ') }}</p>
  </div>
</template>
