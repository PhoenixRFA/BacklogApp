<template>
  <v-date-picker @update:modelValue="$emit('update:modelValue', $event)" :modelValue="modelValue" :input-debounce="500" locale="en-US">
    <template v-slot="{ inputValue, inputEvents }">
      <div class="input-group">
        <span class="input-group-text"><font-awesome-icon icon="calendar-alt" /></span>
        <input :value="inputValue" v-on="inputEvents" id="deadline" class="form-control" placeholder="02/14/2001" />
      </div>
    </template>
  </v-date-picker>
</template>

<script>
import { DatePicker as VDatePicker } from 'v-calendar';

export default {
  name: 'DatePicker',
  components: { VDatePicker },
  emits: ['update:modelValue'],
  props: {
    modelValue: Date,
  },
};
</script>
