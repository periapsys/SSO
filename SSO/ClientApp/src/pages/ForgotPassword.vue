<template>
    <div class="col-12 col-md-12 col-lg-12 auth-main-col text-center p-5">
        <div class="d-flex flex-column align-content-end">
            <div class="app-auth-body mx-auto">
                <div class="app-auth-branding mb-4"><a class="app-logo" href="#"><img class="logo-icon"
                            :src="require('@/assets/logo.png')" alt="logo"></a></div>
                <h2 class="auth-heading text-center mb-3">Forgot Password</h2>
                <p class="mb-4">Please enter your email.</p>
                <div class="auth-form-container text-start">
                    <form class="auth-form login-form" @submit.prevent="submit()">
                        <div class="mb-3">
                            <input class="form-control signin-email" placeholder="Email" type="email"
                                required="required" v-model="email" autocomplete="off">
                        </div><!--//form-group-->                        

                        <!-- CAPTCHA -->
                        <div class="mb-3 text-center">
                            <img :src="captchaImage" alt="captcha" class="border rounded p-2 bg-light"
                                style="height:60px;" />
                        </div>

                        <div class="input-group mb-4">
                            <input type="text" class="form-control" placeholder="Enter captcha" required="required"
                                v-model="captchaAnswer" autocomplete="off">

                            <button class="btn btn-outline-secondary" type="button" @click="getCaptcha()">

                                <i class="bi bi-arrow-clockwise"></i>
                            </button>
                        </div>

                        <div class="text-center">
                            <button type="submit" class="btn app-btn-primary w-100 theme-btn mx-auto">Submit</button>
                            <div class="auth-option text-center pt-5">
                                <a class="text-link" href="#"@click.prevent="$router.back()">Back</a>
							</div>
                        </div>
                    </form>
                </div><!--//auth-form-container-->

            </div><!--//auth-body-->
        </div><!--//flex-column-->
    </div><!--//auth-main-col-->

</template>

<script>
import { loadCaptcha } from '@/services/captcha.service';
import { forgotPassword } from "@/services/authentication.service";
import { emitter } from '@/services/emitter.service';

export default {
    data: () => ({
        email: '',
        captchaImage: '',
        captchaId: '',
        captchaAnswer: ''
    }),
    created() {
        document.title = 'Forgot Password';
    },

    mounted() {
        this.getCaptcha();
    },
    methods: {
        submit() {
            emitter.emit('showLoader', true);
            var param = {
                email: this.email,
                captcha: {
                    id: this.captchaId,
                    answer: this.captchaAnswer
                }
            };
            forgotPassword(param).then(r => {
                alert('If this email is registered, you’ll receive a password reset link.');
                emitter.emit('showLoader', false);
                this.$router.back();
            }, err => {
                alert(err.response.data.error || 'An error occurred. Please try again.');
            }).finally(() => {
                emitter.emit('showLoader', false);
                this.getCaptcha();
            });
        },

        getCaptcha() {
            loadCaptcha().then(r => {
                this.captchaImage = r.data.image;
                this.captchaId = r.data.id;                
                this.captchaAnswer = '';
            });
        }
    }
}
</script>