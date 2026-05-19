<template>
    <div class="row g-4 settings-section">
        <div class="col-12 col-md-3">
            <h3 class="section-title">&nbsp;</h3>
            <div class="section-intro">In a mailer (e.g., forgot password), <i>Simple Mail Transfer Protocol (SMTP)</i>
                is used to send the email containing the password reset link or verification code to the user’s inbox.
            </div>
        </div>
        <div class="col-12 col-md-9">
            <div class="app-card app-card-settings shadow-sm p-4">
                <div class="app-card-body">
                    <form class="settings-form" @submit.prevent="onSubmit" ref="form">
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">SMTP Server*</label>
                            <input class="form-control" required autocomplete="off" v-model="mailSettings.smtpServer" />
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Port*</label>
                            <input class="form-control" required autocomplete="off" v-model="mailSettings.port" type="number"/>
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Username*</label>
                            <input class="form-control" required autocomplete="off" v-model="mailSettings.username" type="email"/>
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">Password*</label>
                            <input class="form-control" required autocomplete="off" v-model="mailSettings.password" type="password"/>
                        </div>
                        <div class="form-check mb-3">
                            <input class="form-check-input" type="checkbox" value="" v-model="mailSettings.enableSsl">
                            <label class="form-label form-check-label" for="settings-checkbox-1">
                                Enable SSL
                            </label>
                        </div>
                        <div class="mb-3">
                            <label for="setting-input-2" class="form-label">To Email</label>
                            <input class="form-control" autocomplete="off" v-model="toEmail" type="email"/>
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
import { modifyMailerSetting, deleteMailerSetting, testMailerSetting, getMailerSetting } from "@/services/realm-mailer.service";

export default {
    data: () => ({
        mailSettings: new Object(),
        toEmail: '',
        showDeleteButton: false
    }),
    mounted() {
        getMailerSetting().then(r => {
            if (r.data != '') {
                this.mailSettings = r.data;
                this.showDeleteButton = true;
            }
        });
    },
    methods: {
        onSubmit() {
            emitter.emit("showLoader", true);

            modifyMailerSetting({ settings: this.mailSettings }).then(r => {
                emitter.emit("showLoader", false);
            }, (err) => {
                alert('Failed to update record.');
                emitter.emit("showLoader", false);
            });
        },
        onTest() {
            emitter.emit("showLoader", true);

            testMailerSetting({ settings: this.mailSettings, toEmail: this.toEmail }).then(r => {
                emitter.emit("showLoader", false);
                alert('Test email sent! Please check your inbox.');
            }, (err) => {
                alert('Failed to send test email. Please check your settings and try again.');
                emitter.emit("showLoader", false);
            });
         },
         onDelete() {
            if (confirm('Are you sure you want to delete this record?')) {
                emitter.emit("showLoader", true);
                deleteMailerSetting().then(r => {
                    window.location.reload();
                }, err => {
                    emitter.emit("showLoader", false);
                });
            }
        },
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