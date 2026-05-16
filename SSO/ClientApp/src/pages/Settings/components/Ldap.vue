<template>
    <div class="row g-4 settings-section">
        <div class="col-12 col-md-3">
            <h3 class="section-title">&nbsp;</h3>
            <div class="section-intro">Use <i>Lightweight Directory Access Protocol (LDAP)</i> to authenticate and
                manage users.</div>
        </div>
        <div class="col-12 col-md-9">
            <div class="app-card app-card-settings shadow-sm p-4">
                <div class="app-card-body">
                    <form class="settings-form" @submit.prevent="onSubmit" ref="form">
                        <div class="alert alert-warning mt-2" role="alert">
                            <b>Warning:</b> Ensure that you create an admin user with the correct LDAP credentials first. Use this user to log in to the LDAP domain to verify proper access and functionality.
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Server*</label>
                            <input class="form-control" placeholder="yourdomain.com" required autocomplete="off"
                                v-model="ldapSettings.server" />
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Port*</label>
                            <input class="form-control" type="number" placeholder="389" required autocomplete="off"
                                v-model="ldapSettings.port" />
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Username*</label>
                            <input class="form-control" placeholder="Username" required autocomplete="off"
                                v-model="ldapSettings.username" />
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Password*</label>
                            <input class="form-control" type="password" required autocomplete="off"
                                v-model="ldapSettings.password" />
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Search base*</label>
                            <input class="form-control" autocomplete="off" placeholder="dc=example,dc=com" required
                                v-model="ldapSettings.searchBase" />
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Search Filter</label>
                            <input class="form-control" autocomplete="off" placeholder="(objectClass=*)"
                                v-model="ldapSettings.searchFilter" />
                        </div>
                        <div class="form-check mb-3">
                            <input class="form-check-input" type="checkbox" value="" v-model="ldapSettings.useSSL">
                            <label class="form-label form-check-label" for="settings-checkbox-1">
                                Use SSL
                            </label>
                        </div>
                        <div class="row">
                            <div class="col-auto mt-2">
                                <button type="submit" class="btn app-btn-primary">Save Changes</button>&nbsp;
                                <button type="button" class="btn app-btn-outline-primary" @click="onTest()">Test
                                    Settings</button>
                            </div>
                            <div class="col-auto ms-auto mt-2">
                                <button type="button" class="btn btn-danger" v-show="showDeleteButton"
                                    @click="onDelete()">Remove</button>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { emitter } from "@/services/emitter.service";
import { modifyLdapSetting, deleteLdapSetting, testLdapSetting } from "@/services/realm-idp-settings.service";
export default {
    props: ["realm"],
    data: () => ({
        ldapSettings: new Object(),
        showDeleteButton: false
    }),
    watch: {
        realm(newVal, oldVal) {
            let res = newVal.realmIdpSettingsCollection
                .filter(x => x.identityProvider === 1);

            this.ldapSettings = res.length > 0 ? res[0].value : new Object();
            this.showDeleteButton = res.length > 0;
        }
    },
    methods: {
        onSubmit() {
            emitter.emit("showLoader", true);

            modifyLdapSetting(this.ldapSettings).then(r => {
                emitter.emit("showLoader", false);
                this.showDeleteButton = true;
                alert('Saved!');
            }, (err) => {
                alert('Failed to update record.');
                emitter.emit("showLoader", false);
            });
        },
        onDelete(id) {
            if (confirm('Are you sure you want to delete this record?')) {
                emitter.emit("showLoader", true);
                deleteLdapSetting().then(r => {
                    window.location.reload();
                }, err => {
                    emitter.emit("showLoader", false);
                });
            }
        },
        onTest() {
            if (this.$refs.form.reportValidity()) {
                emitter.emit("showLoader", true);
                testLdapSetting(this.ldapSettings).then(r => {
                    emitter.emit("showLoader", false);
                    alert('Connected!');
                }, (err) => {
                    emitter.emit("showLoader", false);
                    alert('Failed to connect.');
                });
            }
        }
    }
}
</script>

<style scoped>
@media (max-width: 768px) {

    /* Adjust the max-width as needed for your mobile breakpoint */
    .row {
        display: flex;
        flex-direction: column;
        align-items: flex-start;
    }

    .col-auto {
        width: 100%;
    }
}
</style>