<template>
	<div class="col-12 col-md-12 col-lg-12 auth-main-col text-center p-5">
		<div class="d-flex flex-column align-content-end">
			<div class="app-auth-body mx-auto">
				<div class="app-auth-branding mb-4"><a class="app-logo" href="#"><img class="logo-icon"
							:src="require('@/assets/logo.png')" alt="logo"></a></div>
				<h2 class="auth-heading text-center mb-3">Welcome</h2>
				<p class="mb-4">Login to SSO to continue to system.</p>
				<div class="auth-form-container text-start">
					<form class="auth-form login-form" @submit.prevent="submit()">
						<div class="mb-3">
							<input class="form-control signin-email" placeholder="Username" required="required"
								v-model="param.userName" autocomplete="off">
						</div><!--//form-group-->
						<div class="input-group mb-4">
							<input type="password" id="password" class="form-control" placeholder="Password"
								required="required" v-model="param.password" autocomplete="off">
							<button class="btn app-btn-outline-secondary" type="button" tabindex="-1"
								@click="toggleViewKey('password')">
								<i class="bi bi-eye"></i>
							</button>
						</div><!--//form-group-->
						<div class="text-center">
							<button type="submit" class="btn app-btn-primary w-100 theme-btn mx-auto">Log In</button>
						</div>
						<div class="text-center mt-3">
							<router-link :to="{ name: 'ForgotPassword' }" class="text-muted">Forgot password?</router-link>
						</div>
					</form>
				</div><!--//auth-form-container-->

			</div><!--//auth-body-->
		</div><!--//flex-column-->
	</div><!--//auth-main-col-->
	<Otp :param="param" :system="true"/>

</template>

<script>
import Otp from "@/components/Otp.vue";
import { loginToSystem } from "@/services/authentication.service";
import Cookies from 'js-cookie';
import { emitter } from '@/services/emitter.service';

export default {
	components: {
		Otp
	},
	data: () => ({
		param: new Object(),
		realm: null,
		modal: null,
	}),
	created() {
		document.title = 'Login | System';

		const params = new URLSearchParams(window.location.search);
		this.realm = params.get('realm');
	},

	mounted() {
		// Listen for the hidden.bs.modal event and call onModalClose method
		this.modal = new bootstrap.Modal(document.getElementById('otpModal'));
		this.modal._element.addEventListener('hidden.bs.modal', this.onModalClose);

		if (Cookies.get('system')) {
			window.location.href = 'init';
		}
	},
	methods: {
		submit() {

			emitter.emit('showLoader', true);

			this.param.realmId = this.realm;
			loginToSystem(this.param).then(r => {
				emitter.emit('showLoader', false);
				if (r.status === 202) {
					this.modal.show();
				}
				else {
					Cookies.set('system', r.data.access_token, { expires: 1 });
					this.$router.replace({ name: 'Init' });
				}
			}, err => {
				emitter.emit('showLoader', false);
				alert(err.response.data.error);
			});
		},
		toggleViewKey(param) {
			var e = document.getElementById(param);
			if (e.type === "password") {
				e.type = "text";
			} else {
				e.type = "password";
			}
		},

		onModalClose() {
            location.reload();
        },
	}
}
</script>