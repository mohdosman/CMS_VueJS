<script setup>
import { ref, onMounted } from 'vue';
import AppSpinner from './common/components/AppSpinner.vue';
import HelpDialog from './common/components/HelpDialog.vue';

// The navbar is server-rendered (outside #vue-app), so its Help button reaches the SPA through
// window.cms.openHelp, the same bridge SafetyNet uses.
const helpOpen = ref(false);
onMounted(() => { window.cms = { openHelp: () => { helpOpen.value = true; } }; });
</script>

<template>
  <AppSpinner />
  <!-- Keyed on the path so /admin/users/0 -> /admin/users/<key> (after a create) remounts the view. -->
  <router-view :key="$route.fullPath" />
  <HelpDialog v-if="helpOpen" @close="helpOpen = false" />
</template>
