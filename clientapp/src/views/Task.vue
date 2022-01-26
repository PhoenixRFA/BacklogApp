<template>
  <div class="container">
    <div class="row">
      <breadcrumbs />
    </div>

    <div class="row">
      <div class="col-12">
        <h3 class="mb-4 fw-bold">Task: {{ taskName }}</h3>
      </div>
    </div>

    <div class="row">
      <div class="col-12">
        <div class="card">
          <div class="card-body">
            <div class="row">
              <form>
                <div class="row mb-3">
                  <label for="name" class="col-sm-4">Name</label>
                  <div class="col-sm-8">
                    <input v-model="name" id="name" class="form-control" placeholder="Name" />
                  </div>
                </div>
                <div class="row mb-3">
                  <label for="status" class="col-sm-4">Status</label>
                  <div class="col-sm-8 col-lg-4">
                    <select v-model="status" id="status" :class="statusSelectClass" class="form-select">
                      <option v-for="{ value, text, className } of taskStatuses" :key="value" :class="className" :value="value">{{ text }}</option>
                    </select>
                  </div>
                </div>
                <div class="row mb-3">
                  <label for="description" class="col-sm-4">Description</label>
                  <div class="col-sm-8">
                    <textarea v-model="description" id="description" class="form-control" placeholder="Description"></textarea>
                  </div>
                </div>
                <div class="row mb-3">
                  <label for="deadline" class="col-sm-4">Deadline</label>
                  <div class="col-sm-4">
                    <date-picker v-model="deadline" />
                  </div>
                </div>
                <div class="row mb-3">
                  <label for="priority" class="col-sm-4">Priority</label>
                  <div class="col-sm-8 text-start">
                    <priority-control v-model="priority" />
                  </div>
                </div>
                <div class="row">
                  <label for="assessment" class="col-sm-4">Assessment</label>
                  <div class="col-sm-4">
                    <input v-model="assessment" :class="{ 'is-invalid': isAssessmentInvalid }" id="assessment" class="form-control" placeholder="1w 2d 3h 40m" />
                    <small v-if="isAssessmentInvalid" class="invalid-feedback lh-sm"> Use format: 1w 2d 3h 40m<br />w = weeks<br />d = days<br />h = hours<br />m = minutes </small>
                  </div>
                  <div class="offset-sm-4 col-sm-8 mt-4">
                    <button @click="save" class="btn btn-primary">Save</button>
                  </div>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { PROJECT_SELECT } from '../store/projects.actions';
import { TASK_GET, TASK_EDIT } from '../store/tasks.actions';
import PriorityControl from '../components/priorityControl';
import DatePicker from '../components/datePicker';
import { Discussion, InWork, Completed } from '../services/taskStatuses';

const taskStatuses = [
  { value: Discussion, text: 'Discussion', className: 'text-warning' },
  { value: InWork, text: 'In work', className: 'text-primary' },
  { value: Completed, text: 'Completed', className: 'text-success' },
];

export default {
  name: 'Task',
  components: { PriorityControl, DatePicker },
  props: {
    id: String,
  },
  data() {
    return {
      taskStatuses,
      name: '',
      status: '',
      description: '',
      deadline: null,
      priority: '',
      assessment: '',
    };
  },
  computed: {
    task() {
      return this.$store.state.tasks.tasks.find((x) => x.id === this.id);
    },
    taskName() {
      return this.task ? this.task.name : '';
    },
    isAssessmentInvalid() {
      return this.assessment.length > 0 && !/^((\d+[wdhm])\s?)+$/.test(this.assessment);
    },
    statusSelectClass() {
      switch (this.status) {
        case Discussion:
          return 'text-warning';
        case InWork:
          return 'text-primary';
        case Completed:
          return 'text-success';
        default:
          return '';
      }
    },
  },
  async mounted() {
    await this.$store.dispatch(TASK_GET, { id: this.id });
    await this.$store.dispatch(PROJECT_SELECT, { id: this.task.projectId });

    setTimeout(() => {
      this.name = this.task.name;
      this.status = this.task.status;
      this.description = this.task.description;
      this.deadline = this.task.deadline;
      this.priority = this.task.priority;
      this.assessment = this.task.assessment;
    });
  },
  methods: {
    isFormValid() {
      return this.name && this.status && this.deadline && !this.isAssessmentInvalid;
    },
    async save() {
      if (!this.isFormValid()) return;

      const model = {
        id: this.id,
        assessment: this.assessment,
        deadline: this.deadline,
        description: this.description,
        name: this.name,
        priority: this.priority,
      };

      await this.$store.dispatch(TASK_EDIT, model);
    },
  },
};
</script>
