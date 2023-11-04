<script setup lang="ts">

import {TaskNode, TaskStatus} from "@/misc/types";
import {computed, ref, watch} from "vue";
import {isValidStatusTransition, treeVisitor} from "@/misc/tasks";


interface ITotalSubtaskData {
  completed: boolean,
  plannedEffort: number,
  actualEffort: number | null,
  timeTaken: number | null,
}

enum TaskField {
  PlannedEffort, PlannedEffortForSubtasks, PlannedEffortWithSubtasks,
  ActualEffort, ActualEffortForSubtasks, ActualEffortWithSubtasks,
  TimeTaken, TimeTakenForSubtasks, TimeTakenWithSubtasks,
}


const props = defineProps<{
  task: TaskNode,
  updateTaskCallback: (originalTask: TaskNode, newTask: TaskNode) => void
}>();

const editedTask = ref(makeTaskShallowCopy(props.task));
watch(() => props.task, task => {
  editedTask.value = makeTaskShallowCopy(task);
});

const totalSubtaskData = computed((): ITotalSubtaskData => {
  // recursively recalculate completion time for all the subtasks 
  let totalTimeTaken = 0, totalPlannedEffort = 0, totalActualEffort = 0;
  let allTasksFinished = true;
  treeVisitor(props.task.subtasks, (subtask, _) => {
    totalPlannedEffort += subtask.plannedTime;
    if (subtask.status === TaskStatus.Completed) {
      totalTimeTaken += getTimeTaken(subtask);
      totalActualEffort += subtask.actualTime;
    } else {
      allTasksFinished = false;
    }
  });

  return {
    completed: allTasksFinished,
    plannedEffort: totalPlannedEffort,
    actualEffort: totalActualEffort,
    timeTaken: totalTimeTaken,
  };
});


function makeTaskShallowCopy(task: TaskNode): TaskNode {
  const cloneDate = (date: Date) => new Date(date.getTime());

  return {
    ...task,
    creationDate: cloneDate(task.creationDate),
    completionDate: task.completionDate !== null ? cloneDate(task.completionDate) : null,
    lastStatusChangeDate: cloneDate(task.lastStatusChangeDate),
  };
}


function onSubmit(_e: InputEvent) {
  props.updateTaskCallback(props.task, makeTaskShallowCopy(editedTask.value));
}

function onInputTaskStatus(e: InputEvent) {
  // when finishing, fill in the completion date
  let newStatusStr = (e.target as HTMLSelectElement).value;
  let newStatus = parseInt(newStatusStr) as TaskStatus;
  editedTask.value.completionDate = newStatus == TaskStatus.Completed ? new Date() : null;
}

function onInputCreationDate(e: InputEvent) {
  let newDateStr = (e.target as HTMLInputElement).value;
  if (newDateStr.length > 0)
    editedTask.value.creationDate = new Date(newDateStr);
}

function onInputCompletionDate(e: InputEvent) {
  let newDateStr = (e.target as HTMLInputElement).value;
  if (newDateStr.length > 0)
    editedTask.value.completionDate = new Date(newDateStr);
}


function isTaskFinished(task: TaskNode) {
  return task.status === TaskStatus.Completed
}

function getTimeTaken(task: TaskNode) {
  return task.completionDate!.getTime() - task.creationDate.getTime();
}

function formatDateAndTime(date: Date | null) {
  if (date === null) {
    return "Not Yet Completed";
  } else {
    // from "YYYY-MM-DDTHH:mm:ss.sssZ" to "YYYY-MM-DDTHH:mm:ss" (local timezone)
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 19);
  }
}

function formatDuration(durationMs: number) {
  const seconds = Math.floor((durationMs / 1000) % 60),
      minutes = Math.floor((durationMs / (1000 * 60)) % 60),
      hours = Math.floor((durationMs / (1000 * 60 * 60)) % 24),
      days = Math.floor(durationMs / (1000 * 60 * 60 * 24));

  const hoursStr = (hours < 10) ? "0" + hours : hours;
  const minutesStr = (minutes < 10) ? "0" + minutes : minutes;
  const secondsStr = (seconds < 10) ? "0" + seconds : seconds;
  const daysStr = days >= 1 ? `${days}d ` : "";

  return `${daysStr}${hoursStr}:${minutesStr}:${secondsStr}`;
}

