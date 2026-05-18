<template>
  <div class="app-content pt-3 p-md-3 p-lg-4">
    <div class="container-xl pt-5">
      <div class="row g-3 mb-4 align-items-center justify-content-between">
        <div class="col-auto">
          <h1 class="app-page-title mb-0">Settings</h1>
        </div>
      </div>
      <hr class="mb-4" />

      <div class="accordion" id="accordionExample">
        <div class="accordion-item">
          <h2 class="accordion-header">
            <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapseOne"
              :aria-expanded="openPanel === 'collapseOne'" aria-controls="collapseOne" @click="togglePanel('collapseOne')" :style="buttonStyle('collapseOne')">
              LDAP
            </button>
          </h2>
          <div id="collapseOne" class="accordion-collapse collapse" data-bs-parent="#accordionExample">
            <div class="accordion-body">
              <Ldap :realm="realm" />
            </div>
          </div>
        </div>
        <div class="accordion-item">
          <h2 class="accordion-header">
            <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse"
              data-bs-target="#collapseTwo" :aria-expanded="openPanel === 'collapseTwo'" aria-controls="collapseTwo" @click="togglePanel('collapseTwo')" :style="buttonStyle('collapseTwo')">
              Mailer
            </button>
          </h2>
          <div id="collapseTwo" class="accordion-collapse collapse" data-bs-parent="#accordionExample">
            <div class="accordion-body">
              <Malier />
            </div>
          </div>
        </div>        
      </div>
    </div>
  </div>
</template>
<script>

import * as navbar from "@/services/navbar.service";
import { emitter } from "@/services/emitter.service";
import { getCurrentRealm } from "@/services/realm.service";
import Ldap from "@/pages/Settings/components/Ldap.vue";
import Malier from "./components/Malier.vue";
export default {
  components: {
    Ldap,
    Malier
  },
  data: () => ({
    realm: new Object(),
    openPanel: ''
  }),
  async mounted() {
    navbar.init(this.$route);

    emitter.emit("showLoader", true);

    this.realm = (await getCurrentRealm()).data;

    emitter.emit("showLoader", false);
  },
  methods: {
    togglePanel(panelId) {
      this.openPanel = this.openPanel === panelId ? '' : panelId;
    },
    buttonStyle(panelId) {
      const selected = this.openPanel === panelId;
      return {
        outline: 'none',
        boxShadow: 'none',
        backgroundColor: selected ? '#fff3cd' : null,
        color: selected ? '#7a4d00' : null,
        borderColor: selected ? '#ffe69c' : null
      };
    }
  }
};
</script>