<script setup>
import AppSpinner from './common/components/AppSpinner.vue';
import HelpDialog from './common/components/HelpDialog.vue';
import { useAppHelp } from './common/composables/useAppHelp.js';

const { helpOpen } = useAppHelp();
</script>

<template>
  <AppSpinner />
  <!-- Keyed on the path so /admin/users/0 -> /admin/users/<key> (after a create) remounts the view. -->
  <router-view :key="$route.fullPath" />
  <HelpDialog v-if="helpOpen" @close="helpOpen = false" />
</template>