function formatField(field: TaskField) {
  const getDurationAsHours = (durationMs: number) => {
    const hours = durationMs / (1000 * 60 * 60);
    return Math.round(hours * 100) / 100;
  };

  const isFinished = props.task.status === TaskStatus.Completed;
  const areSubtasksFinished = totalSubtaskData.value.completed;
  const hasSubtasks = props.task.subtasks.length > 0;

  switch (field) {
    case TaskField.PlannedEffort:
      return `${props.task.plannedTime} hour(s)`;
    case TaskField.PlannedEffortForSubtasks:
      if (!hasSubtasks)
        return '-';
      return `${totalSubtaskData.value.plannedEffort} hour(s)`;
    case TaskField.PlannedEffortWithSubtasks:
      return `${props.task.plannedTime + totalSubtaskData.value.plannedEffort} hour(s)`;

    case TaskField.ActualEffort: {
      if (!isFinished)
        return "Not Yet Finished";
      const actualEffort = getDurationAsHours(props.task.actualTime);
      return `${actualEffort} hour(s)`;
    }
    case TaskField.ActualEffortForSubtasks: {
      if (!hasSubtasks)
        return '-';
      const duration = getDurationAsHours(totalSubtaskData.value.actualEffort!);
      return areSubtasksFinished ? `${duration} hour(s)` : "Some Subtasks Aren't Finished";
    }
    case TaskField.ActualEffortWithSubtasks: {
      if (!areSubtasksFinished)
        return "Some Subtasks Aren't Finished";
      const duration = getDurationAsHours(props.task.actualTime + totalSubtaskData.value.actualEffort!);
      return isFinished ? `${duration} hour(s)` : "Not Yet Finished";
    }

    case TaskField.TimeTaken: {
      if (!isFinished)
        return "Not Yet Finished";
      const timeTaken = getTimeTaken(props.task);
      return `${formatDuration(timeTaken)}`;
    }
    case TaskField.TimeTakenForSubtasks: {
      if (!hasSubtasks)
        return '-';
      return areSubtasksFinished ?
          formatDuration(totalSubtaskData.value.timeTaken!)
          : "Some Subtasks Aren't Finished";
    }
    case TaskField.TimeTakenWithSubtasks: {
      if (!areSubtasksFinished)
        return "Some Subtasks Aren't Finished";
      return (isFinished
          ? formatDuration(getTimeTaken(props.task) + totalSubtaskData.value.timeTaken!)
          : "Not Yet Finished");

    }
  }
}

</script>

