<script setup>
import { useRoute } from 'vue-router';
const route = useRoute();
</script>

<template>
  <div class="main_content">
    <h1>{{ route.meta.title }}</h1>
    <p>This screen has not been migrated yet.</p>
  </div>
</template>
