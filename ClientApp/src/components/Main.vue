<script setup lang="ts">
import {onMounted, Ref, ref} from 'vue';
import Task from "@/components/Task.vue";
import {TaskNode, TaskStatus} from "@/misc/types";
import {addNewTask, deleteTask, finishTaskTree, getAllTasks, updateTask} from "@/misc/api";
import {useToast} from "vue-toast-notification";
import {isValidStatusTransition, tasksFilter, treeVisitor} from "@/misc/tasks";
import TaskEditor from "@/components/TaskEditor.vue";


const rootTasks: Ref<TaskNode[]> = ref([]);
const editedTask: Ref<TaskNode | null> = ref(null);
const isLoadingTasks = ref(false);
const toast = useToast();


onMounted(async () => {
  isLoadingTasks.value = true;
  let tasks = await getAllTasks();
  isLoadingTasks.value = false;

  rootTasks.value = tasks;
  if (tasks.length > 0)
    editedTask.value = tasks[0];
});

function selectTaskForEditCallback(task: TaskNode) {
  editedTask.value = task;
}

async function removeTaskCallback(task: TaskNode) {
  const isOk = await deleteTask(task.id);
  if (isOk) {
    rootTasks.value = tasksFilter(rootTasks.value, (t) => t.id !== task.id);
    if (editedTask.value !== null && editedTask.value.id === task.id)
      editedTask.value = null;
  } else if (task.subtasks.length > 0) {
    toast.error(`Can't remove a task that has subtasks`);
  } else {
    toast.error(`Unknown deletion error`);
  }
}

async function addSubtaskCallback(parentTask: TaskNode | null) {
  let mbNewTask = await addNewTask(parentTask?.id ?? null);
  if (mbNewTask === null) {
    toast.error("Unknown insertion error");
  } else {
    // update the view state
    if (parentTask !== null)
      parentTask.subtasks.push(mbNewTask);
    else
      rootTasks.value.push(mbNewTask);
    editedTask.value = mbNewTask;
  }
}

async function updateTaskCallback(oldTask: TaskNode, newTask: TaskNode) {
  const currentTime = new Date();

  const isChangingStatus = oldTask.status !== newTask.status;
  const isFinishing = isChangingStatus && newTask.status === TaskStatus.Completed;
  if (isFinishing) {
    // check if we can change the status to "Completed" for all the nodes in the tree
    const unfinishableTasks: TaskNode[] = [];
    treeVisitor([newTask], (subtask, _) => {
      if (!isValidStatusTransition(subtask.status, TaskStatus.Completed))
        unfinishableTasks.push(subtask);
    });

    if (unfinishableTasks.length === 0) {
      // change the status for all tasks
      newTask.completionDate = currentTime;

      const serverSucceeded = await finishTaskTree(newTask.id, newTask);
      if (serverSucceeded) {
        // update the Vue state 
        treeVisitor([newTask], (subtask, _) => {
          if (oldTask.status === TaskStatus.InProgress)
            subtask.actualTime += currentTime.getTime() - subtask.lastStatusChangeDate.getTime()

          subtask.status = TaskStatus.Completed;
          subtask.completionDate ??= currentTime;
          subtask.lastStatusChangeDate = currentTime;
        });
      } else {
        toast.error("Something went wrong with finishing the tree on the server");
        return;
      }
    } else {
      const task = unfinishableTasks[0];
      toast.error(`One of the subtasks (such as ${task.id}, ${task.name}) cannot be finished!`);
      return;
    }
  } else if (isChangingStatus) {
    if (oldTask.status === TaskStatus.InProgress)
      newTask.actualTime += currentTime.getTime() - newTask.lastStatusChangeDate.getTime()
    newTask.lastStatusChangeDate = currentTime;
  }

  const isOk = await updateTask(oldTask.id, newTask);
  if (isOk) {
    Object.assign(oldTask, newTask, {
      subtasks: oldTask.subtasks
    });
  }
}

</script>
<template>

  <div class="d-flex flex-row mb-3">

    <div v-if="isLoadingTasks">
      <img class="loading-spinner" src="favicon.ico"
           alt="just a spinner"/>
      <br>Loading...
    </div>

    <div v-if="!isLoadingTasks" class="task-tree border border-2 rounded m-1 col-4">
      <div class="overflow-auto ms-1 mt-1 h-100">
        <div v-for="rootTask in rootTasks">
          <Task :depth="0" :task="rootTask" :key="rootTask.id"
                :editedTask="editedTask"
                :removeCallback="removeTaskCallback"
                :addSubtaskCallback="addSubtaskCallback"
                :updateTaskCallback="updateTask"
                :selectTaskForEditCallback="selectTaskForEditCallback"
          />
        </div>
      </div>

      <button class="btn btn-sm btn-primary mb-3 col-sm-12" @click="() => addSubtaskCallback(null)">
        <i class="bi bi-plus-square-fill"></i> Add Root Task
      </button>
    </div>

    <div v-if="!isLoadingTasks && editedTask !== null" class="border border-2 rounded m-1 col-lg-auto col-xl-5">
      <TaskEditor :task="editedTask"
                  :updateTaskCallback="updateTaskCallback"
      />
    </div>

  </div>

</template>
<style scoped>

.task-tree {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 97vh;
}

.loading-spinner {
  margin: 15px;
  width: 50px;
  height: 50px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  100% {
    transform: rotate(360deg);
  }
}

</style>
