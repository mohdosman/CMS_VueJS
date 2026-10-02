<script setup>
import { useComingSoon } from '../common/composables/useComingSoon.js';

const { title } = useComingSoon();
</script>

<template>
  <div class="main_content">
    <h1>{{ title }}</h1>
    <p>This screen has not been migrated yet.</p>
  </div>
</template>
