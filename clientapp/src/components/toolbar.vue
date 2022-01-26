<template>
  <div class="col-12 text-start">
    <div class="btn-group">
      <slot></slot>
    </div>
  </div>
  <!-- <div class="col-12 text-start">
        <div class="btn-group">
            <toolbar-item v-for="{className, icon, text, eventName, event} of actions" :key="eventName" :className="className" :icon="icon" :text="text" :eventName="eventName" :event="event" />
        </div>
    </div> -->
</template>

<script>
export default { name: 'Toolbar' };
// import toolbarItem from './toolbar.item';

// export default {
//     name: 'Toolbar',
//     components: {
//         toolbarItem
//     },
//     props: {
//         actions: Array
//     },
//     data() {
//         return {

//         };
//     }
// }
</script>