<template>

  <div id="task-edit-view" class="container overflow-hidden overflow-y-auto">
    <div class="d-flex flex-column justify-content-center align-items-center">
      <h3>Task Edit Form</h3>
      <small class="text-muted">(ID: {{ task.id }}; Parent ID: {{ task.parentId ?? "None (Root Task)" }})</small>
    </div>

    <form id="taskNodeForm">
      <fieldset>
        <div class="row mb-1">
          <label for="task-name" class="col-sm-2 col-form-label">Name:</label>
          <div class="col-sm-10">
            <input type="text" id="task-name" class="col-sm-10 form-control" v-model="editedTask.name"
                   :style="{backgroundColor: task.name !== editedTask.name ? '#ffe' : ''}">
          </div>
        </div>

        <div class="row mb-1">
          <label for="task-details" class="col-sm-2 col-form-label">Details: </label>
          <div class="col-sm-10">
            <textarea id="task-details" rows="4" cols="50" class="form-control"
                      v-model="editedTask.details"
                      :style="{backgroundColor: task.details !== editedTask.details ? '#ffe' : ''}"
            ></textarea>
          </div>
        </div>

        <div class="row mb-1">
          <label for="task-assignees" class="col-sm-2 col-form-label">Assignees: </label>
          <div class="col-sm-10">
            <input type="text" id="task-assignees" class="form-control" v-model="editedTask.assignees"
                   :style="{backgroundColor: task.assignees !== editedTask.assignees ? '#ffe' : ''}">
          </div>
        </div>

        <div class="row mb-1">
          <label for="task-status" class="col-sm-2 col-form-label">Status: </label>
          <div class="col-sm-10">
            <select id="task-status" class="form-select" v-model.number="editedTask.status"
                    @input="onInputTaskStatus"
                    :style="{backgroundColor: task.status !== editedTask.status ? '#ffe' : ''}">
              <option v-if="isValidStatusTransition(task.status, TaskStatus.NotStarted)" value="0">Assigned</option>
              <option v-if="isValidStatusTransition(task.status, TaskStatus.InProgress)" value="1">In Progress</option>
              <option v-if="isValidStatusTransition(task.status, TaskStatus.Suspended)" value="2">Suspended</option>
              <option v-if="isValidStatusTransition(task.status, TaskStatus.Completed)" value="3">Completed</option>
            </select>
          </div>
        </div>

        <div class="row mb-1">
          <label for="task-planned-time" class="col-sm-4 col-form-label">Planned Effort
            <small class="text-muted"> (in hours) </small>
          </label>
          <div class="col-sm-8">
            <input type="number" id="task-planned-time" class="form-control" min="0"
                   v-model="editedTask.plannedTime"
                   :style="{backgroundColor: task.plannedTime !== editedTask.plannedTime ? '#ffe' : ''}">
          </div>
        </div>

        <div class="row mb-1">
          <label for="task-creation-date" class="col-sm-4 col-form-label">Creation Date: </label>
          <div class="col-sm-8">
            <input id="task-creation-date" type="datetime-local" step="1" class="form-control"
                   :value="formatDateAndTime(editedTask.creationDate)"
                   @input="onInputCreationDate"
                   :style="{backgroundColor: task.creationDate.getTime() !== editedTask.creationDate.getTime() ? '#ffe' : ''}"
            />
          </div>
        </div>

        <div class="row mb-1">
          <label for="task-completion-date" class="col-sm-4 col-form-label"> Completion Date: </label>
          <div class="col-sm-8">
            <input id="task-completion-date" type="datetime-local" step="1" class="form-control"
                   :value="isTaskFinished(editedTask) ? formatDateAndTime(editedTask.completionDate) : ''"
                   :disabled="!isTaskFinished(editedTask)"
                   @input="onInputCompletionDate"
                   :style="{backgroundColor: task.completionDate?.getTime() !== editedTask.completionDate?.getTime() ? '#ffe' : ''}"
            />
          </div>
        </div>


        <hr>
        <table class="table">
          <thead>
          <tr>
            <th>Time Statistics</th>
            <th>Task</th>
            <th>Subtasks</th>
            <th>Task + Subtasks</th>
          </tr>
          </thead>
          <tbody>
          <tr>
            <td>Planned Effort</td>
            <td>{{ formatField(TaskField.PlannedEffort) }}</td>
            <td>{{ formatField(TaskField.PlannedEffortForSubtasks) }}</td>
            <td>{{ formatField(TaskField.PlannedEffortWithSubtasks) }}</td>
          </tr>
          <tr>
            <td>Actual Effort</td>
            <td>{{ formatField(TaskField.ActualEffort) }}</td>
            <td>{{ formatField(TaskField.ActualEffortForSubtasks) }}</td>
            <td>{{ formatField(TaskField.ActualEffortWithSubtasks) }}</td>
          </tr>
          <tr>
            <td>Time Taken</td>
            <td>{{ formatField(TaskField.TimeTaken) }}</td>
            <td>{{ formatField(TaskField.TimeTakenForSubtasks) }}</td>
            <td>{{ formatField(TaskField.TimeTakenWithSubtasks) }}</td>
          </tr>

          </tbody>

        </table>

        <button type="submit" class="btn btn-sm btn-primary col-sm-12" @click.prevent="onSubmit">Apply Changes</button>
      </fieldset>
    </form>

  </div>

</template>

<style scoped>

#task-edit-view {
  border-radius: 5px;
  padding: 5px;

  min-width: 50%;
  height: 95vh;
}

</style>