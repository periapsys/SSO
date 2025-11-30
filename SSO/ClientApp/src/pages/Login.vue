<template>
	<div class="col-12 col-md-12 col-lg-12 auth-main-col text-center p-5">
		<div class="d-flex flex-column align-content-end">
			<div class="app-auth-body mx-auto">
				<div class="app-auth-branding mb-4"><a class="app-logo" href="#"><img class="logo-icon"
							:src="require('@/assets/logo.png')" alt="logo"></a></div>
				<h2 class="auth-heading text-center mb-3">Welcome</h2>
				<p class="mb-4">Login to SSO to continue to {{ app }}.</p>
				<div class="auth-form-container text-start">
					<form class="auth-form login-form" @submit.prevent="submit()">
						<div class="mb-3">
							<input class="form-control signin-email" placeholder="Username" required="required"
								v-model="param.userName" autocomplete="off">
						</div><!--//form-group-->
						<div class="input-group mb-4">
							<input type="password" id="password" class="form-control" placeholder="Password"
								required="required" v-model="param.password" autocomplete="off">
							<button class="btn btn-outline-secondary" type="button" tabindex="-1"
								@click="toggleViewKey('password')">
								<i class="bi bi-eye"></i>
							</button>
						</div><!--//form-group-->
						<div class="text-center">
							<button type="submit" class="btn app-btn-primary w-100 theme-btn mx-auto">Log In</button>
						</div>
					</form>
				</div><!--//auth-form-container-->

			</div><!--//auth-body-->
		</div><!--//flex-column-->
	</div><!--//auth-main-col-->
	<Otp :param="param" />
</template>

<script>
import Otp from "@/components/Otp.vue";
import { login } from "@/services/authentication.service";
import { getAppById } from "@/services/application.service";
import Cookies from 'js-cookie';
import { emitter } from '@/services/emitter.service';

export default {
	components: {
		Otp
	},
	data: () => ({
		param: new Object(),
		urlParams: new URLSearchParams(window.location.search),
		app: '---',
	}),
	created() {
		if (!this.urlParams.get('appId')
			|| !this.urlParams.get('callbackUrl')
			|| !Cookies.get('appId')
			|| this.urlParams.get('appId') != Cookies.get('appId')) {
			window.location.href = "invalid";
		}

		getAppById(this.urlParams.get('appId')).then(r => {
			this.app = r.data.name;
			document.title = 'Login | ' + r.data.name;
		});
	},
	mounted() {
		// Listen for the hidden.bs.modal event and call onModalClose method
		this.modal = new bootstrap.Modal(document.getElementById('otpModal'));
		this.modal._element.addEventListener('hidden.bs.modal', this.onModalClose);

		this.param.applicationId = this.urlParams.get('appId');

		if (Cookies.get('token')) {
			var token = Cookies.get('token');

            window.location.href = `${this.urlParams.get('callbackUrl')}?code=${token}&state=${this.urlParams.get('state')}`;
		}
	},
	methods: {
		submit() {
			emitter.emit('showLoader', true);

			login(this.param).then(r => {
				emitter.emit('showLoader', false);
				if (r.status === 202) {
					this.modal.show();
				}
				else {
					if (this.urlParams.get('callbackUrl')) {
                        window.location.href = `${this.urlParams.get('callbackUrl')}?code=${r.data.id}&state=${this.urlParams.get('state')}`;
					}
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