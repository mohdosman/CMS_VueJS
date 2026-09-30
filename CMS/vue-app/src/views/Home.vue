<script setup>
import { boot } from '../boot.js';
</script>

<template>
  <div class="main_content">
    <h1>Welcome, {{ boot.currentUser?.fullName }}</h1>
    <p v-if="boot.currentUser?.roles?.length">Roles: {{ boot.currentUser.roles.join(', ') }}</p>
  </div>
</template>
