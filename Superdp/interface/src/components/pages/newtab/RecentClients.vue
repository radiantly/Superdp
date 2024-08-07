<script setup>
import { computed, inject } from "vue";
import { UseTimeAgo } from "@vueuse/components";
import { clientManager } from "../../../globals";
import { Client } from "../../../classes/Client";
import { Tab } from "../../../classes/Tab";
import { tabManagerKey } from "../../../keys";

const tabManager = inject(tabManagerKey);

const recentClients = computed(() =>
  [...clientManager.clients.values()]
    .filter((client) => client.lastConnected !== null)
    .sort(Client.LastConnectedComparator)
);

const handleClick = (e, client) => {
  const tab = new Tab({ client });
  client.addTab(tabManager, tab);
};
</script>
<template>
  <div class="recent-box">
    <div class="recent-header">Recent</div>
    <div class="recent-cards">
      <div
        v-for="client in recentClients"
        @click="(e) => handleClick(e, client)"
      >
        <div class="label">
          {{ client.label }}
        </div>
        <UseTimeAgo v-slot="{ timeAgo }" :time="client.lastConnected">
          <div class="info">{{ timeAgo }}</div>
        </UseTimeAgo>
      </div>
    </div>
  </div>
</template>
<style scoped>
.recent-box {
  display: flex;
  flex-direction: column;
  padding: 30px 50px;
  gap: 8px;
}
.recent-header {
  text-transform: uppercase;
  color: var(--light);
  font-weight: 700;
  user-select: none;
  font-size: 11px;
}

.recent-cards {
  display: flex;
  user-select: none;
  gap: 8px;
  flex-wrap: wrap;
}

.recent-cards > div {
  display: flex;
  flex-direction: column;
  padding: 5px 10px;
  background-color: var(--darker-gray);
  border-radius: 2px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.recent-cards > div:hover {
  background-color: var(--darke-gray);
}

.recent-cards .label {
  color: var(--light);
}
.recent-cards .info {
  color: var(--lighter-gray);
  font-style: italic;
  font-size: 11px;
}
</style>
