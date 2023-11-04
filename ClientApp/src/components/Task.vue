<script setup lang="ts">
import {Ref, ref} from "vue";
import {TaskNode, TaskStatus} from "@/misc/types";

const props = defineProps<{
  depth: number
  task: TaskNode
  editedTask: TaskNode | null
  removeCallback: (task: TaskNode) => void
  addSubtaskCallback: (parentTask: TaskNode) => void
  selectTaskForEditCallback: (task: TaskNode) => void
}>();


const subtaskPixelOffset = 25;


function getIconNameAndColor(): { icon: string, color: string } {
  let res = ["", ""];
  switch (props.task.status) {
    case TaskStatus.NotStarted:
      res = ["bi-circle", "#000"];
      break;
    case TaskStatus.InProgress:
      res = ["bi-play-circle-fill", "#ff0766"];
      break;
    case TaskStatus.Suspended:
      res = ["bi-pause-circle-fill", "#6b6dfb"];
      break;
    case TaskStatus.Completed:
      res = ["bi-check-circle-fill", "#4CAF50"];
      break;
  }
  return {icon: res[0], color: res[1]};
}

</script>

<template>
  <div>

    <div class="task d-inline-flex justify-content-between" @click="() => selectTaskForEditCallback(task)">

      <span class="task-name">
        <i :class="['bi', getIconNameAndColor().icon]"
           :style="{ color: getIconNameAndColor().color, marginLeft: '1px', fontSize: '1.25rem' }"
        ></i>
        {{ task.id }} | {{ task.name }}</span>

      <div class="btn-group">
        <button @click.stop="() => removeCallback(task)" class="btn btn-danger btn-sm" title="Remove the task"
                :disabled="task.subtasks.length !== 0">
          -
        </button>
        <button @click.stop="() => addSubtaskCallback(task)" class="btn btn-success btn-sm" title="Add a subtask">
          +
        </button>
      </div>
    </div>

    <div v-for="childrenTask in task.subtasks">
      <Task
          :style="{ marginLeft: `${subtaskPixelOffset}px`}"
          :depth="depth + 1"
          :task="childrenTask"
          :key="childrenTask.id"
          :editedTask="editedTask"
          :removeCallback="removeCallback"
          :addSubtaskCallback="addSubtaskCallback"
          :selectTaskForEditCallback="selectTaskForEditCallback"
      />
    </div>

  </div>
</template>

<style scoped>

.task {
  width: 350px;
  padding-left: 5px;
  border: 1px grey solid;
  border-radius: 10px 4px 4px 10px;

  background-color: v-bind("editedTask?.id === task.id ? '#a0a0a040' : 'initial'");
}

.task:hover {
  background-color: #a0a0a080;
  cursor: pointer;
}

.task-name {
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}


</style>